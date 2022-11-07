using System;
using System.Runtime.InteropServices;

namespace SerialHandler
{
    class PosAPI
    {
        [DllImport(
        "PosAPI.dll",
        CharSet = CharSet.Unicode, CallingConvention =
        CallingConvention.Cdecl
        )]
        [return: MarshalAs(UnmanagedType.BStr)]
        public static extern string put(String message);
        [DllImport(
        "PosAPI.dll",
        CharSet = CharSet.Unicode, CallingConvention =
        CallingConvention.Cdecl
        )]
        [return: MarshalAs(UnmanagedType.BStr)]
        public static extern string returnBill(String message);
        [DllImport("PosAPI.dll")]
        [return: MarshalAs(UnmanagedType.BStr)]
        public static extern string sendData(string value);
        
        public PosAPI(string value)
        {
            sendData(value);
        }
    }
}
