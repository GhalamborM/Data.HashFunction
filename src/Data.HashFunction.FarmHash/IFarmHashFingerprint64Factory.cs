using System;
using System.Collections.Generic;
using System.Text;

namespace Data.HashFunction.FarmHash
{
    /// <summary>
    /// Provides instances of implementations of <see cref="IFarmHashFingerprint64"/>.
    /// </summary>
    public interface IFarmHashFingerprint64Factory
    {
        /// <summary>
        /// Creates a new <see cref="IFarmHashFingerprint64"/> instance.
        /// </summary>
        /// <returns>A <see cref="IFarmHashFingerprint64"/> instance.</returns>
        IFarmHashFingerprint64 Create();
    }
}

