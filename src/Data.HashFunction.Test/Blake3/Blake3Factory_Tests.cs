using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Xunit;
using Data.HashFunction.Blake3;

namespace Data.HashFunction.Test.Blake3
{
    public class Blake3Factory_Tests
    {
        [Fact]
        public void Blake3Factory_Instance_IsDefined()
        {
            Assert.NotNull(Blake3Factory.Instance);
            Assert.IsType<Blake3Factory>(Blake3Factory.Instance);
        }
        
        [Fact]
        public void blake3Factory_Create_Config_IsNull_Throws()
        {
            var blake3Factory = Blake3Factory.Instance;

            Assert.Equal(
                "config",
                Assert.Throws<ArgumentNullException>(
                        () => blake3Factory.Create(null))
                    .ParamName);
        }

        [Fact]
        public void blake3Factory_Create_Config_Works()
        {
			var blake3Config = new Blake3Config();

			var blake3Factory = Blake3Factory.Instance;
			var blake3 = blake3Factory.Create(blake3Config);

            Assert.NotNull(blake3);
            Assert.IsType<Blake3_Implementation>(blake3);

            var resultingBlake3Config = blake3.Config;

            Assert.Equal(resultingBlake3Config.HashSizeInBits, resultingBlake3Config.HashSizeInBits);
            Assert.Equal(resultingBlake3Config, resultingBlake3Config);
		}
    }
}

