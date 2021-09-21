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

        public bool IsLoading
        {
            set
            {
                doBtn.IsEnabled = !value;
                progressBar.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public KeyItem Keys
        {
            get {  return new KeyItem(key1Tb.Text, key2Tb.Text, key3Tb.Text); }
            set
            {
                key1Tb.Text = value.X.ToString("x8");
                key2Tb.Text = value.Y.ToString("x8");
                key3Tb.Text = value.Z.ToString("x8");
            }
        }

        private void cipherBtn_Click(object sender, RoutedEventArgs e)
        {
            var open = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "ZIP|*.zip|All Files|*.*",
                Title = "Select the encrypted compressed file"
            };
            if (open.ShowDialog() != true)
            {
                return;
            }
            cipherFileTb.Text = open.FileName;
        }

        private void plainBtn_Click(object sender, RoutedEventArgs e)
        {
            var open = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "ZIP|*.zip|All Files|*.*",
                Title = "Select the plain file"
            };
            if (open.ShowDialog() != true)
            {
                return;
            }
            plainFileTb.Text = open.FileName;
        }

        private void key1Tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            var keys = (sender as TextBox).Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (keys.Length == 3)
            {
                key1Tb.Text = keys[0];
                key2Tb.Text = keys[1];
                key3Tb.Text = keys[2];
            }
            if (!string.IsNullOrWhiteSpace(key1Tb.Text) && 
                !string.IsNullOrWhiteSpace(key2Tb.Text) && 
                !string.IsNullOrWhiteSpace(key3Tb.Text))
            {
                doBtn.Content = "Select unzip to folder";
            } else
            {
                doBtn.Content = "Get Keys";
            }
        }

        private void doBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cipherFileTb.Text))
            {
                MessageBox.Show("Select the encrypted compressed file");
                return;
            }
            infoTb.Text = string.Empty;
            if (!string.IsNullOrWhiteSpace(key1Tb.Text) &&
                !string.IsNullOrWhiteSpace(key2Tb.Text) &&
                !string.IsNullOrWhiteSpace(key3Tb.Text))
            {
                Unpack();
                return;
            }
            FindKeys();
        }

        private async void Unpack()
        {
            var folder = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = System.IO.Path.GetDirectoryName(cipherFileTb.Text),
                ShowNewFolderButton = true,
                Description = "Select unzip to folder",
            };
            if (folder.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            IsLoading = true;
            var cracker = GetCracker();
            var res = await cracker.UnpackAsync(Keys, cipherFileTb.Text.Trim(), folder.SelectedPath);
            MessageBox.Show(res ? "Unzip Success" : "Unzip Failure");
            IsLoading = false;
        }

        private async void FindKeys()
        {
            if (string.IsNullOrWhiteSpace(plainFileTb.Text))
            {
                MessageBox.Show("Select the plain file");
                return;
            }
            IsLoading = true;
            var cracker = GetCracker();
            var keys = await cracker.FindKeyAsync(cipherFileTb.Text.Trim(), cipherNameTb.Text.Trim(), 
                plainFileTb.Text.Trim(), plainNameTb.Text.Trim());
            if (keys != null)
            {
                Keys = keys;
            }
            MessageBox.Show(keys != null ? "Get Keys Success" : "Get Keys Failure");
            IsLoading = false;
        }

        private Cracker GetCracker()
        {
            progressBar.Value = 0;
            var logger = new EventLogger();
            logger.OnLog += (s, e) =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    infoTb.Text += s + "\n";
                });
            };
            logger.OnProgress += (s, e) =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    progressBar.Value = s * 100 / e;
                });
            };
            return new Cracker(logger);
        }

        private void cipherFileTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.Loadcipher((sender as TextBox).Text.Trim());
            cipherNameTb.Text = "";
        }

        private void plainFileTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.LoadPlain((sender as TextBox).Text.Trim());
            if (ViewModel.FindEqualsFile(out var cipherFile, out var plainFile))
            {
                cipherNameTb.SelectedItem = cipherFile;
                plainNameTb.SelectedItem = plainFile;
            } else
            {
                MessageBox.Show("No corresponding file exists");
            }
        }

        private void cipherFileTb_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;
        }

        private void cipherFileTb_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var file = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
                if (string.IsNullOrEmpty(file))
                {
                    return;
                }
                (sender as TextBox).Text = file;
            }
        }
    }
}
