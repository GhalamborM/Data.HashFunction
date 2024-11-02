using System;
using System.Collections.Generic;
using System.Text;

namespace OpenSource.Data.HashFunction.Blake3
{
	/// <summary>
	/// Implementation of BLAKE3 as specified at https://en.wikipedia.org/wiki/BLAKE_(hash_function)#BLAKE3.
	/// </summary>
	public interface IBlake3
        : IHashFunction
	{
        /// <summary>
        /// Configuration used when creating this instance.
        /// </summary>
        /// <value>
        /// A clone of configuration that was used when creating this instance.
        /// </value>
        IBlake3Config Config { get; }
    }
}
