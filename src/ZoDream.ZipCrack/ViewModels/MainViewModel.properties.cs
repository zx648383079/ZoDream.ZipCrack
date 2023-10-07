using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using ZoDream.Shared.CSharp;
using ZoDream.Shared.Models;
using ZoDream.ZipCrack.Utils;

namespace ZoDream.ZipCrack.ViewModels
{
    public partial class MainViewModel
    {

        private string[] encodingItems = Zip.GetEncodings();
        public string[] EncodingItems
        {
            get => encodingItems;
            set => Set(ref encodingItems, value);
        }

        private bool isPaused = true;

        public bool IsPaused {
            get => isPaused;
            set => Set(ref isPaused, value);
        }


        private int modeIndex;

        public int ModeIndex {
            get => modeIndex;
            set => Set(ref modeIndex, value);
        }

        private string[] modeItems;

        public string[] ModeItems {
            get => modeItems;
            set => Set(ref modeItems, value);
        }

        private bool useDll;

        public bool UseDll {
            get => useDll;
            set => Set(ref useDll, value);
        }

        private string encodingText;

        public string EncodingText {
            get => encodingText;
            set => Set(ref encodingText, value);
        }

        private string cipherArchiveFile = string.Empty;

        public string CipherArchiveFile {
            get => cipherArchiveFile;
            set {
                Set(ref cipherArchiveFile, value);

                if (ModeIndex < 3)
                {
                    InternalKey = null;
                }
                Zip.CodePage = EncodingText.Trim();
                LoadCipher(value);
                CipherSelectedName = string.Empty;
                OnPropertyChanged(nameof(EnableGet));
                OnPropertyChanged(nameof(EnableDecompress));
                OnPropertyChanged(nameof(EnableDecompressFiles));
                OnPropertyChanged(nameof(EnableConverter));
            }
        }

        private string cipherSelectedName = string.Empty;

        public string CipherSelectedName {
            get => cipherSelectedName;
            set {
                Set(ref cipherSelectedName, value);
                OnPropertyChanged(nameof(EnableGet));
                OnPropertyChanged(nameof(EnableDecompress));
            }
        }

        private string plainArchiveFile = string.Empty;

        public string PlainArchiveFile {
            get => plainArchiveFile;
            set {
                Set(ref plainArchiveFile, value);
                Zip.CodePage = EncodingText.Trim();
                LoadPlain(value);
                if (FindEqualsFile(out var cipherFile, out var plainFile))
                {
                    CipherSelectedName = cipherFile!.Name;
                    PlainSelectedName = plainFile!.Name;
                }
                else
                {
                    MessageBox.Show(LocalizedLangExtension.GetString("plainError"));
                    PlainSelectedName = string.Empty;
                }
                OnPropertyChanged(nameof(EnableGet));
            }
        }

        private string plainSelectedName = string.Empty;

        public string PlainSelectedName {
            get => plainSelectedName;
            set {
                Set(ref plainSelectedName, value);
                OnPropertyChanged(nameof(EnableGet));
            }
        }

        private string plainFile = string.Empty;

        public string PlainFile {
            get => plainFile;
            set {
                Set(ref plainFile, value);
                OnPropertyChanged(nameof(EnableGet));
            }
        }

        private string plainText = string.Empty;

        public string PlainText {
            get => plainText;
            set {
                Set(ref plainText, value);
                OnPropertyChanged(nameof(EnableGet));
            }
        }


        private ObservableCollection<FileItem> cipherItems = new();

        public ObservableCollection<FileItem> CipherItems
        {
            get => cipherItems;
            set => Set(ref cipherItems, value);
        }

        private ObservableCollection<FileItem> plainItems = new();

        public ObservableCollection<FileItem> PlainItems
        {
            get => plainItems;
            set => Set(ref plainItems, value);
        }


        private string passwordRule = string.Empty;
        public string PasswordRule {
            get => passwordRule;
            set {
                Set(ref passwordRule, value);
                OnPropertyChanged(nameof(EnableRecover));
            }
        }

        private string passwordNew = string.Empty;
        public string PasswordNew {
            get => passwordNew;
            set => Set(ref passwordNew, value);
        }

        private string decodeFile = string.Empty;

        public string DecodeFile {
            get => decodeFile;
            set {
                Set(ref decodeFile, value);
                OnPropertyChanged(nameof(EnableDecode));
            }
        }

        private KeyItem? internalKey;

        public KeyItem? InternalKey {
            get => internalKey;
            set {
                Set(ref internalKey, value);
                OnPropertyChanged(nameof(EnableDecompress));
                OnPropertyChanged(nameof(EnableDecompressFiles));
                OnPropertyChanged(nameof(EnableConverter));
                OnPropertyChanged(nameof(EnableDecode));
                OnPropertyChanged(nameof(EnableRecover));
            }
        }

    }
}