using System;
using System.Collections.Generic;
using System.Text;

namespace ZoDream.Shared.Crack
{
    public class CrackData
    {
        public const int ENCRYPTION_HEADER_SIZE = 12;

        public IList<byte> CipherText { get; set; } = new List<byte>();
        public IList<byte> PlainText { get; set; } = new List<byte>();
        public IList<byte> KeyStream { get; set; } = new List<byte>();

        public int Offset { get; set; } = 0;

        public IList<Tuple<int, byte>> ExtraPlainText { get; set; } = new  List<Tuple<int, byte>>();

        /// <summary>
        /// 更新 KeyStream
        /// </summary>
        public void Update()
        {
            //for (int i = 0; i < PlainText.Count; i++)
            //{
            //    KeyStream.Add((byte)(PlainText[i] ^ CipherText[i + CrackData.ENCRYPTION_HEADER_SIZE + Offset]));
            //}

            Offset = ENCRYPTION_HEADER_SIZE + Offset;

            for (int i = 0; i < PlainText.Count; i++)
            {
                KeyStream.Add((byte)(PlainText[i] ^ CipherText[i + Offset]));
            }
        }
    }
}
