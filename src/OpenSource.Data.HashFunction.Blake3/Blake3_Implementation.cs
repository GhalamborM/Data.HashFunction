using OpenSource.Data.HashFunction.Core;
using OpenSource.Data.HashFunction.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;

namespace OpenSource.Data.HashFunction.Blake3
{
	internal partial class Blake3_Implementation
		: HashFunctionBase,
			IBlake3
	{
		private readonly IBlake3Config _config;
		public IBlake3Config Config => _config.Clone();

		public const int MinHashSizeInBits = 8;
		public const int MaxHashSizeInBits = 4096;

		public Blake3_Implementation(IBlake3Config config)
		{
			if (config == null)
				throw new ArgumentNullException(nameof(config));

			_config = config.Clone();

			if (_config.HashSizeInBits < MinHashSizeInBits || _config.HashSizeInBits > MaxHashSizeInBits)
				throw new ArgumentOutOfRangeException($"{nameof(config)}.{nameof(config.HashSizeInBits)}", _config.HashSizeInBits, $"Expected: {MinHashSizeInBits} >= {nameof(config)}.{nameof(config.HashSizeInBits)} <= {MaxHashSizeInBits}");

			if (_config.HashSizeInBits % 8 != 0)
				throw new ArgumentOutOfRangeException($"{nameof(config)}.{nameof(config.HashSizeInBits)}", _config.HashSizeInBits, $"{nameof(config)}.{nameof(config.HashSizeInBits)} must be a multiple of 8.");
		}

		public override int HashSizeInBits => _config.HashSizeInBits;

		protected override IHashValue ComputeHashInternal(ArraySegment<byte> data, CancellationToken cancellationToken)
		{
			byte[] outputBuffer = new byte[Config.HashSizeInBits];

			Span<byte> output = new Span<byte>(outputBuffer);
			Hasher.Hash(data.AsSpan(), output);

			return new HashValue(outputBuffer, Config.HashSizeInBits);
		}
	}
}