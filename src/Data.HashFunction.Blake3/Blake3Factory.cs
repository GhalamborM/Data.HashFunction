using System;
using System.Collections.Generic;
using System.Text;

namespace Data.HashFunction.Blake3
{
    /// <summary>
    /// Provides instances of implementations of <see cref="IBlake3"/>.
    /// </summary>
    public sealed class Blake3Factory
        : IBlake3Factory
    {
        /// <summary>
        /// Gets the singleton instance of this factory.
        /// </summary>
        public static IBlake3Factory Instance { get; } = new Blake3Factory();


        private Blake3Factory()
        {

        }

        /// <summary>
        /// Creates a new <see cref="IBlake3" /> instance with the default configuration.
        /// </summary>
        /// <returns>
        /// A <see cref="IBlake3" /> instance.
        /// </returns>
        public IBlake3 Create()
        {
            return Create(new Blake3Config());
        }

        /// <summary>
        /// Creates a new <see cref="IBlake3" /> instance with given configuration.
        /// </summary>
        /// <param name="config">The configuration to use.</param>
        /// <returns>
        /// A <see cref="IBlake3" /> instance.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="config"/></exception>
        public IBlake3 Create(IBlake3Config config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            return new Blake3_Implementation(config);
        }
    }
}

