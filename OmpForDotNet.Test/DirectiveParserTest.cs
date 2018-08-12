using NUnit.Framework;
using OmpForDotNet.Utility.Entities;
using OmpForDotNet.Utility.Parsers;

namespace OmpForDotNet.Test
{
    [TestFixture]
    public class DirectiveParserTest
    {
        [Test]
        [TestCaseSource(nameof(TestData))]
        public void Test(string testDirective, DirectiveType expectedType)
        {                                                                            
            DirectiveType directiveType = DirectiveParser.GetDirectiveType(testDirective);

            Assert.AreEqual(expectedType, directiveType);
        }

        private static TestCaseData[] TestData =
        {
            new TestCaseData("#region hidden implementation", DirectiveType.UNKNOWN),
            new TestCaseData("#region omp parallel", DirectiveType.OMP_PARALLEL),
            new TestCaseData("#region omp parallel for", DirectiveType.OMP_PARALLEL_FOR)
        };
    }
}
