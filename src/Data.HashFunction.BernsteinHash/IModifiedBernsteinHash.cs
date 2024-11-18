using System;
using System.Collections.Generic;
using System.Text;

namespace Data.HashFunction.BernsteinHash
{
    /// <summary>
    /// Implementation of Modified Bernstein hash as specified at http://www.eternallyconfuzzled.com/tuts/algorithms/jsw_tut_hashing.aspx#djb2.
    /// 
    /// "
    /// [The Modified Bernstein hash is a] minor update to <see cref="IBernsteinHash">Bernstein's hash</see> which replaces addition with 
    ///   XOR for the combining step. 
    /// This change does not appear to be well known or often used, the original algorithm is still 
    ///   recommended by nearly everyone, but the new algorithm typically results in a better distribution.
    ///   
    /// Dan Bernstein created [the Bernstein hash] algorithm and posted it in a newsgroup. 
    ///   It is known by many as the Chris Torek hash because Chris went a long way toward popularizing it. 
    ///   Since then it has been used successfully by many, but despite that the algorithm itself is not very sound 
    ///     when it comes to avalanche and permutation of the internal state. 
    ///   It has proven very good for small character keys, where it can outperform algorithms that result 
    ///     in a more random distribution.    
    ///     
    ///   Bernstein's hash should be used with caution. 
    ///   It performs very well in practice, for no apparently known reasons 
    ///     (much like how the constant 33 does better than more logical constants for no apparent reason), 
    ///     but in theory it is not up to snuff. 
    ///   Always test this function with sample data for every application to ensure that it does not encounter 
    ///     a degenerate case and cause excessive collisions.
    /// "
    /// </summary>
    public interface IModifiedBernsteinHash
        : IStreamableHashFunction
    {

    }
}

