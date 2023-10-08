using System.Collections.Generic;
using System.IO;
using System.Text;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Storage;

namespace ZoDream.Shared.CSharp
{
    public class PasswordDictionary : IPasswordProvider
    {
        public PasswordDictionary(string fileName): this(new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
            
        }

        public PasswordDictionary(FileStream stream): this(stream, 0)
        {
            
        }

        public PasswordDictionary(FileStream stream, long position)
        {
            BaseStream = stream;
            Encoding = TxtEncoder.GetEncoding(BaseStream);
            var beginPosition = 0L;
            while (true)
            {
                var code = stream.ReadByte();
                if (code == -1)
                {
                    Count++;
                    if (Count == position)
                    {
                        beginPosition = stream.Position;
                    }
                    break;
                }
                if (code == 0x0A)
                {
                    Count++;
                    if (Count == position)
                    {
                        beginPosition = stream.Position;
                    }
                    continue;
                }
                if (code != 0x0D)
                {
                    continue;
                }
                Count++;
                var offset = BaseStream.ReadByte() == 0x0A ? 0 : -1;
                if (Count == position)
                {
                    beginPosition = stream.Position + offset;
                }
            }
            BaseStream.Seek(beginPosition, SeekOrigin.Begin);
            Position = position - 1;
        }
        private readonly FileStream BaseStream;
        private readonly Encoding Encoding;

        public long Position { get; private set; } = -1;

        public long Count { get; private set; }

        public bool HasMore => Position < Count - 1;

        public void Dispose()
        {
            BaseStream.Dispose();
        }

        public string Next()
        {
            if (!HasMore)
            {
                return string.Empty;
            }
            Position++;
            return ReadLine();
        }

        private string ReadLine()
        {
            var buffer = new List<byte>();
            while (true)
            {
                var code = BaseStream.ReadByte();
                if (code == -1)
                {
                    break;
                }
                if (code == 0x0A)
                {
                    break;
                }
                if (code != 0x0D)
                {
                    buffer.Add((byte)code);
                    continue;
                }
                var pos = BaseStream.Position;
                code = BaseStream.ReadByte();
                if (code != 0x0A && code != 0x0D)
                {
                    BaseStream.Seek(pos, SeekOrigin.Begin);
                }
                break;
            }
            return Encoding.GetString(buffer.ToArray());
        }
    }
}
