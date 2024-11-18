using System;
using System.Collections.Generic;
using System.Text;

namespace Data.HashFunction.FarmHash
{
    /// <summary>
    /// Provides instances of implementations of <see cref="IFarmHashFingerprint64"/>.
    /// </summary>
    public sealed class FarmHashFingerprint64Factory
        : IFarmHashFingerprint64Factory
    {
        /// <summary>
        /// Gets the singleton instance of this factory.
        /// </summary>
        public static IFarmHashFingerprint64Factory Instance { get; } = new FarmHashFingerprint64Factory();


        private FarmHashFingerprint64Factory()
        {

        }


        /// <summary>
        /// Creates a new <see cref="IFarmHashFingerprint64"/> instance.
        /// </summary>
        /// <returns>A <see cref="IFarmHashFingerprint64"/> instance.</returns>
        public IFarmHashFingerprint64 Create() =>
            new FarmHashFingerprint64_Implementation();
    }
}

