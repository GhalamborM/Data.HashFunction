using System.Collections.Generic;
using System.Linq;

namespace OpenSource.Data.HashFunction.Blake3
{
    /// <summary>
    /// Defines a configuration for a Blake2B hash function implementation.
    /// </summary>
    public class Blake3Config
        : IBlake3Config
    {
        /// <summary>
        /// The default hash size, in bits.
        /// </summary>
        public const int DefaultHashSizeInBits = 32;

        /// <summary>
        /// Gets the desired hash size, in bits.
        /// </summary>
        /// <value>
        /// The desired hash size, in bits.
        /// </value>
        /// <remarks>
        /// Defaults to <c>32</c>.
        /// </remarks>
        public int HashSizeInBits { get; set; } = DefaultHashSizeInBits;

        /// <summary>
        /// Makes a deep clone of current instance.
        /// </summary>
        /// <returns>A deep clone of the current instance.</returns>
        public IBlake3Config Clone() =>
            new Blake3Config() {
                HashSizeInBits = HashSizeInBits
            };
    }
}