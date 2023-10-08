using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.CSharp;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Interfaces
{
    public interface ICracker
    {

        public ILogger? Logger { get; }
        public bool Paused { get; }

        public void Stop();
        /// <summary>
        /// 获取key
        /// </summary>
        /// <param name="cipherFile"></param>
        /// <param name="cipherFileName"></param>
        /// <param name="plainFile"></param>
        /// <param name="plainFileName"></param>
        /// <returns></returns>
        public Task<KeyItem?> FindKeyAsync(string cipherFile, string cipherFileName, string plainFile, string plainFileName);
        /// <summary>
        /// 获取key
        /// </summary>
        /// <param name="cipherFile"></param>
        /// <param name="cipherBegin"></param>
        /// <param name="cipherEnd"></param>
        /// <param name="plainFile"></param>
        /// <param name="plainBegin"></param>
        /// <param name="plainEnd"></param>
        /// <returns></returns>
        public Task<KeyItem?> FindKeyAsync(string cipherFile, long cipherBegin, long cipherEnd, string plainFile, long plainBegin, long plainEnd);
        /// <summary>
        /// 获取key
        /// </summary>
        /// <param name="cipherFile"></param>
        /// <param name="cipherFileName"></param>
        /// <param name="plainFile">已知的明文文件</param>
        /// <returns></returns>
        public Task<KeyItem?> FindKeyAsync(string cipherFile, string cipherFileName, string plainFile);
        /// <summary>
        /// 获取key
        /// </summary>
        /// <param name="cipherFile"></param>
        /// <param name="cipherFileName"></param>
        /// <param name="plainData">已知的明文字符串</param>
        /// <returns></returns>
        public Task<KeyItem?> FindKeyAsync(string cipherFile, string cipherFileName, byte[] plainData);
        /// <summary>
        /// 获取key
        /// </summary>
        /// <param name="cipherFile"></param>
        /// <param name="cipherBegin"></param>
        /// <param name="cipherEnd"></param>
        /// <param name="plainData"></param>
        /// <returns></returns>
        public Task<KeyItem?> FindKeyAsync(string cipherFile, long cipherBegin, long cipherEnd, byte[] plainData);
        /// <summary>
        /// 获取key
        /// </summary>
        /// <param name="cipherData"></param>
        /// <param name="plainData"></param>
        /// <returns></returns>
        public Task<KeyItem?> FindKeyAsync(byte[] cipherData, byte[] plainData);

        /// <summary>
        /// 根据key获取压缩文件
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="cipherFile"></param>
        /// <param name="targetFolder"></param>
        /// <returns></returns>
        public Task<bool> UnpackAsync(KeyItem keys, string cipherFile, string targetFolder);
        /// <summary>
        /// 根据key获取压缩文件
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="cipherFile"></param>
        /// <param name="cipherFileName"></param>
        /// <param name="targetFolder"></param>
        /// <returns></returns>
        public Task<bool> UnpackAsync(KeyItem keys, string cipherFile, string cipherFileName, string targetFolder);
        /// <summary>
        /// 根据key获取压缩文件
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="cipherFile"></param>
        /// <param name="cipherBegin"></param>
        /// <param name="cipherEnd"></param>
        /// <param name="targetFile"></param>
        /// <returns></returns>
        public Task<bool> UnpackAsync(KeyItem keys, string cipherFile, long cipherBegin, long cipherEnd, string targetFile);
        /// <summary>
        /// 根据keys把压缩包转成不设密码的压缩包
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="cipherFile"></param>
        /// <param name="targetFile"></param>
        /// <returns></returns>
        public Task<bool> PackAsync(KeyItem keys, string cipherFile, string targetFile);
        /// <summary>
        /// 根据keys把压缩包转成已知密码的压缩包
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="cipherFile"></param>
        /// <param name="targetFile"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public Task<bool> PackAsync(KeyItem keys, string cipherFile, string targetFile, string password);
        /// <summary>
        /// 根据keys修复密码
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public Task<string> RecoverPasswordAsync(KeyItem keys, string rule);
    }
}
