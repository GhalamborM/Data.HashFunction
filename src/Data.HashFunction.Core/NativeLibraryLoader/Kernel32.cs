using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Data.HashFunction.Core.LibraryLoader
{
    internal static class Kernel32
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string fileName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr module, string procName);

        [DllImport("kernel32.dll")]
        public static extern int FreeLibrary(IntPtr module);
    }
}

