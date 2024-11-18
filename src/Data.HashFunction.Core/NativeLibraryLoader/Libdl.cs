using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Data.HashFunction.Core.LibraryLoader
{
    internal static class Libdl
    {
        [DllImport("libdl.so.2", EntryPoint = "dlopen", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dlopen_linux([MarshalAs(UnmanagedType.LPStr)] string fileName, int flags);

		[DllImport("libdl.so.2", EntryPoint = "dlsym", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr dlsym_linux(IntPtr handle, string name);

		[DllImport("libdl.so.2", EntryPoint = "dlclose", CallingConvention = CallingConvention.Cdecl)]
		public static extern int dlclose_linux(IntPtr handle);

		[DllImport("libdl.so.2", EntryPoint = "dlerror", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr dlerror_linux();


		[DllImport("libdl.dylib", EntryPoint = "dlopen", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr dlopen_osx([MarshalAs(UnmanagedType.LPStr)] string fileName, int flags);

		[DllImport("libdl.dylib", EntryPoint = "dlsym", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr dlsym_osx(IntPtr handle, string name);

		[DllImport("libdl.dylib", EntryPoint = "dlclose", CallingConvention = CallingConvention.Cdecl)]
		public static extern int dlclose_osx(IntPtr handle);

		[DllImport("libdl.dylib", EntryPoint = "dlerror", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr dlerror_osx();

		public const int RTLD_NOW = 0x002;

		public static string DLError()
		{
			IntPtr ptr = IntPtr.Zero;

			OperatingSystemHelper.PlatformType pt = OperatingSystemHelper.GetCurrentPlatfom();
			if (pt == OperatingSystemHelper.PlatformType.Linux || pt == OperatingSystemHelper.PlatformType.Android)
				ptr = dlerror_linux();
			else if (pt == OperatingSystemHelper.PlatformType.MacOS || pt == OperatingSystemHelper.PlatformType.iOS)
				ptr = dlerror_osx();

			if (ptr == IntPtr.Zero)
				return null;

			string str = Marshal.PtrToStringAnsi(ptr);
			return str.Trim();
		}
	}
}

