using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using ZoDream.Shared.CSharp;
using ZoDream.Shared.Models;
using ZoDream.Shared.ViewModel;

namespace ZoDream.ZipCrack.ViewModels
{
    public class MainViewModel: BindableBase
    {

        private string[] encodingItems = Zip.GetEncodings();
        public string[] EncodingItems
        {
            get => encodingItems;
            set => Set(ref encodingItems, value);
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

    }
}