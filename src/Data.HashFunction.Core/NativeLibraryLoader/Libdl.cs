using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Data.HashFunction.Core.LibraryLoader
{
    internal static class Libdl
    {
        [DllImport("libdl.so", EntryPoint = "dlopen")]
        public static extern IntPtr dlopen_linux(string fileName, int flags);

		[DllImport("libdl.so", EntryPoint = "dlsym")]
		public static extern IntPtr dlsym_linux(IntPtr handle, string name);

		[DllImport("libdl.so", EntryPoint = "dlclose")]
		public static extern int dlclose_linux(IntPtr handle);

		[DllImport("libdl.so", EntryPoint = "dlerror")]
		public static extern string dlerror_linux();


		[DllImport("libdl.dylib", EntryPoint = "dlopen")]
		public static extern IntPtr dlopen_osx(string fileName, int flags);

		[DllImport("libdl.dylib", EntryPoint = "dlsym")]
		public static extern IntPtr dlsym_osx(IntPtr handle, string name);

		[DllImport("libdl.dylib", EntryPoint = "dlclose")]
		public static extern int dlclose_osx(IntPtr handle);

		[DllImport("libdl.dylib", EntryPoint = "dlerror")]
		public static extern string dlerror_osx();

		public const int RTLD_NOW = 0x002;
    }
}

