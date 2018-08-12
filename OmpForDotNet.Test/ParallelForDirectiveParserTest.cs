using NUnit.Framework;
using OmpForDotNet.Utility.Entities;
using OmpForDotNet.Utility.Parsers;

namespace OmpForDotNet.Test
{
    [TestFixture]
    public class ParallelForDirectiveParserTest
    {
        [Test]
        [TestCaseSource(nameof(TestData))]
        public void Test(string directive, OmpDirectiveInfo expectedResult)
        {
            ParallelForDirectiveParser parser = new ParallelForDirectiveParser();

            OmpDirectiveInfo result = parser.Parse(directive);

            Assert.AreEqual(expectedResult.Options?.Count, result.Options?.Count);
        }

        [Test]
        public void ParseNumThreadsTest()
        {
            string directive = "#region parallel for num_threads(5)";

            ParallelForDirectiveParser parser = new ParallelForDirectiveParser();

            OmpDirectiveInfo result = parser.Parse(directive);
            Assert.IsNotNull(result.Options);
            Assert.IsNotNull(result.Options["num_threads"]);
            Assert.AreEqual(5, int.Parse(result.Options["num_threads"][0]));
        }

        [Test]
        public void ParseThreadPrivateTest()
        {
            string directive = "#region parallel for threadprivate(a,b,c)";

            ParallelForDirectiveParser parser = new ParallelForDirectiveParser();

            OmpDirectiveInfo result = parser.Parse(directive);
            Assert.IsNotNull(result.Options);
            Assert.IsNotNull(result.Options["threadprivate"]);
            Assert.AreEqual("a", result.Options["threadprivate"][0]);
            Assert.AreEqual("b", result.Options["threadprivate"][1]);
            Assert.AreEqual("c", result.Options["threadprivate"][2]);
        }

        private static TestCaseData[] TestData = 
        {
            new TestCaseData("#region parallel for", new OmpDirectiveInfo(DirectiveType.OMP_PARALLEL_FOR, null))
        };
    }
}
