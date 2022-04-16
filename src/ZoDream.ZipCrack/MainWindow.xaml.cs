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
using ZoDream.Shared.CSharp;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Loggers;
using ZoDream.Shared.Models;
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
        private ICracker? crackerTask;

        public bool IsLoading
        {
            set
            {
                ApplyVisible(value, progressBar, StopBtn);
                ApplyVisible(!value, GetActionPanel, UnzipActionPanel, PwdActionPanel, ConverterActionPanel);
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
            PwdNewTb.Text = PwdRuleTb.Text = TextTb.Text = plainSourceTb.FileName =
                KeyTb.Text = plainFileTb.FileName = plainNameTb.Text = 
                cipherNameTb.Text = cipherFileTb.FileName = string.Empty;
            ViewModel.CipherItems.Clear();
            ViewModel.PlainItems.Clear();
            infoTb.Clear();
            GetBtnEnable();
            UnzipBtnEnable();
            ConverterBtnEnable();
            RecoverBtnEnable();
        }

        private void KeyTb_TextChanged(object sender, string text)
        {
            UnzipBtnEnable();
            ConverterBtnEnable();
            RecoverBtnEnable();
        }

        private void cipherNameTb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetBtnEnable();
            UnzipBtnEnable();
            ConverterBtnEnable();
        }

        private void plainSourceTb_FileChanged(object sender, string fileName)
        {
            GetBtnEnable();
        }

        private void TextTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            GetBtnEnable();
        }

        private void PwdRuleTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            RecoverBtnEnable();
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

        private void ModeTb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var i = (sender as ComboBox)!.SelectedIndex;
            ApplyVisible(SourcePanel, i < 5);
            ApplyVisible(DistPanel, i < 1);
            ApplyVisible(DistFilePanel, i == 1);
            ApplyVisible(DistTextPanel, i == 2);
            ApplyVisible(PwdPanel, i == 5);
            ApplyVisible(GetActionPanel, i < 3);
            ApplyVisible(UnzipActionPanel, i < 4 && i > 2);
            ApplyVisible(ConverterActionPanel, i == 4);
            ApplyVisible(PwdActionPanel, i == 5);
            ApplyVisible(CvtPanel, i == 4);

        }

        private void RecoverBtn_Click(object sender, RoutedEventArgs e)
        {
            RecoverPassword();
        }

        private void ConverterBtn_Click(object sender, RoutedEventArgs e)
        {
            ConverterZip();
        }

        private void ApplyVisible(UIElement? box, bool visible)
        {
            if (box == null)
            {
                return;
            }
            box.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ApplyVisible(bool visible, params UIElement?[] items)
        {
            foreach (var item in items)
            {
                ApplyVisible(item, visible);
            }
        }

        private void GetBtnEnable()
        {
            if (string.IsNullOrWhiteSpace(cipherFileTb.FileName) || 
                string.IsNullOrWhiteSpace(cipherNameTb.Text) ||
                (ModeTb.SelectedIndex < 1 && (string.IsNullOrWhiteSpace(plainFileTb.FileName) ||
                string.IsNullOrWhiteSpace(plainNameTb.Text))) ||
                (ModeTb.SelectedIndex == 1 && string.IsNullOrWhiteSpace(plainSourceTb.FileName)) ||
                (ModeTb.SelectedIndex == 2 && string.IsNullOrWhiteSpace(TextTb.Text))
                )
            {
                GetBtn.IsEnabled = false;
                return;
            }
            GetBtn.IsEnabled = true;
        }

        private void UnzipBtnEnable()
        {
            if (string.IsNullOrWhiteSpace(cipherFileTb.FileName) || !KeyTb.IsCompleted)
            {
                UnzipFilesBtn.IsEnabled = UnzipFileBtn.IsEnabled = false;
                return;
            }
            UnzipFileBtn.IsEnabled = !string.IsNullOrWhiteSpace(cipherNameTb.Text);
            UnzipFilesBtn.IsEnabled = true;
        }

        private void ConverterBtnEnable()
        {
            if (string.IsNullOrWhiteSpace(cipherFileTb.FileName) || !KeyTb.IsCompleted)
            {
                ConverterBtn.IsEnabled = false;
                return;
            }
            ConverterBtn.IsEnabled = true;
        }

        private void RecoverBtnEnable()
        {
            if (!KeyTb.IsCompleted || string.IsNullOrWhiteSpace(PwdRuleTb.Text))
            {
                RecoverBtn.IsEnabled = false;
                return;
            }
            RecoverBtn.IsEnabled = true;
        }

        private async void ConverterZip()
        {
            var picker = new Microsoft.Win32.SaveFileDialog
            {
                Title = "选择保存路径",
                Filter = "ZIP文件|*.zip|所有文件|*.*",
            };
            if (picker.ShowDialog() != true)
            {
                return;
            }
            IsLoading = true;
            crackerTask = GetCracker();
            var res = await crackerTask.PackAsync(KeyTb.Keys!, cipherFileTb.FileName,
                picker.FileName,
                PwdNewTb.Text);
            MessageBox.Show(LocalizedLangExtension.GetString(res ?
                "converterSuccess" : "converterError"));
            IsLoading = false;
        }

        private async void RecoverPassword()
        {
            IsLoading = true;
            crackerTask = GetCracker();
            var res = await crackerTask.RecoverPasswordAsync(KeyTb.Keys!, PwdRuleTb.Text);
            if (string.IsNullOrEmpty(res))
            {
                MessageBox.Show(LocalizedLangExtension.GetString("recoverError"));
            } else {
                MessageBox.Show(res, LocalizedLangExtension.GetString("recoverSuccess"));
            }
            IsLoading = false;
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
            }
            else
            {
                res = await crackerTask.UnpackAsync(KeyTb.Keys, cipherFileTb.FileName, folder.SelectedPath);
            }
            MessageBox.Show(LocalizedLangExtension.GetString(res ? "unzipSuccess" : "unzipError"));
            IsLoading = false;
        }

        private async void FindKeys()
        {
            Zip.CodePage = EncodingTb.Text.Trim();
            IsLoading = true;
            crackerTask = GetCracker();
            KeyItem? keys;
            if (ModeTb.SelectedIndex < 1)
            {
                if (string.IsNullOrWhiteSpace(plainFileTb.FileName))
                {
                    MessageBox.Show(LocalizedLangExtension.GetString("selectPlainTip"));
                    IsLoading = false;
                    return;
                }
                keys = await crackerTask.FindKeyAsync(cipherFileTb.FileName, cipherNameTb.Text.Trim(),
                    plainFileTb.FileName, plainNameTb.Text.Trim());
            } else if (ModeTb.SelectedIndex == 1)
            {
                keys = await crackerTask.FindKeyAsync(cipherFileTb.FileName, 
                    cipherNameTb.Text.Trim(),
                    plainSourceTb.FileName);
            } else if (ModeTb.SelectedIndex == 2)
            {
                keys = await crackerTask.FindKeyAsync(cipherFileTb.FileName,
                    cipherNameTb.Text.Trim(),
                    Encoding.GetEncoding(EncodingTb.Text.Trim()).GetBytes(TextTb.Text)
                    );
            } else
            {
                keys = null;
            }
            if (keys != null)
            {
                KeyTb.Keys = keys;
            }
            App.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(LocalizedLangExtension.GetString(keys != null ?
                "getSuccess" : "getError"));
                IsLoading = false;
            });
        }

        private ICracker GetCracker()
        {
            crackerTask?.Stop();
            progressBar.Value = 0;
            var logger = new EventLogger();
            var isLastProgress = false;
            logger.OnLog += (s, e) =>
            {
                isLastProgress = false;
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
                    if (isLastProgress)
                    {
                        infoTb.ReplaceLine($"{s}/{e}");
                    } else
                    {
                        infoTb.AppendLine($"{s}/{e}");
                    }
                });
                isLastProgress = true;
            };
            return new Shared.CPlus.Cracker(logger);
        }

    }
}
