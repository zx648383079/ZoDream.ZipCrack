using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.Crack;

namespace ZoDream.Shared.Interfaces
{
    public interface ICracker
    {

        public ILogger? Logger { get; }
        public bool Paused { get; }

        public void Stop();

        public Task<KeyItem?> FindKeyAsync(string cipherFile, string cipherFileName, string plainFile, string plainFileName);

        public Task<bool> UnpackAsync(KeyItem keys, string cipherFile, string distFolder);
    }
}
