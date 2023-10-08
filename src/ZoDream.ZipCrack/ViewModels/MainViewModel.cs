using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using ZoDream.Shared.CSharp;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Loggers;
using ZoDream.Shared.Models;
using ZoDream.Shared.ViewModel;
using ZoDream.ZipCrack.Utils;

namespace ZoDream.ZipCrack.ViewModels
{
    public partial class MainViewModel: BindableBase
    {

        public MainViewModel()
        {
            GetCommand = new RelayCommand(TapGet);
            StopCommand = new RelayCommand(TapStop);
            ClearCommand = new RelayCommand(TapClear);
            DecodeCommand = new RelayCommand(TapDecode);
            DecompressCommand = new RelayCommand(TapDecompress);
            DecompressFilesCommand = new RelayCommand(TapDecompressFiles);
            RecoverCommand = new RelayCommand(TapRecover);
            ConverterCommand = new RelayCommand(TapConverter);
            CrackCommand = new RelayCommand(TapCrack);
            modeItems = new string[]
            {
                LocalizedLangExtension.GetString("getKey"),
                LocalizedLangExtension.GetString("getKeyByFile"),
                LocalizedLangExtension.GetString("getKeyByText"),
                LocalizedLangExtension.GetString("unzip"),
                LocalizedLangExtension.GetString("converterZip"),
                LocalizedLangExtension.GetString("recover"),
                LocalizedLangExtension.GetString("decodeFile"),
                LocalizedLangExtension.GetString("ruleCrack"),
                LocalizedLangExtension.GetString("dictCrack"),
            };
            encodingText = Zip.DefaultEncoding();
        }

        public ILogger Logger { get; private set; } = new EventLogger();
        private ICracker? crackerTask;
        private CancellationTokenSource? tokenSource;

        private ICracker GetCracker()
        {
            crackerTask?.Stop();
            if (UseDll)
            {
                return new Shared.CPlus.Cracker(Logger);
            }
            return new Cracker(Logger);
        }

        public bool FindEqualsFile(out FileItem? cipher, out FileItem? plain)
        {
            foreach (var item in cipherItems)
            {
                foreach (var it in PlainItems)
                {
                    if (item.CRC32 == it.CRC32)
                    {
                        cipher = item;
                        plain = it;   
                        return true;
                    }
                }
            }
            cipher = null;
            plain = null;
            return false;
        }

        public void LoadCipher(string file)
        {
            if (Path.GetExtension(file) != ".zip")
            {
                return;
            }
            var items = Zip.GetFiles(file);
            CipherItems.Clear();
            foreach (var item in items)
            {
                CipherItems.Add(item);
            }
        }

        public void LoadPlain(string file)
        {
            if (Path.GetExtension(file) != ".zip")
            {
                return;
            }
            var items = Zip.GetFiles(file);
            PlainItems.Clear();
            foreach (var item in items)
            {
                PlainItems.Add(item);
            }
        }


        public async void FindKeys()
        {
            Zip.CodePage = EncodingText;
            IsPaused = false;
            crackerTask = GetCracker();
            KeyItem? keys;
            if (ModeIndex < 1)
            {
                if (string.IsNullOrWhiteSpace(PlainArchiveFile))
                {
                    MessageBox.Show(LocalizedLangExtension.GetString("selectPlainTip"));
                    IsPaused = true;
                    return;
                }
                keys = await crackerTask.FindKeyAsync(CipherArchiveFile, CipherSelectedName.Trim(),
                    PlainArchiveFile, PlainSelectedName.Trim());
            }
            else if (ModeIndex == 1)
            {
                keys = await crackerTask.FindKeyAsync(CipherArchiveFile, CipherSelectedName.Trim(),
                    PlainFile);
            }
            else if (ModeIndex == 2)
            {
                keys = await crackerTask.FindKeyAsync(CipherArchiveFile, CipherSelectedName.Trim(),
                    Zip.DeflateText(PlainText, Encoding.GetEncoding(EncodingText.Trim())));
            }
            else
            {
                keys = null;
            }
            if (keys != null)
            {
                InternalKey = keys;
            }
            App.Current.Dispatcher.Invoke(() => {
                MessageBox.Show(LocalizedLangExtension.GetString(keys != null ?
                "getSuccess" : "getError"));
                IsPaused = true;
            });
        }

        public async void Unpack(bool justFile = false)
        {
            var folder = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = Path.GetDirectoryName(CipherArchiveFile)!,
                ShowNewFolderButton = true,
                Description = LocalizedLangExtension.GetString("unzipBtnContent"),
            };
            if (folder.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            IsPaused = false;
            Zip.CodePage = EncodingText.Trim();
            crackerTask = GetCracker();
            bool res;
            if (justFile)
            {
                res = await crackerTask.UnpackAsync(InternalKey!, CipherArchiveFile, CipherSelectedName.Trim(), folder.SelectedPath!);
            }
            else
            {
                res = await crackerTask.UnpackAsync(InternalKey!, CipherArchiveFile, folder.SelectedPath!);
            }
            MessageBox.Show(LocalizedLangExtension.GetString(res ? "unzipSuccess" : "unzipError"));
            IsPaused = true;
        }

        public void Stop()
        {
            crackerTask?.Stop();
            tokenSource?.Cancel();
            IsPaused = true;
        }
    }
}