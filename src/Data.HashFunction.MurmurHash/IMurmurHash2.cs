using System;
using System.Collections.Generic;
using System.Text;

namespace Data.HashFunction.MurmurHash
{
    /// <summary>
    /// Implementation of MurmurHash2 as specified at https://github.com/aappleby/smhasher/blob/master/src/MurmurHash2.cpp 
    ///   and https://github.com/aappleby/smhasher/wiki/MurmurHash2.
    /// 
    /// This hash function has been superseded by <see cref="IMurmurHash3">MurmurHash3</see>.
    /// </summary>
    public interface IMurmurHash2
        : IHashFunction
    {

        /// <summary>
        /// Configuration used when creating this instance.
        /// </summary>
        /// <value>
        /// A clone of configuration that was used when creating this instance.
        /// </value>
        IMurmurHash2Config Config { get; }

    }
}

