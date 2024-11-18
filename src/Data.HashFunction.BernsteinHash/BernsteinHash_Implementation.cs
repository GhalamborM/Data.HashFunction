using Data.HashFunction.BernsteinHash;
using Data.HashFunction.Core;
using Data.HashFunction.Core.Utilities;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Data.HashFunction.BernsteinHash
{
    internal class BernsteinHash_Implementation
        : StreamableHashFunctionBase,
            IBernsteinHash
    {
        public override int HashSizeInBits { get; } = 32;


        public override IBlockTransformer CreateBlockTransformer() =>
            new BlockTransformer();


        private class BlockTransformer
            : BlockTransformerBase<BlockTransformer>
        {
            private UInt32 _hashValue;


            protected override void CopyStateTo(BlockTransformer other)
            {
                base.CopyStateTo(other);

                other._hashValue = _hashValue;
            }

            protected override void TransformByteGroupsInternal(ArraySegment<byte> data)
            {
                var dataArray = data.Array;
                var endOffset = data.Offset + data.Count;

                var tempHashValue = _hashValue;

                for (var currentOffset = data.Offset; currentOffset < endOffset; ++currentOffset)
                    tempHashValue = (33 * tempHashValue) + dataArray[currentOffset];

                _hashValue = tempHashValue;
            }

            protected override IHashValue FinalizeHashValueInternal(CancellationToken cancellationToken)
            {
                return new HashValue(
                    BitConverter.GetBytes(_hashValue),
                    32);
            }
        }
    }
}

