using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using ZoDream.Shared.CSharp;
using ZoDream.ZipCrack.Utils;
using static System.Net.Mime.MediaTypeNames;

namespace ZoDream.ZipCrack.ViewModels
{
    public partial class MainViewModel
    {

        public ICommand GetCommand { get; private set; }
        public ICommand DecompressCommand { get; private set; }
        public ICommand DecompressFilesCommand { get; private set; }
        public ICommand ConverterCommand { get; private set; }
        public ICommand RecoverCommand { get; private set; }
        public ICommand DecodeCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand ClearCommand { get; private set; }

        private bool EnableGet(object? _)
        {
            return !string.IsNullOrWhiteSpace(CipherArchiveFile) &&
                string.IsNullOrWhiteSpace(CipherSelectedName) &&
                !(ModeIndex < 1 && (string.IsNullOrWhiteSpace(PlainArchiveFile) ||
                string.IsNullOrWhiteSpace(PlainSelectedName))) &&
                !(ModeIndex == 1 && string.IsNullOrWhiteSpace(PlainFile)) &&
                !(ModeIndex == 2 && string.IsNullOrWhiteSpace(PlainText));
        }

        private void TapGet(object? _)
        {
            FindKeys();
        }

        private void TapStop(object? _)
        {
            var box = MessageBox.Show(LocalizedLangExtension.GetString("stopTip"),
                LocalizedLangExtension.GetString("tip"),
                MessageBoxButton.YesNo);
            if (box != MessageBoxResult.Yes)
            {
                return;
            }
            Stop();
        }

        private void TapClear(object? _)
        {
            var box = MessageBox.Show(LocalizedLangExtension.GetString("clearTip"),
                LocalizedLangExtension.GetString("tip"),
                MessageBoxButton.YesNo);
            if (box != MessageBoxResult.Yes)
            {
                return;
            }
            InternalKey = null;
            PasswordNew = PasswordRule = PlainText = PlainFile =
               PlainArchiveFile = PlainSelectedName =
               CipherSelectedName = CipherArchiveFile = string.Empty;
            CipherItems.Clear();
            PlainItems.Clear();
            // infoTb.Clear();
        }

        private bool EnableDecode(object? _)
        {
            return !string.IsNullOrWhiteSpace(DecodeFile);
        }

        private void TapDecode(object? _)
        {
            var picker = new Microsoft.Win32.SaveFileDialog()
            {
                FileName = System.IO.Path.GetFileName(DecodeFile)
            };
            if (picker.ShowDialog() != true)
            {
                return;
            }
            var outputFile = picker.FileName;
            try
            {
                Zip.DecodeDeflatedFile(DecodeFile, outputFile);
                MessageBox.Show(LocalizedLangExtension.GetString("converterSuccess"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                MessageBox.Show(LocalizedLangExtension.GetString("converterError"));
                File.Delete(outputFile);
            }
        }

        private bool EnableRecover(object? _)
        {
            return false;
        }

        private async void TapRecover(object? _)
        {
            IsPaused = false;
            crackerTask = GetCracker();
            var res = await crackerTask.RecoverPasswordAsync(InternalKey!, PasswordRule);
            if (string.IsNullOrEmpty(res))
            {
                MessageBox.Show(LocalizedLangExtension.GetString("recoverError"));
            }
            else
            {
                MessageBox.Show(res, LocalizedLangExtension.GetString("recoverSuccess"));
            }
            IsPaused = true;
        }

        private bool EnableConverter(object? _)
        {
            return false;
        }

        private async void TapConverter(object? _)
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
            IsPaused = false;
            crackerTask = GetCracker();
            var res = await crackerTask.PackAsync(InternalKey!, CipherArchiveFile,
                picker.FileName,
                PasswordNew);
            MessageBox.Show(LocalizedLangExtension.GetString(res ?
                "converterSuccess" : "converterError"));
            IsPaused = true;
        }

        private bool EnableDecompress(object? _)
        {
            return InternalKey is not null;
        }

        private void TapDecompress(object? _)
        {
            if (string.IsNullOrWhiteSpace(CipherSelectedName))
            {
                MessageBox.Show(LocalizedLangExtension.GetString("selectAFileTip"));
                return;
            }
            Unpack(true);
        }

        private bool EnableDecompressFiles(object? _)
        {
            return EnableDecompress(_);
        }

        private void TapDecompressFiles(object? _)
        {
            Unpack();
        }

    }
}