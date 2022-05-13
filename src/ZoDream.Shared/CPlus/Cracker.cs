using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.CPlus
{
    public class Cracker : ICracker
    {
        public Cracker()
        {

        }

        public Cracker(ILogger logger)
        {
            Logger = logger;
        }

        public ILogger? Logger { get; private set; }

        private CancellationTokenSource stopToken = new();

        public bool Paused => stopToken.IsCancellationRequested;

        private CancellationToken StartNew()
        {
            if (Paused)
            {
                stopToken = new CancellationTokenSource();
            }
            return stopToken.Token;
        }

        public Task<KeyItem?> FindKeyAsync(string cipherFile, string cipherFileName, string plainFile, string plainFileName)
        {
            return Task.Factory.StartNew(() =>
            {
                return ParseKey(CrackerNativeMethods.FindKey(cipherFile, cipherFileName, plainFile, plainFileName, Callback));
            }, StartNew());
        }

        public Task<KeyItem?> FindKeyAsync(string cipherFile, long cipherBegin, long cipherEnd, string plainFile, long plainBegin, long plainEnd)
        {
            return Task.Factory.StartNew(() =>
            {
                return ParseKey(CrackerNativeMethods.FindKey(cipherFile, cipherBegin, cipherEnd, plainFile, plainBegin, plainEnd, Callback));
            }, StartNew());
        }

        public Task<KeyItem?> FindKeyAsync(string cipherFile, string cipherFileName, string plainFile)
        {
            return Task.Factory.StartNew(() =>
            {
                return ParseKey(CrackerNativeMethods.FindKey(cipherFile, cipherFileName, plainFile, Callback));
            }, StartNew());
        }

        public Task<KeyItem?> FindKeyAsync(string cipherFile, string cipherFileName, byte[] plainData)
        {
            return Task.Factory.StartNew(() =>
            {
                return ParseKey(CrackerNativeMethods.FindKey(cipherFile, cipherFileName, plainData, plainData.Length, Callback));
            }, StartNew());
        }

        public Task<KeyItem?> FindKeyAsync(string cipherFile, long cipherBegin, long cipherEnd, byte[] plainData)
        {
            return Task.Factory.StartNew(() =>
            {
                return ParseKey(CrackerNativeMethods.FindKey(cipherFile, cipherBegin, cipherEnd, plainData, plainData.Length, Callback));
            }, StartNew());
        }

        public Task<KeyItem?> FindKeyAsync(byte[] cipherData, byte[] plainData)
        {
            return Task.Factory.StartNew(() =>
            {
                return ParseKey(CrackerNativeMethods.FindKey(cipherData, cipherData.Length, plainData, plainData.Length, Callback));
            }, StartNew());
        }

        public Task<bool> PackAsync(KeyItem keys, string cipherFile, string distFile)
        {
            return Task.Factory.StartNew(() =>
            {
                return CrackerNativeMethods.Pack(ParseKey(keys), cipherFile, distFile, Callback);
            }, StartNew());
        }

        public Task<bool> PackAsync(KeyItem keys, string cipherFile, string distFile, string password)
        {
            return Task.Factory.StartNew(() =>
            {
                return CrackerNativeMethods.Pack(ParseKey(keys), cipherFile, distFile, password, Callback);
            }, StartNew());
        }

        public Task<string> RecoverPasswordAsync(KeyItem keys, string rule)
        {
            return Task.Factory.StartNew(() =>
            {
                var ruler = new CSharp.PasswordRule(rule);
                var sb = new StringBuilder();
                var len = CrackerNativeMethods.Recover(ParseKey(keys), ruler.Length, rule, sb, Callback);
                return len > 0 ? sb.ToString().Substring(0, len) : string.Empty;
            }, StartNew());
        }

        public void Stop()
        {
            stopToken.Cancel();
        }

        public Task<bool> UnpackAsync(KeyItem keys, string cipherFile, string distFolder)
        {
            return Task.Factory.StartNew(() =>
            {
                return CrackerNativeMethods.Unpack(ParseKey(keys), cipherFile, distFolder, Callback);
            }, StartNew());
        }

        public Task<bool> UnpackAsync(KeyItem keys, string cipherFile, string cipherFileName, string distFolder)
        {
            return Task.Factory.StartNew(() =>
            {
                var distFile = Path.Combine(distFolder, cipherFileName);
                return CrackerNativeMethods.Unpack(ParseKey(keys), cipherFile, cipherFileName, distFile, Callback);
            }, StartNew());
        }

        public Task<bool> UnpackAsync(KeyItem keys, string cipherFile, long cipherBegin, long cipherEnd, string distFile)
        {
            return Task.Factory.StartNew(() =>
            {
                return CrackerNativeMethods.Unpack(ParseKey(keys), cipherFile, cipherBegin, cipherEnd, distFile, Callback);
            }, StartNew());
        }

        private KeyItem? ParseKey(KeyStruct item)
        {
            if (item.x == 0 && item.y == 0 && item.z == 0)
            {
                return null;
            }
            return new KeyItem(item.x, item.y, item.z);
        }

        private KeyStruct ParseKey(KeyItem item)
        {
            return new KeyStruct() { x = item.X, y = item.Y, z = item.Z };
        }

        public bool Callback(int progress, int total, string msg)
        {
            if (total < 0)
            {
                Logger?.Info(msg);//(string)Marshal.PtrToStructure(msg, typeof(string)));
            } else
            {
                Logger?.Progress(progress, total);
            }
            return !Paused;
        }
    }
}
