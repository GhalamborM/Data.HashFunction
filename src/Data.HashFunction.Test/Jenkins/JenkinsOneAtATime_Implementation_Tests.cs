using System;
using System.Collections.Generic;
using Data.HashFunction.Jenkins;
using Data.HashFunction.Test._Utilities;
using System.Text;

namespace Data.HashFunction.Test.Jenkins
{
    public class JenkinsOneAtATime_Implementation_Tests
    {

        public class IStreamableHashFunction_Tests
            : IStreamableHashFunction_TestBase<IJenkinsOneAtATime>
        {
            protected override IEnumerable<KnownValue> KnownValues { get; } =
                new KnownValue[] {
                    new KnownValue(32, TestConstants.FooBar, 0xf952fde7),
                };

            protected override IJenkinsOneAtATime CreateHashFunction(int hashSize) =>
                new JenkinsOneAtATime_Implementation();
        }


    }
}

