using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.CSharp
{
    public static class Zip
    {
        public static string CodePage
        {
            set
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                try
                {
                    ZipStrings.CodePage = Encoding.GetEncoding(value).CodePage;
                }
                catch (Exception)
                {
                    ZipStrings.UseUnicode = true;
                }
            }
        }

        public static bool GetFileDataPosition(FileStream stream, string name, out long begin, out long end)
        {
            return GetFileDataPosition(stream, name, out var _, out begin, out end);
        }

        public static bool GetFileDataPosition(FileStream stream, string name, out ZipEntry? entry, out long begin, out long end)
        {
            begin = 0;
            end = 0;
            // RegisterEncoding();
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


        public static string[] GetEncodings()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return Encoding.GetEncodings().Select(i => i.Name).ToArray();
        }

        public static string DefaultEncoding()
        {
            var lang = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
            if (lang == "zh-CN" || lang == "zh-TW")
            {
                return "gb2312";
            }
            return "utf-8";
        }

        public static void DecodeDeflatedFile(Stream inputStream, Stream outputStream)
        {
            var inflater = new InflaterInputStream(inputStream, new Inflater(true));
            int size;
            var data = new byte[4096];
            while (true)
            {
                size = inflater.Read(data, 0, data.Length);
                if (size > 0)
                {
                    outputStream.Write(data, 0, size);
                }
                else
                {
                    break;
                }
            }
            inflater.Close();
        }

        public static void DecodeDeflatedFile(string inputFile, string outputFile)
        {
            if (inputFile == outputFile)
            {
                throw new ArgumentException("Input file cannot be equal to output file");
            }
            using var inputFs = File.OpenRead(inputFile);
            using var outputFs = File.OpenWrite(outputFile);
            DecodeDeflatedFile(inputFs, outputFs);
        }
    }
}
