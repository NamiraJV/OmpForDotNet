using NUnit.Framework;
using OmpForDotNet.Utility.Entities;
using OmpForDotNet.Utility.Factories;
using OmpForDotNet.Utility.Parsers;
using System;

namespace OmpForDotNet.Test
{
    [TestFixture]
    public class DirectiveParserFactoryTest
    {
        [Test]
        [TestCaseSource(nameof(TestData))]
        public void Test(DirectiveType type, Type expectedType)
        {
            DirectiveParserFactory factory = new DirectiveParserFactory();

            DirectiveParser parser = factory.GetParser(type);

            Assert.AreEqual(expectedType, parser?.GetType());
        }

        private static TestCaseData[] TestData =
        {
            new TestCaseData(DirectiveType.OMP_PARALLEL_FOR, typeof(ParallelForDirectiveParser)),
            new TestCaseData(DirectiveType.OMP_PARALLEL, typeof(ParallelDirectiveParser)),
            new TestCaseData(DirectiveType.UNKNOWN, null)
        };
    }
}
