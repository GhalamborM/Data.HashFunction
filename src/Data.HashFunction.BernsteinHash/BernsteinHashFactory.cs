using System;
using System.Collections.Generic;
using Data.HashFunction.BernsteinHash;
using System.Text;

namespace Data.HashFunction.BernsteinHash
{
    /// <summary>
    /// Provides instances of implementations of <see cref="IBernsteinHash"/>.
    /// </summary>
    public sealed class BernsteinHashFactory
        : IBernsteinHashFactory
    {
        /// <summary>
        /// Gets the singleton instance of this factory.
        /// </summary>
        public static IBernsteinHashFactory Instance { get; } = new BernsteinHashFactory();


        private BernsteinHashFactory()
        {

        }

        /// <summary>
        /// Creates a new <see cref="IBernsteinHash" /> instance with the default configuration.
        /// </summary>
        /// <returns>
        /// A <see cref="IBernsteinHash" /> instance.
        /// </returns>
        public IBernsteinHash Create()
        {
            return new BernsteinHash_Implementation();
        }
    }
}

