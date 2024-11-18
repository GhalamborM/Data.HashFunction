using System;
using System.Collections.Generic;
using System.Text;

namespace Data.HashFunction.MurmurHash
{
    /// <summary>
    /// Defines a configuration for a <see cref="IMurmurHash1"/> implementation.
    /// </summary>
    public interface IMurmurHash1Config
    {
        /// <summary>
        /// Gets the seed.
        /// </summary>
        /// <value>
        /// The seed.
        /// </value>
        UInt32 Seed { get; }



        /// <summary>
        /// Makes a deep clone of current instance.
        /// </summary>
        /// <returns>A deep clone of the current instance.</returns>
        IMurmurHash1Config Clone();
    }
}

