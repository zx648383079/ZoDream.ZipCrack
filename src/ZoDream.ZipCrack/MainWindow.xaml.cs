using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZoDream.Shared;
using ZoDream.Shared.Crack;
using ZoDream.Shared.Loggers;
using ZoDream.ZipCrack.Utils;
using ZoDream.ZipCrack.ViewModels;

namespace ZoDream.ZipCrack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }

        public MainViewModel ViewModel = new MainViewModel();
        private Cracker? crackerTask;

        public bool IsLoading
        {
            set
            {
                StopBtn.Visibility = progressBar.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                ActionPanel.Visibility = !value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void doBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cipherFileTb.FileName))
            {
                MessageBox.Show(LocalizedLangExtension.GetString("selectZipTip"));
                return;
            }
            infoTb.Clear();
            if (KeyTb.IsCompleted)
            {
                Unpack();
                return;
            }
            FindKeys();
        }

        private async void Unpack(bool justFile = false)
        {
            var folder = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = System.IO.Path.GetDirectoryName(cipherFileTb.FileName),
                ShowNewFolderButton = true,
                Description = LocalizedLangExtension.GetString("unzipBtnContent"),
            };
            if (folder.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            IsLoading = true;
            Zip.CodePage = EncodingTb.Text.Trim();
            crackerTask = GetCracker();
            bool res;
            if (justFile)
            {
                res = await crackerTask.UnpackAsync(KeyTb.Keys, cipherFileTb.FileName, cipherNameTb.Text.Trim(), folder.SelectedPath);
            } else
            {
                res = await crackerTask.UnpackAsync(KeyTb.Keys, cipherFileTb.FileName, folder.SelectedPath);
            }
            MessageBox.Show(LocalizedLangExtension.GetString(res ? "unzipSuccess" : "unzipError"));
            IsLoading = false;
        }

        private async void FindKeys()
        {
            if (string.IsNullOrWhiteSpace(plainFileTb.FileName))
            {
                MessageBox.Show(LocalizedLangExtension.GetString("selectPlainTip"));
                return;
            }
            IsLoading = true;
            Zip.CodePage = EncodingTb.Text.Trim();
            crackerTask = GetCracker();
            var keys = await crackerTask.FindKeyAsync(cipherFileTb.FileName, cipherNameTb.Text.Trim(), 
                plainFileTb.FileName, plainNameTb.Text.Trim());
            if (keys != null)
            {
                KeyTb.Keys = keys;
            }
            MessageBox.Show(LocalizedLangExtension.GetString(keys != null ?
                "getSuccess" : "getError"));
            IsLoading = false;
        }

        private Cracker GetCracker()
        {
            crackerTask?.Stop();
            progressBar.Value = 0;
            var logger = new EventLogger();
            logger.OnLog += (s, e) =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    infoTb.AppendLine(s);
                });
            };
            logger.OnProgress += (s, e) =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    progressBar.Visibility = Visibility.Visible;
                    progressBar.Value = s * 100 / e;
                });
            };
            return new Cracker(logger);
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EncodingTb.Text = Zip.DefaultEncoding();
        }

        private void cipherFileTb_FileChanged(object sender, string fileName)
        {
            KeyTb.Text = string.Empty;
            Zip.CodePage = EncodingTb.Text.Trim();
            ViewModel.Loadcipher(fileName);
            cipherNameTb.Text = "";
        }

        private void plainFileTb_FileChanged(object sender, string fileName)
        {
            Zip.CodePage = EncodingTb.Text.Trim();
            ViewModel.LoadPlain(fileName);
            if (ViewModel.FindEqualsFile(out var cipherFile, out var plainFile))
            {
                cipherNameTb.SelectedItem = cipherFile;
                plainNameTb.SelectedItem = plainFile;
                GetBtn.IsEnabled = true;
            }
            else
            {
                MessageBox.Show(LocalizedLangExtension.GetString("plainError"));
                GetBtn.IsEnabled = false;
                plainNameTb.Text = string.Empty;
            }
        }

        private void GetBtn_Click(object sender, RoutedEventArgs e)
        {
            FindKeys();
        }

        private void UnzipFileBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cipherNameTb.Text))
            {
                MessageBox.Show(LocalizedLangExtension.GetString("selectAFileTip"));
                return;
            }
            Unpack(true);
        }

        private void UnzipFilesBtn_Click(object sender, RoutedEventArgs e)
        {
            Unpack();
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            var box = MessageBox.Show(LocalizedLangExtension.GetString("clearTip"),
                LocalizedLangExtension.GetString("tip"),
                MessageBoxButton.YesNo);
            if (box != MessageBoxResult.Yes)
            {
                return;
            }
            KeyTb.Text = plainFileTb.FileName = plainNameTb.Text = 
                cipherNameTb.Text = cipherFileTb.FileName = string.Empty;
            ViewModel.CipherItems.Clear();
            ViewModel.PlainItems.Clear();
            infoTb.Clear();
            UnzipFileBtn.IsEnabled = UnzipFilesBtn.IsEnabled = GetBtn.IsEnabled = false;
        }

        private void KeyTb_TextChanged(object sender, string text)
        {
            UnzipFileBtn.IsEnabled = UnzipFilesBtn.IsEnabled = KeyTb.IsCompleted;
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            var box = MessageBox.Show(LocalizedLangExtension.GetString("stopTip"), 
                LocalizedLangExtension.GetString("tip"), 
                MessageBoxButton.YesNo);
            if (box != MessageBoxResult.Yes)
            {
                return;
            }
            crackerTask?.Stop();
            IsLoading = false;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            crackerTask?.Stop();
        }
    }
}
