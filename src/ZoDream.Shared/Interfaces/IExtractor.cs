using System.Threading;
using System.Threading.Tasks;

namespace ZoDream.Shared.Interfaces
{
    public interface IExtractor
    {
        public Task<bool> ExtractAsync(string fileName, 
            string saveFolder,
            CancellationToken token = default);
        public Task<bool> ExtractAsync(string fileName, string password, 
            string saveFolder, 
            CancellationToken token = default);
        /// <summary>
        /// 查看密码对不对
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool TryExtract(string fileName, string password);

        public Task<string?> TryExtractAsync(string fileName, 
            IPasswordProvider provider, 
            string saveFolder, CancellationToken token = default);
    }
}
