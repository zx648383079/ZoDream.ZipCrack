using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZoDream.Shared.Crack;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared
{
    public class Cracker: ICracker
    {
        public Cracker()
        {

        }

        public Cracker(ILogger logger)
        {
            Logger = logger;
        }

        private CancellationTokenSource stopToken = new CancellationTokenSource();

        public ILogger? Logger { get; private set; }

        public Task<KeyItem?> FindKeyAsync(string cipherFile, string cipherFileName, string plainFile, string plainFileName)
        {
            stopToken = new CancellationTokenSource();
            return Task.Factory.StartNew(() => FindKey(cipherFile, cipherFileName, plainFile, plainFileName), stopToken.Token);
        }

        public KeyItem? FindKey(string cipherFile, string cipherFileName, string plainFile, string plainFileName)
        {
            using(var cipherStream = File.OpenRead(cipherFile))
            using(var plainStream = File.OpenRead(plainFile))
            {
                return FindKey(cipherStream, cipherFileName, plainStream, plainFileName);
            }
        }

        public KeyItem? FindKey(FileStream cipherStream, string cipherFileName, FileStream plainStream, string plainFileName)
        {
            try
            {
                if (!Zip.GetFileDataPosition(cipherStream, cipherFileName, out var cipherBegin, out var cipherTend))
                {
                    return null;
                }
                if (!Zip.GetFileDataPosition(plainStream, plainFileName, out var plainBegin, out var plainEnd))
                {
                    return null;
                }
                return FindKey(cipherStream, cipherBegin, cipherTend, plainStream, plainBegin, plainEnd);
            }
            catch (Exception ex)
            {
                Logger?.Error(ex.Message);
                return null;
            }
        }

        public KeyItem? FindKey(string cipherFile, long cipherBegin, long cipherTend, string plainFile, long plainBegin, long plainEnd)
        {
            using (var cipherStream = File.OpenRead(cipherFile))
            using (var plainStream = File.OpenRead(plainFile))
            {
                return FindKey(cipherStream, cipherBegin, cipherTend, plainStream, plainBegin, plainEnd);
            }
        }

        public KeyItem? FindKey(FileStream cipherStream, long cipherBegin, long cipherEnd, FileStream plainStream, long plainBegin, long plainEnd)
        {
            var data = new CrackData();
            plainStream.Seek(plainBegin, SeekOrigin.Begin);
            var plainMinEnd = Math.Min(plainBegin + (1 << 20), plainEnd);
            for (var i = plainBegin; i < plainMinEnd; i++)
            {
                data.PlainText.Add((byte)plainStream.ReadByte());
            }
            cipherStream.Seek(cipherBegin, SeekOrigin.Begin);
            for (var i = cipherBegin; i < cipherEnd; i++)
            {
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
            var zr = new Zreduction(this, data.KeyStream);
            if (data.KeyStream.Count > Attack.SIZE)
            {
                Logger?.Info($"Z reduction using {data.KeyStream.Count - Attack.CONTIGUOUS_SIZE} bytes of known plaintext");
                zr.Reduce();
            }
            zr.Generate();
            Logger?.Info($"Generated {zr.Count} Z values. ");

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
                    continue; // cannot break out of an OpenMP for loop
                }

                attack.Carryout(candidates[i]);
                {
                    Logger?.Progress(++done, size);
                    shouldStop = attack.SolutionItems.Count > 0;
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


        public Task<KeyItem?> FindKeyAsync(string cipherFile, long cipherBegin, long cipherTend, string plainFile, long plainBegin, long plainEnd)
        {
            return Task.Factory.StartNew(() => FindKey(cipherFile, cipherBegin, cipherTend, plainFile, plainBegin, plainEnd));
        }

        public Task<bool> UnpackAsync(KeyItem keys, string cipherFile, string distFolder)
        {
            return Task.Factory.StartNew(() => Unpack(keys, cipherFile, distFolder));
        }

        public bool Unpack(KeyItem keys, string cipherFile, string distFolder)
        {
            var res = true;
            using(var fs = File.OpenRead(cipherFile))
            {
                var items = Zip.GetEntries(fs);
                var i = 0;
                foreach (var item in items)
                {
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
            stopToken = new CancellationTokenSource();
            return Task.Factory.StartNew(() => Unpack(keys, cipherFile, cipherFileName, distFolder), stopToken.Token);
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
            cipherStream.Seek(begin, SeekOrigin.Begin);
            var offset = begin + CrackData.ENCRYPTION_HEADER_SIZE;
            distStream.Seek(0, SeekOrigin.Begin);
            for (var i = begin; i < end; i++)
            {
                var b = (char)cipherStream.ReadByte();
                var p = (byte)(b ^ KeystreamTab.GetByte(keys.Z));
                keys.Update(p);
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
            cipherStream.Seek(begin, SeekOrigin.Begin);
            var tempFile = $"__{DateTime.Now.Millisecond}.zip";
            var tempFs = File.Open(tempFile, FileMode.Create);
            var offset = begin + CrackData.ENCRYPTION_HEADER_SIZE;
            for (var i = begin; i < end; i++)
            {
                var b = (char)cipherStream.ReadByte();
                var p = (byte)(b ^ KeystreamTab.GetByte(keys.Z));
                keys.Update(p);
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

        public void Stop()
        {
            stopToken.Cancel();
        }
    }
}
