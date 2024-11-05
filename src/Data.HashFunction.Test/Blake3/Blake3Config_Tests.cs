using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Xunit;
using Data.HashFunction.Blake3;

namespace Data.HashFunction.Test.Blake3
{
    public class Blake3Config_Tests
	{
        [Fact]
        public void Blake3Config_Defaults_HaventChanged()
        {
            var blake3Config = new Blake3Config();

			Assert.Equal(Blake3Config.DefaultHashSizeInBits, blake3Config.HashSizeInBits);
		}

        [Fact]
        public void Blake3Config_Clone_Works()
        {
            var blake3Config = new Blake3Config() {
                HashSizeInBits = 64,
            };

            var blake3ConfigClone = blake3Config.Clone();

            Assert.IsType<Blake3Config>(blake3ConfigClone);

            Assert.Equal(blake3Config.HashSizeInBits, blake3ConfigClone.HashSizeInBits);
        }
    }
}

