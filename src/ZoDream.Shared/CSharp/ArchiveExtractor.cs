using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.CSharp
{
    public class ArchiveExtractor : IExtractor
    {

        public ArchiveExtractor(
            ILogger logger)
        {
            Logger = logger;
        }

        public ILogger Logger { get; private set; }

        public bool Extract(string fileName, 
            string saveFolder,
            CancellationToken token = default)
        {
            try
            {
                using var fs = File.OpenRead(fileName);
                using var extractor = ReaderFactory.Open(fs);
                Extract(extractor, saveFolder, token);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Extract Error: {ex.Message}");
                return false;
            }
        }

        private void Extract(IReader extractor, 
            string saveFolder,
            CancellationToken token = default)
        {
            while (extractor.MoveToNextEntry())
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                var fullPath = Path.Combine(saveFolder, extractor.Entry.Key);
                if (extractor.Entry.IsDirectory)
                {
                    Directory.CreateDirectory(fullPath);
                    continue;
                }
                using var fileFs = File.Create(fullPath);
                extractor.WriteEntryTo(fileFs);
            }
            Logger.Info($"Extract successfully!");
        }

        public bool Extract(string fileName, string password, 
            string saveFolder,
            CancellationToken token = default)
        {
            try
            {
                using var fs = File.OpenRead(fileName);
                using var extractor = ReaderFactory.Open(fs, new ReaderOptions()
                {
                    Password = password,
                });
                Extract(extractor, saveFolder, token);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Extract Error: {ex.Message}");
                return false;
            }
        }

        public bool TryExtract(string fileName, string password)
        {
            // var tempFile = $"__{DateTime.Now.Millisecond}.zip";
            // var tempFs = File.Open(tempFile, FileMode.Create);
            try
            {
                using var fs = File.OpenRead(fileName);
                using var extractor = ReaderFactory.Open(fs, new ReaderOptions()
                {
                    Password = password,
                });
                var i = 0;
                // 当前判断功能有问题
                while (extractor.MoveToNextEntry() && i < 10)
                {
                    if (extractor.Entry.IsDirectory)
                    {
                        continue;
                    }
                    using var ms = new MemoryStream();
                    extractor.WriteEntryTo(ms);
                    i++;
                }
                Logger.Error($"Extract Error: Not Found File");
                return i > 0;
            }
            catch (Exception ex)
            {
                // Logger.Error($"Extract Error: {ex.Message}");
                return false;
            }
            finally
            {
                //tempFs.Close();
                //File.Delete(tempFile);
            }
        }

        public Task<bool> ExtractAsync(string fileName, 
            string saveFolder,
            CancellationToken token = default)
        {
            return Task.Factory.StartNew(() => {
                return Extract(fileName, saveFolder, token);
            }, token);
        }
        public Task<bool> ExtractAsync(string fileName, 
            string password, string saveFolder, CancellationToken token = default)
        {
            return Task.Factory.StartNew(() => {
                return Extract(fileName, password, saveFolder, token);
            }, token);
        }

        public Task<string?> TryExtractAsync(string fileName, IPasswordProvider provider, string saveFolder, 
            CancellationToken token = default)
        {
            return Task.Factory.StartNew(() => {
                Logger.Info("Begin Try Extract...");
                while (provider.HasMore)
                {
                    if (token.IsCancellationRequested)
                    {
                        Logger.Info($"Task is canceled!");
                        return null;
                    }
                    var password = provider.Next();
                    if (string.IsNullOrEmpty(password))
                    {
                        continue;
                    }
                    Logger.Progress(provider.Position, provider.Count);
                    if (TryExtract(fileName, password))
                    {
                        Logger.Info($"Found Password: {password}");
                        return password;
                    }
                }
                Logger.Info($"Not Found Password!");
                return null;
            }, token);
        }

        public async Task<string?> TryExtractAsync(string fileName, 
            string rule, string saveFolder, CancellationToken token = default)
        {
            return await TryExtractAsync(fileName, rule, 0, saveFolder, token);
        }

        public async Task<string?> TryExtractAsync(string fileName, string rule, long offset, string saveFolder, CancellationToken token = default)
        {
            using var provider = new PasswordRule(rule, offset);
            return await TryExtractAsync(fileName, provider, saveFolder, token);
        }

        public Task<string?> TryExtractWidthDictionaryAsync(string fileName, string dictFileName, 
            long offset,
            string saveFolder, CancellationToken token = default)
        {
            return Task.Factory.StartNew(() => {
                Logger.Info("Begin Try Extract...");
                using var provider = new PasswordDictionary(dictFileName, offset);
                while (provider.HasMore)
                {
                    if (token.IsCancellationRequested)
                    {
                        Logger.Info($"Task is canceled!");
                        return null;
                    }
                    var password = provider.Next();
                    if (string.IsNullOrWhiteSpace(password))
                    {
                        continue;
                    }
                    Logger.Progress(provider.Position, provider.Count);
                    if (TryExtract(fileName, password))
                    {
                        Logger.Info($"Found Password: {password}");
                        return password;
                    }
                }
                Logger.Info($"Not Found Password!");
                return null;
            }, token);
        }

        public async Task<string?> TryExtractWidthDictionaryAsync(string fileName, string dictFileName, string saveFolder, CancellationToken token = default)
        {
            return await TryExtractWidthDictionaryAsync(fileName, dictFileName, 0, saveFolder, token);
        }
    }
}
