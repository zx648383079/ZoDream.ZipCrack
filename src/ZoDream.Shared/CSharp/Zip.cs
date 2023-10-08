using SharpCompress;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Common.Zip;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Writers;
using SharpCompress.Writers.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            }
        }

        public static bool GetFileDataPosition(FileStream stream, string name, out long begin, out long end)
        {
            return GetFileDataPosition(stream, name, out var _, out begin, out end);
        }

        public static bool GetFileDataPosition(FileStream stream, string name, out IEntry? entry, out long begin, out long end)
        {
            begin = 0;
            end = 0;
            entry = null;
            var reader = ArchiveFactory.Open(stream);
            foreach (var item in reader.Entries)
            {
                if (item.IsDirectory || item.Key != name)
                {
                    continue;
                }
                try
                {
                    item.OpenEntryStream();
                }
                catch (Exception)
                {
                }
                begin = stream.Position;
                end = begin + item.CompressedSize;
                entry = item;
                break;
            }
            if (entry is null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取文件在压缩流中的起始位置
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        private static long GetEntryPosition(IEntry entry)
        {
            var type = typeof(ZipEntry);
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            // var mInfo = type.GetMethod("Display", flags);
            var field = type.GetField("_filePart", flags);
            var part = field.GetValue(entry);
            var partType = Type.GetType("SharpCompress.Common.Zip.SeekableZipFilePart,SharpCompress");
            object header;
            if (partType.IsAssignableFrom(part.GetType()))
            {
                var headerField = partType.GetMethod("get_Header", flags);
                header = headerField.Invoke(part, new object[0]);
            } else
            {
                header = part;
            }
            var headerType = Type.GetType("SharpCompress.Common.Zip.Headers.ZipFileEntry,SharpCompress");
            var positionField = headerType.GetMethod("get_DataStartPosition", flags);
            var pos = positionField.Invoke(header, new object[0]);
            return pos is null ? 0L : (long)pos;
            // return (long)mInfo.Invoke(entry, new object[] { "Hello" });
        }

        public static bool GetFileDataPosition(FileStream stream, IEntry item, out long begin, out long end)
        {
            //begin = GetEntryPosition(item);
            //end = begin + item.CompressedSize;
            GetFileDataPosition(stream, item.Key, out begin, out end);
            return true;
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
            return GetEntries(fs).Select(item => new FileItem(item.Key, item.Crc.ToString("X"), item.Size)).ToList();
        }

        public static IList<IEntry> GetEntries(FileStream fs)
        {
            var items = new List<IEntry>();
            var reader = ArchiveFactory.Open(fs);
            foreach (var entry in reader.Entries)
            {
                if (entry.IsDirectory)
                {
                    continue;
                }
                items.Add(entry);
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
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void InflateFile(string inputFile, string outputFile)
        {
            if (inputFile == outputFile)
            {
                throw new ArgumentException("Input file cannot be equal to output file");
            }
            using var inputFs = File.OpenRead(inputFile);
            using var outputFs = File.OpenWrite(outputFile);
            InflateFile(inputFs, outputFs);
        }
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="outputStream"></param>
        public static void InflateFile(Stream inputStream, Stream outputStream)
        {
            var inflator = new DeflateStream(inputStream, CompressionMode.Decompress);
            inflator.TransferTo(outputStream);
        }
        /// <summary>
        /// 加密, 存在问题，跟 7z 压缩有区别
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="outputStream"></param>
        public static void DeflateFile(Stream inputStream, Stream outputStream)
        {
            var deflatorStream = new DeflateStream(outputStream, CompressionMode.Compress, CompressionLevel.Level0);
            inputStream.TransferTo(deflatorStream);
        }

        public static void DeflateFile(string file, Stream outputStream)
        {
            using var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            DeflateFile(fs, outputStream);
        }
        /// <summary>
        /// /加密
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="outputStream"></param>
        public static void DeflateByte(byte[] input, Stream outputStream)
        {
            var deflatorStream = new DeflateStream(outputStream, CompressionMode.Compress, CompressionLevel.Default);
            deflatorStream.Write(input, 0, input.Length);
        }

        public static byte[] DeflateText(string val, Encoding encoding)
        {
            using var ms = new MemoryStream();
            DeflateByte(encoding.GetBytes(val), ms);
            return ms.GetBuffer();
        }

        public static void DeflateFile(string fileName, string output)
        {
            using var fs = File.Create(output);
            using var writer = WriterFactory.Open(fs, ArchiveType.Zip, new WriterOptions(CompressionType.Deflate));
            writer.Write(Path.GetFileName(fileName), fileName);
        }
    }
}
