using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZoDream.Shared.Interfaces
{
    public interface IExtractor
    {
        public bool Extract(string fileName, string saveFolder);
        public bool Extract(string fileName, string password, string saveFolder);

        public Task<string?> ExtractAsync(string fileName, IPasswordProvider provider, 
            string saveFolder, CancellationToken token = default);
    }
}
