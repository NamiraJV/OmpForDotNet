using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using OmpForDotNet.Utility.CodeAnalysis;
using OmpForDotNet.Utility.Entities;
using System.Collections.Generic;
using System.Linq;

namespace OmpForDotNet.Test
{
    [TestFixture]
    public class CodeAnalyzerTest
    {
        /* TODO: tests to add
         * 1) root parameter is null
         * 2) no region/endregion directives in code
         * 3) directives: region n1 endregion n1 region n2 region n3 endregion n3 endregion n2 region n4 region n5 endregion n4 endregion n5
         * 4) no nodes between directives/some nodes
        */

        [Test]
        [TestCaseSource(nameof(GetRegionDirectivesTestData))]
        public void GetRegionDirectivesTest(string testCode, List<DirectiveSyntaxNode> expectedNodes)
        {
            CodeAnalyzer analyzer = new CodeAnalyzer(new Utility.Factories.DirectiveParserFactory());
            SyntaxNode root = CSharpSyntaxTree.ParseText(testCode)
                .GetRoot();

            List<DirectiveSyntaxNode> nodes = analyzer.GetRegionNodes(root);

            AreEqualNodes(expectedNodes, nodes);
        }

        private static TestCaseData[] GetRegionDirectivesTestData = 
        {
            new TestCaseData()
        };

        private void AreEqualNodes(List<DirectiveSyntaxNode> expectedNodes, List<DirectiveSyntaxNode> actualNodes)
        {
            if (expectedNodes == null)
            {
                Assert.IsNull(actualNodes);
                return;
            }

            Assert.AreEqual(expectedNodes.Count, actualNodes.Count);
            
            for (int i = 0, length = expectedNodes.Count; i < length; i++)
            {
                Assert.AreEqual(expectedNodes[i], actualNodes[i]);
            }
        }

        [Test]
        public void GetAllRegionDirectivesTest()
        {
            CodeAnalyzer analyzer = new CodeAnalyzer(new Utility.Factories.DirectiveParserFactory());
            SyntaxNode root = CSharpSyntaxTree.ParseText(_testCode)
                .GetRoot();

            List<DirectiveSyntaxNode> nodes = analyzer.GetRegionNodes(root);

            Assert.AreEqual(2, nodes.Count);
        }

        [Test]
        public void FilterOmpDirectives()
        {
            CodeAnalyzer analyzer = new CodeAnalyzer(new Utility.Factories.DirectiveParserFactory());
            SyntaxNode root = CSharpSyntaxTree.ParseText(_testCode)
                .GetRoot();
            List<DirectiveSyntaxNode> nodes = analyzer.GetRegionNodes(root);

            List<DirectiveSyntaxNode> ompNodes = analyzer.FilterOmpNodes(nodes);

            Assert.AreEqual(1, ompNodes.Count);
            Assert.AreEqual(DirectiveType.OMP_PARALLEL_FOR, ompNodes[0].DirectiveInfo.Type);
        }

        private string _testCode = @"
namespace N
{
    #region n1
    class C
    {
        void method()
        {
            int n = 10;
            int[] arr = new int[n];
            #region n2 omp parallel for
            for (int i = 0; i < n; i++)
            { 
                arr[i] = i;
            }
            #endregion n2
        }
    }
    #endregion n1
}";

        private static string[] TestCodeStrings = new[] 
        {
            @"
namespace N
{
    class C
    {
        void method()
        {
            int n = 10;
            int[] arr = new int[n];
            for (int i = 0; i < n; i++)
            { 
                arr[i] = i;
            }
        }
    }
}"
        };
    }
}
