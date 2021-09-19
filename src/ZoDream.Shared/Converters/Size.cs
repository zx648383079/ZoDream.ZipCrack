using System;
using System.Collections.Generic;
using System.Text;

namespace ZoDream.Shared.Converters
{
    public static class Size
    {
        public static string Format(long v)
        {
            if (v < 0)
            {
                return "0";
            }
            else if (v >= 1024 * 1024 * 1024) //文件大小大于或等于1024MB
            {
                return string.Format("{0:0.00} GB", (double)v / (1024 * 1024 * 1024));
            }
            else if (v >= 1024 * 1024) //文件大小大于或等于1024KB
            {
                return string.Format("{0:0.00} MB", (double)v / (1024 * 1024));
            }
            else if (v >= 1024) //文件大小大于等于1024bytes
            {
                return string.Format("{0:0.00} KB", (double)v / 1024);
            }
            else
            {
                return string.Format("{0:0.00} B", v);
            }
        }
    }
}
