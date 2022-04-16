using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.CSharp
{
    public class Cracker : ICracker
    {
        public Cracker()
        {

        }

        public Cracker(ILogger logger)
        {
            Logger = logger;
        }

        private readonly int PlainSize = 1 << 20;

        private CancellationTokenSource stopToken = new();

        public ILogger? Logger { get; private set; }

        public bool Paused => stopToken.IsCancellationRequested;

        public Task<KeyItem?> FindKeyAsync(string cipherFile, string cipherFileName, string plainFile, string plainFileName)
        {
            return Task.Factory.StartNew(() => FindKey(cipherFile, cipherFileName, plainFile, plainFileName), StartNew());
        }

        public KeyItem? FindKey(string cipherFile, string cipherFileName, string plainFile, string plainFileName)
        {
            using (var cipherStream = File.OpenRead(cipherFile))
            using (var plainStream = File.OpenRead(plainFile))
            {
                return FindKey(cipherStream, cipherFileName, plainStream, plainFileName);
            }
        }

        public KeyItem? FindKey(FileStream cipherStream, string cipherFileName, FileStream plainStream, string plainFileName)
        {
            try
            {
                if (!Zip.GetFileDataPosition(cipherStream, cipherFileName, out var cipherBegin, out var cipherEnd))
                {
                    return null;
                }
                if (Paused)
                {
                    return null;
                }
                if (!Zip.GetFileDataPosition(plainStream, plainFileName, out var plainBegin, out var plainEnd))
                {
                    return null;
                }
                if (Paused)
                {
                    return null;
                }
                return FindKey(cipherStream, cipherBegin, cipherEnd, plainStream, plainBegin, plainEnd);
            }
            catch (Exception ex)
            {
                Logger?.Error(ex.Message);
                return null;
            }
        }

        public KeyItem? FindKey(string cipherFile, long cipherBegin, long cipherEnd, string plainFile, long plainBegin, long plainEnd)
        {
            using (var cipherStream = File.OpenRead(cipherFile))
            using (var plainStream = File.OpenRead(plainFile))
            {
                return FindKey(cipherStream, cipherBegin, cipherEnd, plainStream, plainBegin, plainEnd);
            }
        }

        public KeyItem? FindKey(FileStream cipherStream, long cipherBegin, long cipherEnd, FileStream plainStream, long plainBegin, long plainEnd)
        {
            var data = new CrackData();
            plainStream.Seek(plainBegin, SeekOrigin.Begin);
            var plainMinEnd = Math.Min(plainBegin + PlainSize, plainEnd);
            for (var i = plainBegin; i < plainMinEnd; i++)
            {
                data.PlainText.Add((byte)plainStream.ReadByte());
            }
            cipherStream.Seek(cipherBegin, SeekOrigin.Begin);
            for (var i = cipherBegin; i < cipherEnd; i++)
            {
                if (Paused)
                {
                    return null;
                }
                data.CipherText.Add((byte)cipherStream.ReadByte());
            }
            cipherStream.Close();
            plainStream.Close();
            return FindKey(data);
        }

        public KeyItem? FindKey(CrackData data)
        {
            if (data.PlainText.Count < Attack.SIZE)
            {
                // 太小了
                return null;
            }
            if (data.PlainText.Count > data.CipherText.Count)
            {
                // 不能小于
                return null;
            }
            data.Update();
            if (Paused)
            {
                return null;
            }
            var zr = new Zreduction(this, data.KeyStream);
            if (data.KeyStream.Count > Attack.SIZE)
            {
                Logger?.Info($"Z reduction using {data.KeyStream.Count - Attack.CONTIGUOUS_SIZE} bytes of known plaintext");
                zr.Reduce();
            }
            if (Paused)
            {
                return null;
            }
            zr.Generate();
            Logger?.Info($"Generated {zr.Count} Z values. ");
            if (Paused)
            {
                return null;
            }
            // iterate over remaining Zi[2,32) values
            var candidates = zr.ZiVector;
            var size = zr.Count;
            var done = 0;
            Logger?.Info($"Attack on {zr.Count}  Z values at index {data.Offset + zr.Index - CrackData.ENCRYPTION_HEADER_SIZE}");
            var attack = new Attack(data, zr.Index);
            var shouldStop = false;
            for (var i = 0; i < size; ++i) // OpenMP 2.0 requires signed index variable
            {
                if (shouldStop)
                {
                    break;
                }
                if (Paused)
                {
                    return null;
                }
                try
                {
                    attack.Carryout(candidates[i]);
                    {
                        if (Paused)
                        {
                            return null;
                        }
                        Logger?.Progress(++done, size);
                        shouldStop = attack.SolutionItems.Count > 0;
                    }
                }
                catch (Exception ex)
                {
                    Logger?.Error(ex.Message);
                    return null;
                }
            }
            // print the keys
            // std::cout << "[" << put_time << "] ";
            if (attack.SolutionItems.Count < 1)
            {
                Logger?.Info("Could not find the keys.");
                return null;
            }
            var sb = new StringBuilder();
            sb.AppendLine("Keys:");
            foreach (var item in attack.SolutionItems)
            {
                sb.AppendLine(item.ToString());
            }
            Logger?.Info(sb.ToString());
            return attack.SolutionItems[0];
        }


        public Task<KeyItem?> FindKeyAsync(string cipherFile, long cipherBegin, long cipherEnd, string plainFile, long plainBegin, long plainEnd)
        {
            return Task.Factory.StartNew(() => FindKey(cipherFile, cipherBegin, cipherEnd, plainFile, plainBegin, plainEnd), StartNew());
        }

        public Task<bool> UnpackAsync(KeyItem keys, string cipherFile, string distFolder)
        {
            return Task.Factory.StartNew(() => Unpack(keys, cipherFile, distFolder), StartNew());
        }

        public bool Unpack(KeyItem keys, string cipherFile, string distFolder)
        {
            var res = true;
            using (var fs = File.OpenRead(cipherFile))
            {
                var items = Zip.GetEntries(fs);
                var i = 0;
                foreach (var item in items)
                {
                    if (Paused)
                    {
                        return false;
                    }
                    Logger?.Progress(++i, items.Count);
                    if (!Unpack(keys, fs, item, distFolder))
                    {
                        res = false;
                    }
                }
            }
            return res;
        }

        public Task<bool> UnpackAsync(KeyItem keys, string cipherFile, string cipherFileName, string distFolder)
        {
            return Task.Factory.StartNew(() => Unpack(keys, cipherFile, cipherFileName, distFolder), StartNew());
        }

        public bool Unpack(KeyItem keys, FileStream cipherStream, ZipEntry entry, string distFolder)
        {
            if (!Zip.GetFileDataPosition(cipherStream, entry, out var begin, out var end))
            {
                return false;
            }
            return Unpack(keys, cipherStream, entry, begin, end, distFolder);
        }

        public bool Unpack(KeyItem keys, FileStream cipherStream, ZipEntry entry, long begin, long end, string distFolder)
        {
            string directoryName = Path.GetDirectoryName(entry.Name);
            string fileName = Path.GetFileName(entry.Name);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }
            if (directoryName.Length > 0)
            {
                Directory.CreateDirectory(Path.Combine(distFolder, directoryName));
            }
            Logger?.Info($"开始解压：{entry.Name}");
            var distFile = Path.Combine(distFolder, entry.Name);
            using (var fs = File.Create(distFile))
            {
                var res = Unpack(keys.Clone(), cipherStream, begin, end, fs, entry.CompressionMethod);
                if (!res)
                {
                    fs.Close();
                    File.Delete(distFile);
                }
                return res;
            }
        }

        public bool Unpack(KeyItem keys, string cipherFile, string cipherFileName, string distFolder)
        {
            using (var fs = File.OpenRead(cipherFile))
            {
                if (!Zip.GetFileDataPosition(fs, cipherFileName, out var entry, out var begin, out var end))
                {
                    return false;
                }
                if (entry == null)
                {
                    return false;
                }
                return Unpack(keys, fs, entry, begin, end, distFolder);
            }
        }

        public bool Unpack(KeyItem keys, FileStream cipherStream, long begin, long end, FileStream distStream, CompressionMethod compression)
        {
            switch (compression)
            {
                case CompressionMethod.Stored:
                    return UnpackStored(keys, cipherStream, begin, end, distStream);
                case CompressionMethod.Deflated:
                    return Unpack(keys, cipherStream, begin, end, distStream);
                default:
                    Logger?.Error("Unsupported compression method " + compression);
                    break;
            }
            return false;
        }

        /// <summary>
        /// 根据Stored编码保存
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="cipherStream"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="distStream"></param>
        /// <returns></returns>
        public bool UnpackStored(KeyItem keys, FileStream cipherStream, long begin, long end, FileStream distStream)
        {
            var key = new Keys(keys);
            cipherStream.Seek(begin, SeekOrigin.Begin);
            var offset = begin + CrackData.ENCRYPTION_HEADER_SIZE;
            distStream.Seek(0, SeekOrigin.Begin);
            for (var i = begin; i < end; i++)
            {
                var b = (char)cipherStream.ReadByte();
                var p = (byte)(b ^ KeystreamTab.GetByte(keys.Z));
                key.Update(p);
                if (i < offset)
                {
                    continue;
                }
                distStream.WriteByte(p);
            }
            return true;
        }

        /// <summary>
        /// 这是根据deflater编码
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="cipherStream"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="distStream"></param>
        /// <returns></returns>
        public bool Unpack(KeyItem keys, FileStream cipherStream, long begin, long end, FileStream distStream)
        {
            var key = new Keys(keys);
            cipherStream.Seek(begin, SeekOrigin.Begin);
            var tempFile = $"__{DateTime.Now.Millisecond}.zip";
            var tempFs = File.Open(tempFile, FileMode.Create);
            var offset = begin + CrackData.ENCRYPTION_HEADER_SIZE;
            for (var i = begin; i < end; i++)
            {
                var b = (char)cipherStream.ReadByte();
                var p = (byte)(b ^ KeystreamTab.GetByte(keys.Z));
                key.Update(p);
                if (i < offset)
                {
                    continue;
                }
                tempFs.WriteByte(p);
            }
            distStream.Seek(0, SeekOrigin.Begin);
            tempFs.Seek(0, SeekOrigin.Begin);
            var res = true;
            try
            {
                var inflater = new InflaterInputStream(tempFs, new Inflater(true), 4096);
                int size;
                var data = new byte[2048];
                while (true)
                {
                    size = inflater.Read(data, 0, data.Length);
                    if (size > 0)
                    {
                        distStream.Write(data, 0, size);
                    }
                    else
                    {
                        break;
                    }
                }
                inflater.Close();
            }
            catch (Exception ex)
            {
                res = false;
                Logger?.Error(ex.Message);
            }
            tempFs.Close();
            File.Delete(tempFile);
            return res;
        }

        /// <summary>
        /// 解压一个文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="distFile"></param>
        /// <returns></returns>
        public bool Unpack(string file, string distFile)
        {
            var tempFs = File.OpenRead(file);
            var distStream = File.OpenWrite(distFile);
            var inflater = new InflaterInputStream(tempFs, new Inflater(true), 4096);
            int size;
            var data = new byte[2048];
            while (true)
            {
                size = inflater.Read(data, 0, data.Length);
                if (size > 0)
                {
                    distStream.Write(data, 0, size);
                }
                else
                {
                    break;
                }
            }
            inflater.Close();
            tempFs.Close();
            return true;
        }

        private CancellationToken StartNew()
        {
            if (Paused)
            {
                stopToken = new CancellationTokenSource();
            }
            return stopToken.Token;
        }

        public void Stop()
        {
            stopToken.Cancel();
        }

        public async Task<KeyItem?> FindKeyAsync(string cipherFile, string cipherFileName, string plainFile)
        {
            StartNew();
            using var fs = new FileStream(plainFile, FileMode.Open, FileAccess.Read);
            var buffer = new byte[Math.Min(PlainSize, fs.Length)];
            if (buffer.Length == 0)
            {
                return null;
            }
            fs.Read(buffer, 0, buffer.Length);
            return await FindKeyAsync(cipherFile, cipherFileName, buffer);
        }

        public async Task<KeyItem?> FindKeyAsync(string cipherFile, string cipherFileName, byte[] plainData)
        {
            var token = StartNew();
            using var fs = new FileStream(cipherFile, FileMode.Open, FileAccess.Read);
            if (!Zip.GetFileDataPosition(fs, cipherFileName, out var cipherBegin, out var cipherEnd))
            {
                return null;
            }
            if (token.IsCancellationRequested)
            {
                return null;
            }
            return await FindKeyAsync(fs, cipherBegin, cipherEnd, plainData);
        }

        public async Task<KeyItem?> FindKeyAsync(string cipherFile, long cipherBegin, long cipherEnd, byte[] plainData)
        {
            var token = StartNew();
            using var fs = new FileStream(cipherFile, FileMode.Open, FileAccess.Read);
            if (token.IsCancellationRequested)
            {
                return null;
            }
            return await FindKeyAsync(fs, cipherBegin, cipherEnd, plainData);
        }

        public async Task<KeyItem?> FindKeyAsync(FileStream cipherStream, long cipherBegin, long cipherEnd, byte[] plainData)
        {
            var token = StartNew();
            var buffer = new byte[cipherEnd - cipherBegin];
            if (buffer.Length == 0)
            {
                return null;
            }
            cipherStream.Seek(cipherBegin, SeekOrigin.Begin);
            cipherStream.Read(buffer, 0, buffer.Length);
            if (token.IsCancellationRequested)
            {
                return null;
            }
            return await FindKeyAsync(buffer, plainData);
        }

        public Task<KeyItem?> FindKeyAsync(byte[] cipherData, byte[] plainData)
        {
            return Task.Factory.StartNew(() =>
            {
                return FindKey(new CrackData()
                {
                    CipherText = cipherData,
                    PlainText = plainData
                });
            }, StartNew());
        }

        public Task<bool> UnpackAsync(KeyItem keys, string cipherFile, long cipherBegin, long cipherEnd, string distFile)
        {
            return Task.Factory.StartNew(() =>
            {
                using (var fs = File.Create(distFile))
                using (var cipherStream = File.OpenRead(cipherFile))
                {
                    var res = Unpack(keys.Clone(), cipherStream, cipherBegin, cipherEnd, fs, CompressionMethod.Deflated);
                    if (!res)
                    {
                        fs.Close();
                        File.Delete(distFile);
                    }
                    return res;
                }
            }, StartNew());
        }

        public async Task<bool> PackAsync(KeyItem keys, string cipherFile, string distFile)
        {
            return await PackAsync(keys, cipherFile, distFile, string.Empty);
        }

        public Task<bool> PackAsync(KeyItem keys, string cipherFile, string distFile, string password)
        {
            throw new NotImplementedException();
        }

        public Task<string> RecoverPasswordAsync(KeyItem keys, string rule)
        {
            var token = StartNew();
            return Task.Factory.StartNew(() =>
            {
                var ruler = new PasswordRule(rule);
                return PasswordRecovery.Recover(new Keys(keys), ruler.Length, ruler.Charset, Logger, token);
            }, token);
        }

        ~Cracker()
        {
            Stop();
        }
    }
}
