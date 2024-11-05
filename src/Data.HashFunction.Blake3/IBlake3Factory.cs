using System;
using System.Collections.Generic;
using System.Text;

namespace Data.HashFunction.Blake3
{
    /// <summary>
    /// Provides instances of implementations of <see cref="IBlake3"/>.
    /// </summary>
    public interface IBlake3Factory
    {
        /// <summary>
        /// Creates a new <see cref="IBlake3"/> instance with the default configuration.
        /// </summary>
        /// <returns>A <see cref="IBlake3"/> instance.</returns>
        IBlake3 Create();


		/// <summary>
		/// Creates a new <see cref="IBlake3"/> instance with given configuration.
		/// </summary>
		/// <param name="config">The configuration to use.</param>
		/// <returns>A <see cref="IBlake3"/> instance.</returns>
		IBlake3 Create(IBlake3Config config);

    }
}

