using System;
using System.Collections.Generic;
using System.Text;

namespace Data.HashFunction.SpookyHash
{
    /// <summary>
    /// Implements SpookyHash V2 as specified at http://burtleburtle.net/bob/hash/spooky.html.
    /// </summary>
    public interface ISpookyHashV2
        : IStreamableHashFunction
    {

        /// <summary>
        /// Configuration used when creating this instance.
        /// </summary>
        /// <value>
        /// A clone of configuration that was used when creating this instance.
        /// </value>
        ISpookyHashConfig Config { get; }

    }
}

