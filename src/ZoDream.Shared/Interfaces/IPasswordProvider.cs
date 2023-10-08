using System;
using System.Collections.Generic;
using System.Text;

namespace ZoDream.Shared.Interfaces
{
    public interface IPasswordProvider: IDisposable
    {

        public long Position { get; }
        public long Count { get; }
        public bool HasMore { get; }

        public string Next();
    }
}
