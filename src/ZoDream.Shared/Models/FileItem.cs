using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Shared.Models
{
    public class FileItem
    {
        public string Name { get; set; }

        public string CRC32 { get; set; }

        public long Size { get; set; } = 0;


        public FileItem(string name, string crc32)
        {
            Name = name;
            CRC32 = crc32;
        }

        public FileItem(string name, string crc32, long size)
        {
            Name = name;
            CRC32 = crc32;
            Size = size;
        }
    }
}
