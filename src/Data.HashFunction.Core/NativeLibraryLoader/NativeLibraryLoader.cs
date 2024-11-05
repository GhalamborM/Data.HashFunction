using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using static Data.HashFunction.Core.LibraryLoader.OperatingSystemHelper;

namespace Data.HashFunction.Core.LibraryLoader
{
	/// <summary>
	/// Native library loader for fast native parts of some hash algorithms.
	/// </summary>
	public abstract class NativeLibraryLoader : IDisposable
    {
        protected readonly string libraryName;
        protected readonly IntPtr libraryHandle;

		/// <summary>
		/// Initializes a new instance of <see cref="NativeLibraryLoader"/> for use with the given library name/path. This will automatically load the given library correctly for the current running operating system and architecture.
		/// </summary>
		/// <param name="library">The name/path of the library to be loaded.</param>
		/// <exception cref="ArgumentNullException">Thrown when the given parameter is null.</exception>
		/// <exception cref="FileLoadException">Thrown when the given library could not be loaded either because it is not found or invalid for the current operating system and architecture.</exception>
		public NativeLibraryLoader(string library)
        {
            _ = library ?? throw new ArgumentNullException(nameof(library));

            string ext = Path.GetExtension(library);
            if (ext == string.Empty)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
					library += ".dll";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
					library += ".so";
				}
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
					library += ".dylib";
				}
            }

			this.libraryName = library;
            libraryHandle = LoadLibrary(this.libraryName);

            if (libraryHandle == IntPtr.Zero)
            {
                string p = Path.Combine(Path.GetDirectoryName(this.libraryName), "runtimes", Path.GetFileName(this.libraryName));
				libraryHandle = LoadLibrary(p);
			}

			if (libraryHandle == IntPtr.Zero)
			{
                string fn = "";

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    fn = "win";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
					fn = "linux";
				}
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
					fn = "osx";
				}

				if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
                {
                    fn += "-x86";
                }
                else if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
				{
					fn += "-x64";
				}
				else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm)
				{
					fn += "-arm";
				}
				else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
				{
					fn += "-arm64";
				}

				string p = Path.Combine(Path.GetDirectoryName(this.libraryName), "runtimes", fn, Path.GetFileName(this.libraryName));
				libraryHandle = LoadLibrary(p);

				if (libraryHandle == IntPtr.Zero)
                {
					p = Path.Combine(Path.GetDirectoryName(this.libraryName), "runtimes", fn, "native", Path.GetFileName(this.libraryName));
					libraryHandle = LoadLibrary(p);
				}
			}

			if (libraryHandle == IntPtr.Zero)
            {
                throw new FileLoadException("Could not load " + libraryName);
            }
        }

        protected abstract IntPtr LoadLibrary(string libraryName);
        protected abstract void FreeLibrary(IntPtr libraryHandle);

        /// <summary>
        /// Tries to load a function with the given name.
        /// </summary>
        /// <param name="functionName">The name of the function (found in the currently loaded library) to load.</param>
        /// <returns>Returns the handle of the loaded function, or a <see cref="IntPtr.Zero"/> if the function was not found.</returns>
        public abstract IntPtr LoadFunction(string functionName);

		/// <summary>
		/// Tries to load a function with the given name. This function also outputs a delegate that is converted from a function pointer using <seealso cref="Marshal.GetFunctionPointerForDelegate{TDelegate}(TDelegate)"/>.
		/// </summary>
		/// <typeparam name="T">The type of the delegate to convert the function pointer to.</typeparam>
		/// <param name="name">The name of the function (found in the currently loaded library) to load.</param>
		/// <param name="field">The delegate output of the converted function pointer.</param>
		public unsafe void LoadFunction<T>(string name, out T field)
        {
            IntPtr funcPtr = LoadFunction(name);
            if (funcPtr != IntPtr.Zero)
            {
                field = Marshal.GetDelegateForFunctionPointer<T>(funcPtr);
            }
            else
            {
                field = default(T);
            }
        }

        /// <summary>
        /// Checks whether the given function exists in the currently loaded library or not.
        /// </summary>
        /// <param name="name">The name of the function to check.</param>
        /// <returns>Returns <see langword="true"/> if the function exists in the currently loaded library, otherwise <see langword="false"/>.</returns>
        public unsafe bool IsFunctionAvailable(string name)
        {
			IntPtr funcPtr = LoadFunction(name);
            return funcPtr != IntPtr.Zero;
		}

        /// <summary>
        /// Frees the loaded library.
        /// </summary>
        public void Dispose()
        {
            FreeLibrary(libraryHandle);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NativeLibraryLoader"/> with the given <paramref name="library"/> parameter.
        /// </summary>
        /// <param name="library">The library to be loaded.</param>
        /// <returns>Returns the initialized instance of <see cref="NativeLibraryLoader"/></returns>
        /// <exception cref="PlatformNotSupportedException">Thrown when the current operating system is not *yet* supported.</exception>
        public static NativeLibraryLoader Load(string library)
        {
            if (IsOSPlatform(PlatformType.Windows))
            {
                return new WindowsNativeLibrary(library);
            }
            else if (IsOSPlatform(PlatformType.Android) || IsOSPlatform(PlatformType.Linux))
            {
                return new LinuxNativeLibrary(library);
            }
            else if (IsOSPlatform(PlatformType.MacOS))
            {
				return new OSXNativeLibrary(library);
			}
            else
            {
                throw new PlatformNotSupportedException("Cannot load native libraries on this platform: " + RuntimeInformation.OSDescription);
            }
        }

        private class WindowsNativeLibrary : NativeLibraryLoader
		{
            public WindowsNativeLibrary(string library) : base(library)
            {
            }

            protected override IntPtr LoadLibrary(string libraryName)
            {
                return Kernel32.LoadLibrary(libraryName);
            }

            protected override void FreeLibrary(IntPtr libraryHandle)
            {
                Kernel32.FreeLibrary(libraryHandle);
            }

            public override IntPtr LoadFunction(string functionName)
            {
                return Kernel32.GetProcAddress(libraryHandle, functionName);
            }
        }

        private class LinuxNativeLibrary : NativeLibraryLoader
		{
            public LinuxNativeLibrary(string library) : base(library)
            {
            }

            protected override IntPtr LoadLibrary(string libraryName)
            {
                Libdl.dlerror_linux();
                IntPtr handle = Libdl.dlopen_linux(libraryName, Libdl.RTLD_NOW);
                if (handle == IntPtr.Zero && !Path.IsPathRooted(libraryName))
                {
                    string baseDir = AppContext.BaseDirectory;
                    if (!string.IsNullOrWhiteSpace(baseDir))
                    {
                        string localPath = Path.Combine(baseDir, libraryName);
                        handle = Libdl.dlopen_linux(localPath, Libdl.RTLD_NOW);
                    }
                }

                return handle;
            }

            protected override void FreeLibrary(IntPtr libraryHandle)
            {
                Libdl.dlclose_linux(libraryHandle);
            }

            public override IntPtr LoadFunction(string functionName)
            {
                return Libdl.dlsym_linux(libraryHandle, functionName);
            }
        }

		private class OSXNativeLibrary : NativeLibraryLoader
		{
			public OSXNativeLibrary(string library) : base(library)
			{
			}

			protected override IntPtr LoadLibrary(string libraryName)
			{
				Libdl.dlerror_osx();
				IntPtr handle = Libdl.dlopen_osx(libraryName, Libdl.RTLD_NOW);
				if (handle == IntPtr.Zero && !Path.IsPathRooted(libraryName))
				{
					string baseDir = AppContext.BaseDirectory;
					if (!string.IsNullOrWhiteSpace(baseDir))
					{
						string localPath = Path.Combine(baseDir, libraryName);
						handle = Libdl.dlopen_osx(localPath, Libdl.RTLD_NOW);
					}
				}

				return handle;
			}

			protected override void FreeLibrary(IntPtr libraryHandle)
			{
				Libdl.dlclose_osx(libraryHandle);
			}

			public override IntPtr LoadFunction(string functionName)
			{
				return Libdl.dlsym_osx(libraryHandle, functionName);
			}
		}
	}
}

