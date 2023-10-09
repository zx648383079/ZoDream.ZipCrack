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

        public bool Extract(string fileName, string saveFolder)
        {
            try
            {
                using var fs = File.OpenRead(fileName);
                using var extractor = SharpCompress.Readers.ReaderFactory.Open(fs);
                while (extractor.MoveToNextEntry())
                {
                    var fullPath = Path.Combine(saveFolder, extractor.Entry.Key);
                    if (extractor.Entry.IsDirectory)
                    {
                        Directory.CreateDirectory(fullPath);
                        continue;
                    }
                    using var fileFs = File.Create(fullPath);
                    extractor.WriteEntryTo(fileFs);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Extract Error: {ex.Message}");
                return false;
            }
        }

        public bool Extract(string fileName, string password, string saveFolder)
        {
            // var tempFile = $"__{DateTime.Now.Millisecond}.zip";
            // var tempFs = File.Open(tempFile, FileMode.Create);
            try
            {
                using var fs = File.OpenRead(fileName);
                using var extractor = SharpCompress.Readers.ReaderFactory.Open(fs, new SharpCompress.Readers.ReaderOptions()
                {
                    Password = password,
                });
while (extractor.MoveToNextEntry())
                {
                    if (extractor.Entry.IsDirectory)
                    {
                        continue;
                    }
                using var ms = new MemoryStream();
                extractor.WriteEntryTo(ms);
                // extractor.ExtractArchive(saveFolder);
                return true;
               }
            }
            catch (Exception ex)
            {
                // Logger.Error($"Extract Error: {ex.Message}");
                return false;
            } finally
            {
                //tempFs.Close();
                //File.Delete(tempFile);
            }
        }

        public Task<string?> ExtractAsync(string fileName, IPasswordProvider provider, string saveFolder, CancellationToken token = default)
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
                    if (Extract(fileName, password, saveFolder))
                    {
                        Logger.Info($"Found Password: {password}");
                        return password;
                    }
                }
                Logger.Info($"Not Found Password!");
                return null;
            }, token);
        }

        public async Task<string?> ExtractAsync(string fileName, string rule, string saveFolder, CancellationToken token = default)
        {
            return await ExtractAsync(fileName, rule, 0, saveFolder, token);
        }

        public async Task<string?> ExtractAsync(string fileName, string rule, long offset, string saveFolder, CancellationToken token = default)
        {
            using var provider = new PasswordRule(rule, offset);
            return await ExtractAsync(fileName, provider, saveFolder, token);
        }

        public Task<string?> ExtractWidthDictionaryAsync(string fileName, string dictFileName, 
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
                    if (Extract(fileName, password, saveFolder))
                    {
                        Logger.Info($"Found Password: {password}");
                        return password;
                    }
                }
                Logger.Info($"Not Found Password!");
                return null;
            }, token);
        }

        public async Task<string?> ExtractWidthDictionaryAsync(string fileName, string dictFileName, string saveFolder, CancellationToken token = default)
        {
            return await ExtractWidthDictionaryAsync(fileName, dictFileName, 0, saveFolder, token);
        }
    }
}
