using System;
using System.Collections.Generic;
using System.Text;

namespace OpenSource.Data.HashFunction.Blake3
{
    /// <summary>
    /// Defines a configuration for a <see cref="IBlake3"/> implementation.
    /// </summary>
    public interface IBlake3Config
    {
		/// <summary>
		/// Gets the desired hash size, in bits.
		/// </summary>
		/// <value>
		/// The desired hash size, in bits.
		/// </value>
		/// <remarks>
		/// Defaults to <c>32</c>.
		/// </remarks>
		int HashSizeInBits { get; }


        /// <summary>
        /// Makes a deep clone of current instance.
        /// </summary>
        /// <returns>A deep clone of the current instance.</returns>
        IBlake3Config Clone();
    }
}
