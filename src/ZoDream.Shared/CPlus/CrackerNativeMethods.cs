using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.CPlus
{
    public delegate bool CallBackHandler(int progress, int total, string msg);

    public static class CrackerNativeMethods
    {
        [DllImport("cracker.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern KeyStruct FindKey(string zipFile, string zipFileName, string plainFile, string plainFileName, CallBackHandler callBack);

        [DllImport("cracker.dll", EntryPoint="FindKey2", CallingConvention = CallingConvention.Cdecl)]
        public static extern KeyStruct FindKey(string cipherFile, long cipherBegin, long cipherEnd, string plainFile, long plainBegin, long plainEnd, CallBackHandler callback);

        [DllImport("cracker.dll", EntryPoint = "FindKey3", CallingConvention = CallingConvention.Cdecl)]
        public static extern KeyStruct FindKey(string cipherFile, string cipherFileName, string plainFile, CallBackHandler callback);


        [DllImport("cracker.dll", EntryPoint = "FindKey4", CallingConvention = CallingConvention.Cdecl)]
        public static extern KeyStruct FindKey(string cipherFile, string cipherFileName, byte[] plainData, int plainLength, CallBackHandler callback);


        [DllImport("cracker.dll", EntryPoint = "FindKey5", CallingConvention = CallingConvention.Cdecl)]
        public static extern KeyStruct FindKey(string cipherFile, long cipherBegin, long cipherEnd, byte[] plainData, int plainLength, CallBackHandler callback);

        [DllImport("cracker.dll", EntryPoint = "FindKey6", CallingConvention = CallingConvention.Cdecl)]
        public static extern KeyStruct FindKey(byte[] cipherData, int cipherLength, byte[] plainData, int plainLength, CallBackHandler callback);


        [DllImport("cracker.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Pack(KeyStruct keys, string cipherFile, string distFile, CallBackHandler callback);


        [DllImport("cracker.dll", EntryPoint = "Pack2", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Pack(KeyStruct keys, string cipherFile, string distFile, string password, CallBackHandler callback);

        [DllImport("cracker.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Recover(KeyStruct keys, int length, string rule, [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder password, CallBackHandler callback);

        [DllImport("cracker.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Unpack(KeyStruct keys, string cipherFile, string distFolder, CallBackHandler callback);

        [DllImport("cracker.dll", EntryPoint = "Unpack3", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Unpack(KeyStruct keys, string cipherFile, string cipherFileName, string distFolder, CallBackHandler callback);

        [DllImport("cracker.dll", EntryPoint = "Unpack2", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Unpack(KeyStruct keys, string cipherFile, long cipherBegin, long cipherEnd, string distFile, CallBackHandler callback);
    }
}
