﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSource.Data.HashFunction
{
    /// <summary>
    /// Common interface to non-cryptographic hash functions.
    /// </summary>
    public interface IHashFunction
    {
        /// <summary>
        /// Size of produced hash, in bits.
        /// </summary>
        /// <value>
        /// The size of the hash, in bits.
        /// </value>
        int HashSizeInBits { get; }


        /// <summary>
        /// Computes hash value for given byte array.
        /// </summary>
        /// <param name="data">Array of data to hash.</param>
        /// <returns>
        /// Hash value of the data.
        /// </returns>
        /// <exception cref="ArgumentNullException">;<paramref name="data"/></exception>
        IHashValue ComputeHash(byte[] data);

        /// <summary>
        /// Computes hash value for given byte array.
        /// </summary>
        /// <param name="data">Array of data to hash.</param>
        /// <param name="cancellationToken">A cancellation token to observe while calculating the hash value.</param>
        /// <returns>
        /// Hash value of the data.
        /// </returns>
        /// <exception cref="ArgumentNullException">;<paramref name="data"/></exception>
        /// <exception cref="TaskCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
        IHashValue ComputeHash(byte[] data, CancellationToken cancellationToken);


        /// <summary>
        /// Computes hash value for given stream.
        /// </summary>
        /// <param name="data">Stream of data to hash.</param>
        /// <returns>
        /// Hash value of the data.
        /// </returns>
        /// <exception cref="ArgumentNullException">;<paramref name="data"/></exception>
        /// <exception cref="ArgumentException">Stream must be readable.;<paramref name="data"/></exception>
        /// <exception cref="ArgumentException">Stream must be seekable for this type of hash function.;<paramref name="data"/></exception>
        IHashValue ComputeHash(Stream data);

        /// <summary>
        /// Computes hash value for given stream.
        /// </summary>
        /// <param name="data">Stream of data to hash.</param>
        /// <param name="cancellationToken">A cancellation token to observe while calculating the hash value.</param>
        /// <returns>
        /// Hash value of the data.
        /// </returns>
        /// <exception cref="ArgumentNullException">;<paramref name="data"/></exception>
        /// <exception cref="ArgumentException">Stream must be readable.;<paramref name="data"/></exception>
        /// <exception cref="ArgumentException">Stream must be seekable for this type of hash function.;<paramref name="data"/></exception>
        /// <exception cref="TaskCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
        IHashValue ComputeHash(Stream data, CancellationToken cancellationToken);
    }
}
