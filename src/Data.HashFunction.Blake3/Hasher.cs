// Original project: https://github.com/xoofx/Blake3.NET
// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using Data.HashFunction.Core;
using Data.HashFunction.Core.LibraryLoader;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Data.HashFunction.Blake3
{
	/// <summary>
	/// An incremental hash state that can accept any number of writes.
	/// </summary>
	/// <remarks>
	/// Performance note: The <see cref="Update{T}"/> and <see cref="UpdateWithJoin{T}"/> methods perform poorly when the caller's input buffer is small.
	/// See their method docs below. A 16 KiB buffer is large enough to leverage all currently supported SIMD instruction sets.
	/// </remarks>
	internal unsafe class Hasher : IDisposable
	{
		private delegate void* blake3_new_del();
		private delegate void* blake3_new_keyed_del(void* ptr32Bytes);

		private delegate void* blake3_new_derive_key_del(void* ptr, void* size);

		private delegate void blake3_hash_del(void* ptr, void* size, void* ptrOut);

		private delegate void blake3_hash_preemptive_del(void* ptr, void* size, void* ptrOut);

		private delegate void blake3_delete_del(void* hasher);

		private delegate void blake3_reset_del(void* hasher);

		private delegate void blake3_update_del(void* hasher, void* ptr, void* size);
		private delegate void blake3_update_preemptive_del(void* hasher, void* ptr, void* size);

		private delegate void blake3_update_rayon_del(void* hasher, void* ptr, void* size);

		private delegate void blake3_finalize_del(void* hasher, void* ptr);

		private delegate void blake3_finalize_xof_del(void* hasher, void* ptr, void* size);
		private delegate void blake3_finalize_seek_xof_del(void* hasher, ulong offset, void* ptr, void* size);

		private static readonly blake3_new_del blake3_new;
		private static readonly blake3_new_keyed_del blake3_new_keyed;

		private static readonly blake3_new_derive_key_del blake3_new_derive_key;

		private static readonly blake3_hash_del blake3_hash;

		private static readonly blake3_hash_preemptive_del blake3_hash_preemptive;

		private static readonly blake3_delete_del blake3_delete;

		private static readonly blake3_reset_del blake3_reset;

		private static readonly blake3_update_del blake3_update;
		private static readonly blake3_update_preemptive_del blake3_update_preemptive;

		private static readonly blake3_update_rayon_del blake3_update_rayon;

		private static readonly blake3_finalize_del blake3_finalize;

		private static readonly blake3_finalize_xof_del blake3_finalize_xof;
		private static readonly blake3_finalize_seek_xof_del blake3_finalize_seek_xof;

		private static NativeLibraryLoader lib_loader;
		static Hasher()
		{
			lib_loader = NativeLibraryLoader.Load("blake3_dotnet");

			lib_loader.LoadFunction<blake3_new_del>("blake3_new", out blake3_new);
			lib_loader.LoadFunction<blake3_new_keyed_del>("blake3_new_keyed", out blake3_new_keyed);

			lib_loader.LoadFunction<blake3_new_derive_key_del>("blake3_new_derive_key", out blake3_new_derive_key);

			lib_loader.LoadFunction<blake3_hash_del>("blake3_hash", out blake3_hash);

			lib_loader.LoadFunction<blake3_hash_preemptive_del>("blake3_hash", out blake3_hash_preemptive);

			lib_loader.LoadFunction<blake3_delete_del>("blake3_delete", out blake3_delete);

			lib_loader.LoadFunction<blake3_reset_del>("blake3_reset", out blake3_reset);

			lib_loader.LoadFunction<blake3_update_del>("blake3_update", out blake3_update);
			lib_loader.LoadFunction<blake3_update_preemptive_del>("blake3_update", out blake3_update_preemptive);

			lib_loader.LoadFunction<blake3_update_rayon_del>("blake3_update_rayon", out blake3_update_rayon);

			lib_loader.LoadFunction<blake3_finalize_del>("blake3_finalize", out blake3_finalize);

			lib_loader.LoadFunction<blake3_finalize_xof_del>("blake3_finalize_xof", out blake3_finalize_xof);
			lib_loader.LoadFunction<blake3_finalize_seek_xof_del>("blake3_finalize_seek_xof", out blake3_finalize_seek_xof);
		}

		private void* _hasher;
	    /// <summary>
	    /// We are taking a limit of 1024 bytes to switch to a preemptive version,
	    /// as it takes around 1μs on a x64 very recent CPU to complete, which is
	    /// better aligned with the documentation of <see cref="SuppressGCTransitionAttribute"/>:
	    /// `Native function always executes for a trivial amount of time (less than 1 microsecond).`
	    /// </summary>
	    private const int LimitPreemptive = 1024;

	    private Hasher(void* hasher)
	    {
	        _hasher = hasher;
		}

	    /// <summary>
	    /// The default hash function.
	    /// </summary>
	    /// <param name="input">The input data to hash.</param>
	    /// <param name="output">The output hash.</param>
	    /// <remarks>
	    /// For an incremental version that accepts multiple writes <see cref="Update{T}"/>
	    /// This function is always single-threaded. For multi-threading support <see cref="UpdateWithJoin"/> 
	    /// </remarks>
	    public static void Hash(ReadOnlySpan<byte> input, Span<byte> output)
	    {
	        if (output.Length == 32)
	        {
	            fixed (void* ptrOut = output)
	            fixed (void* ptr = input)
	            {
	                var size = input.Length;
	                if (size <= LimitPreemptive)
	                {
	                    blake3_hash(ptr, (void*) size, ptrOut);
	                }
	                else
	                {
	                    blake3_hash_preemptive(ptr, (void*) size, ptrOut);
	                }
	            }
	        }
	        else
	        {
				using (var hasher = New())
				{
					hasher.Update(input);
					hasher.Finalize(output);
				}
	        }
	    }

	    /// <summary>
	    /// Dispose this instance.
	    /// </summary>
	    public void Dispose()
	    {
	        if (_hasher != null) blake3_delete(_hasher);
	        _hasher = null;
	    }

	    /// <summary>
	    /// Reset the Hasher to its initial state.
	    /// </summary>
	    /// <remarks>
	    /// This is functionally the same as overwriting the Hasher with a new one, using the same key or context string if any.
	    /// However, depending on how much inlining the optimizer does, moving a Hasher might copy its entire CV stack, most of which is useless uninitialized bytes.
	    /// This methods avoids that copy.
	    /// </remarks>
	    public void Reset()
	    {
	        if (_hasher == null) ThrowNullReferenceException();
	        blake3_reset(_hasher);
	    }

	    /// <summary>
	    /// Add input bytes to the hash state. You can call this any number of times.
	    /// </summary>
	    /// <param name="data">The input data byte buffer to hash.</param>
	    /// <remarks>
	    /// This method is always single-threaded. For multi-threading support, see <see cref="UpdateWithJoin"/> below.
	    ///
	    /// Note that the degree of SIMD parallelism that update can use is limited by the size of this input buffer.
	    /// The 8 KiB buffer currently used by std::io::copy is enough to leverage AVX2, for example, but not enough to leverage AVX-512.
	    /// A 16 KiB buffer is large enough to leverage all currently supported SIMD instruction sets.
	    /// </remarks>
	    public void Update(ReadOnlySpan<byte> data)
	    {
	        if (_hasher == null) ThrowNullReferenceException();
	        fixed (void* ptr = data)
	        {
	            FastUpdate(_hasher, ptr, data.Length);
	        }
	    }

	    /// <summary>
	    /// Add input data to the hash state. You can call this any number of times.
	    /// </summary>
	    /// <typeparam name="T">Type of the data</typeparam>
	    /// <param name="data">The data span to hash.</param>
	    /// <remarks>
	    /// This method is always single-threaded. For multi-threading support, see <see cref="UpdateWithJoin"/> below.
	    ///
	    /// Note that the degree of SIMD parallelism that update can use is limited by the size of this input buffer.
	    /// The 8 KiB buffer currently used by std::io::copy is enough to leverage AVX2, for example, but not enough to leverage AVX-512.
	    /// A 16 KiB buffer is large enough to leverage all currently supported SIMD instruction sets.
	    /// </remarks>
	    public void Update<T>(ReadOnlySpan<T> data) where T : unmanaged
	    {
	        if (_hasher == null) ThrowNullReferenceException();
	        fixed (void* ptr = data)
	        {
	            FastUpdate(_hasher, ptr, data.Length * sizeof(T));
	        }
	    }

	    /// <summary>
	    /// Add input bytes to the hash state, as with update, but potentially using multi-threading.
	    /// </summary>
	    /// <param name="data">The input byte buffer.</param>
	    /// <remarks>
	    /// To get any performance benefit from multi-threading, the input buffer size needs to be very large.
	    /// As a rule of thumb on x86_64, there is no benefit to multi-threading inputs less than 128 KiB.
	    /// Other platforms have different thresholds, and in general you need to benchmark your specific use case.
	    /// Where possible, memory mapping an entire input file is recommended, to take maximum advantage of multi-threading without needing to tune a specific buffer size.
	    /// Where memory mapping is not possible, good multi-threading performance requires doing IO on a background thread, to avoid sleeping all your worker threads while the input buffer is (serially) refilled.
	    /// This is quite complicated compared to memory mapping.
	    /// </remarks>
	    public void UpdateWithJoin(ReadOnlySpan<byte> data)
	    {
	        if (data == null) ThrowArgumentNullException();
	        if (_hasher == null) ThrowNullReferenceException();
	        fixed (void* ptr = data)
	        {
	            blake3_update_rayon(_hasher, ptr, (void*)data.Length);
	        }
	    }

	    /// <summary>
	    /// Add input data span to the hash state, as with update, but potentially using multi-threading.
	    /// </summary>
	    /// <param name="data">The input data buffer.</param>
	    /// <remarks>
	    /// To get any performance benefit from multi-threading, the input buffer size needs to be very large.
	    /// As a rule of thumb on x86_64, there is no benefit to multi-threading inputs less than 128 KiB.
	    /// Other platforms have different thresholds, and in general you need to benchmark your specific use case.
	    /// Where possible, memory mapping an entire input file is recommended, to take maximum advantage of multi-threading without needing to tune a specific buffer size.
	    /// Where memory mapping is not possible, good multi-threading performance requires doing IO on a background thread, to avoid sleeping all your worker threads while the input buffer is (serially) refilled.
	    /// This is quite complicated compared to memory mapping.
	    /// </remarks>
	    public void UpdateWithJoin<T>(ReadOnlySpan<T> data) where T : unmanaged
	    {
	        if (_hasher == null) ThrowNullReferenceException();
	        fixed (void* ptr = data)
	        {
	            void* size = (void*) (IntPtr) (data.Length * sizeof(T));
	            blake3_update_rayon(_hasher, ptr, size);
	        }
	    }

	    /// <summary>
	    /// Finalize the hash state to the output span, which can supply any number of output bytes.
	    /// </summary>
	    /// <param name="hash">The output hash, which can supply any number of output bytes.</param>
	    /// <remarks>
	    /// This method is idempotent. Calling it twice will give the same result. You can also add more input and finalize again.
	    /// </remarks>
	    public void Finalize(Span<byte> hash)
	    {
	        if (_hasher == null) ThrowNullReferenceException();
	        ref var pData = ref MemoryMarshal.GetReference(hash);
	        fixed (void* ptr = &pData)
	        {
	            var size = hash.Length;
	            if (size == 32)
	            {
	                blake3_finalize(_hasher, ptr);
	            }
	            else
	            {
	                blake3_finalize_xof(_hasher, ptr, (void*)(IntPtr)hash.Length);
	            }
	        }
	    }

	    /// <summary>
	    /// Finalize the hash state to the output span, which can supply any number of output bytes.
	    /// </summary>
	    /// <param name="offset">The offset to seek to in the output stream, relative to the start.</param>
	    /// <param name="hash">The output hash, which can supply any number of output bytes.</param>
	    /// <remarks>
	    /// This method is idempotent. Calling it twice will give the same result. You can also add more input and finalize again.
	    /// </remarks>
	    public void Finalize(ulong offset, Span<byte> hash)
	    {
	        if (_hasher == null) ThrowNullReferenceException();
	        ref var pData = ref MemoryMarshal.GetReference(hash);
	        fixed (void* ptr = &pData)
	        {
	            blake3_finalize_seek_xof(_hasher, offset, ptr, (void*)(IntPtr)hash.Length);
	        }
	    }

	    /// <summary>
	    /// Finalize the hash state to the output span, which can supply any number of output bytes.
	    /// </summary>
	    /// <param name="offset">The offset to seek to in the output stream, relative to the start.</param>
	    /// <param name="hash">The output hash, which can supply any number of output bytes.</param>
	    /// <remarks>
	    /// This method is idempotent. Calling it twice will give the same result. You can also add more input and finalize again.
	    /// </remarks>
	    public void Finalize(long offset, Span<byte> hash)
	    {
	        Finalize((ulong)offset, hash);
	    }

	    /// <summary>
	    /// Construct a new Hasher for the regular hash function.
	    /// </summary>
	    /// <returns>A new instance of the hasher</returns>
	    /// <remarks>
	    /// The struct returned needs to be disposed explicitly.
	    /// </remarks>
	    public static Hasher New()
	    {
	        return new Hasher(blake3_new());
	    }

	    /// <summary>
	    /// Construct a new Hasher for the keyed hash function.
	    /// </summary>
	    /// <param name="key">A 32 byte key.</param>
	    /// <returns>A new instance of the hasher</returns>
	    /// <remarks>
	    /// The struct returned needs to be disposed explicitly.
	    /// </remarks>
	    public static Hasher NewKeyed(ReadOnlySpan<byte> key)
	    {
	        if (key.Length != 32) throw new ArgumentOutOfRangeException(nameof(key), "Expecting the key to be 32 bytes");

	        fixed(void* ptr = key)
	            return new Hasher(blake3_new_keyed(ptr));
	    }

	    /// <summary>
	    /// Construct a new Hasher for the key derivation function.
	    /// </summary>
	    /// <returns>A new instance of the hasher</returns>
	    /// <remarks>
	    /// The struct returned needs to be disposed explicitly.
	    /// </remarks>
	    public static Hasher NewDeriveKey(string text)
	    {
	        return NewDeriveKey(Encoding.UTF8.GetBytes(text));
	    }

	    /// <summary>
	    /// Construct a new Hasher for the key derivation function.
	    /// </summary>
	    /// <returns>A new instance of the hasher</returns>
	    /// <remarks>
	    /// The struct returned needs to be disposed explicitly.
	    /// </remarks>
	    public static Hasher NewDeriveKey(ReadOnlySpan<byte> str)
	    {
	        fixed(void* ptr = str)
	            return new Hasher(blake3_new_derive_key(ptr, (void*)str.Length));
	    }

	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
	    private static void FastUpdate(void* hasher, void* ptr, long size)
	    {
	        if (size <= LimitPreemptive)
	        {
	            blake3_update(hasher, ptr, (void*)size);
	        }
	        else
	        {
	            blake3_update_preemptive(hasher, ptr, (void*)size);
	        }
	    }

	    [MethodImpl(MethodImplOptions.NoInlining)]
	    private static void ThrowNullReferenceException()
	    {
	        throw new NullReferenceException("The Hasher is not initialized or already destroyed.");
	    }

	    [MethodImpl(MethodImplOptions.NoInlining)]
	    private static void ThrowArgumentNullException()
	    {
	        // ReSharper disable once NotResolvedInText
	        throw new ArgumentNullException("data");
	    }

	    [MethodImpl(MethodImplOptions.NoInlining)]
	    private static void ThrowArgumentOutOfRange(int size)
	    {
	        // ReSharper disable once NotResolvedInText
	        throw new ArgumentOutOfRangeException("output", $"Invalid size {size} of the output buffer. Expecting >= 32");
	    }
	}
}