using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZoDream.Shared.Models;

namespace ZoDream.Shared
{
    public static class Zip
    {

        public static bool GetFileDataPosition(FileStream stream, string name, out long begin, out long end)
        {
            return GetFileDataPosition(stream, name, out var _, out begin, out end);
        }

        public static bool GetFileDataPosition(FileStream stream, string name, out ZipEntry? entry, out long begin, out long end)
        {
            begin = 0;
            end = 0;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ZipStrings.CodePage = Encoding.GetEncoding("gb2312").CodePage;
            using (var zipFile = new ZipFile(stream))
            {
                zipFile.IsStreamOwner = false;
                var item = zipFile.GetEntry(name);
                if (item == null)
                {
                    entry = null;
                    return false;
                }
                begin = zipFile.LocateEntry(item);
                end = begin + item.CompressedSize;
                entry = item;
                return true;
            }
        }

        public static bool GetFileDataPosition(FileStream stream, ZipEntry item, out long begin, out long end)
        {
            using (var zipFile = new ZipFile(stream))
            {
                zipFile.IsStreamOwner = false;
                item.IsCrypted = false;
                begin = zipFile.LocateEntry(item);
                end = begin + item.CompressedSize;
                return true;
            }
        }


        public static IList<FileItem> GetFiles(string path)
        {
            if (!File.Exists(path))
            {
                return new List<FileItem>();
            }
            return GetFiles(File.OpenRead(path));
        }

        public static IList<FileItem> GetFiles(FileStream fs)
        {
            return GetEntries(fs).Select(item => new FileItem(item.Name, item.Crc.ToString("X"), item.Size)).ToList();
        }

        public static IList<ZipEntry> GetEntries(FileStream fs)
        {
            var items = new List<ZipEntry>();
            // ZipStrings.UseUnicode = true;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ZipStrings.CodePage = Encoding.GetEncoding("gb2312").CodePage;
            using (var stream = new ZipFile(fs))
            {
                stream.IsStreamOwner = false;
                foreach (ZipEntry item in stream)
                {
                    if (!item.IsFile)
                    {
                        continue;
                    }
                    items.Add(item);
                }
            }
            return items;
        }
    }
}
