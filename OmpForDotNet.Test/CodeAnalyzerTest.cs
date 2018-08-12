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
    }
}
