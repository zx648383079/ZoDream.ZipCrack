﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using ZoDream.Shared.CSharp;
using ZoDream.ZipCrack.Utils;

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
        public ICommand CrackCommand { get; private set; }
        public ICommand ClearCommand { get; private set; }

        public bool EnableGet => !(string.IsNullOrWhiteSpace(CipherArchiveFile) ||
                string.IsNullOrWhiteSpace(CipherSelectedName) ||
                (ModeIndex < 1 && (string.IsNullOrWhiteSpace(PlainArchiveFile) ||
                string.IsNullOrWhiteSpace(PlainSelectedName))) ||
                (ModeIndex == 1 && string.IsNullOrWhiteSpace(PlainFile)) ||
                (ModeIndex == 2 && string.IsNullOrWhiteSpace(PlainText)));

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

        public bool EnableDecode => !string.IsNullOrWhiteSpace(DecodeFile);

        private void TapDecode(object? _)
        {
            var picker = new Microsoft.Win32.SaveFileDialog()
            {
                FileName = Path.GetFileName(DecodeFile)
            };
            if (picker.ShowDialog() != true)
            {
                return;
            }
            var outputFile = picker.FileName;
            try
            {
                Zip.InflateFile(DecodeFile, outputFile);
                MessageBox.Show(LocalizedLangExtension.GetString("converterSuccess"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                MessageBox.Show(LocalizedLangExtension.GetString("converterError"));
                File.Delete(outputFile);
            }
        }

        public bool EnableRecover => !string.IsNullOrWhiteSpace(PasswordRule) && InternalKey is not null;

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

        public bool EnableConverter => !string.IsNullOrWhiteSpace(CipherArchiveFile) && InternalKey is not null;

        private async void TapConverter(object? _)
        {
            var picker = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "ZIP|*.zip|All Files|*.*",
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

        public bool EnableDecompress => !string.IsNullOrWhiteSpace(CipherArchiveFile) 
            && !string.IsNullOrWhiteSpace(CipherSelectedName)
            && InternalKey is not null;

        private void TapDecompress(object? _)
        {
            if (string.IsNullOrWhiteSpace(CipherSelectedName))
            {
                MessageBox.Show(LocalizedLangExtension.GetString("selectAFileTip"));
                return;
            }
            Unpack(true);
        }

        public bool EnableDecompressFiles => !string.IsNullOrWhiteSpace(CipherArchiveFile) && InternalKey is not null;

        private void TapDecompressFiles(object? _)
        {
            Unpack();
        }

        public bool EnableCrack => !string.IsNullOrWhiteSpace(CipherArchiveFile) && ((ModeIndex == 7 && !string.IsNullOrWhiteSpace(PasswordRule)) ||
            (ModeIndex == 8 && !string.IsNullOrWhiteSpace(DictionaryFile)));

        public async void TapCrack(object? _)
        {
            Stop();
            //var folder = new System.Windows.Forms.FolderBrowserDialog
            //{
            //    SelectedPath = Path.GetDirectoryName(CipherArchiveFile)!,
            //    ShowNewFolderButton = true,
            //    Description = LocalizedLangExtension.GetString("unzipBtnContent"),
            //};
            //if (folder.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            //{
            //    return;
            //}
            var saveFolder = string.Empty;//folder.SelectedPath;
            tokenSource = new System.Threading.CancellationTokenSource();
            IsPaused = false;
            var extractor = new ArchiveExtractor(Logger);
            var password = string.Empty;
            if (ModeIndex == 7)
            {
                password = await extractor.TryExtractAsync(CipherArchiveFile, PasswordRule, PasswordOffset, saveFolder, tokenSource.Token);
            } else if (ModeIndex == 8)
            {
                password = await extractor.TryExtractWidthDictionaryAsync(CipherArchiveFile, DictionaryFile, PasswordOffset, saveFolder, tokenSource.Token);
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                IsPaused = true;
                return;
            }
            var res = MessageBox.Show($"Found password: {password}\n Does you need to unpack.", 
                "Found Successfully", MessageBoxButton.YesNo);
            if (res != MessageBoxResult.Yes) 
            {
                IsPaused = true;
                return;
            }
            var folder = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = Path.GetDirectoryName(CipherArchiveFile)!,
                ShowNewFolderButton = true,
            };
            if (folder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                await extractor.ExtractAsync(CipherArchiveFile, password, folder.SelectedPath, tokenSource.Token);
            }
            IsPaused = true;
        }
    }
}