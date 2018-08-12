using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using OmpForDotNet.Utility.Entities;
using OmpForDotNet.Utility.Factories;
using OmpForDotNet.Utility.Parsers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OmpForDotNet.Utility.CodeAnalysis
{
    /// <summary>
    /// Allows to process document syntax tree
    /// </summary>
    public class CodeAnalyzer
    {
        /// <summary>
        /// Factory that is responsble for directive parameters parser creation
        /// </summary>
        private DirectiveParserFactory _factory;

        /// <summary>
        /// Analyzer constructor
        /// </summary>
        /// <param name="factory">Directive parser factory</param>
        public CodeAnalyzer(DirectiveParserFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Allows to load solution by file path
        /// </summary>
        /// <param name="solutionPath">Path to *.sln file in a file system</param>
        /// <returns>Solution</returns>
        public async Task<Solution> GetSolutionByPath(string solutionPath)
        {
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            Solution solution = await workspace.OpenSolutionAsync(solutionPath);

            return solution;
        }

        /// <summary>
        /// Allows to get parts of code surrounded by #region directives
        /// </summary>
        /// <param name="root">Root of the document syntax tree</param>
        /// <returns>List of processed nodes</returns>
        public List<DirectiveSyntaxNode> GetRegionNodes(SyntaxNode root)
        {
            var regionDirectives = new List<SyntaxTrivia>();
            var endRegionDirectives = new List<SyntaxTrivia>();

            var directiveNodes = new List<DirectiveSyntaxNode>();
            // find all #region directives
            foreach (var regionDirective in root.DescendantTrivia().Where(i => i.Kind() == SyntaxKind.RegionDirectiveTrivia))
            {
                regionDirectives.Add(regionDirective);
            }

            // find all #endregion directives
            foreach (var endRegionDirective in root.DescendantTrivia().Where(j => j.Kind() == SyntaxKind.EndRegionDirectiveTrivia))
            {
                endRegionDirectives.Add(endRegionDirective);
            }

            // just in case: order by position in the document to allow further processing
            regionDirectives = regionDirectives
                .OrderBy(d => d.SpanStart)
                .ToList();

            endRegionDirectives = endRegionDirectives
                .OrderBy(d => d.SpanStart)
                .ToList();

            foreach (var regionDirective in regionDirectives)
            {
                var directiveNode = new DirectiveSyntaxNode { RegionDirective = regionDirective };

                // find #endregion directive for the corresponding #region
                foreach(var endRegionDirective in endRegionDirectives)
                {
                    // skip #endregion if it is earlier than #region
                    if(endRegionDirective.SpanStart < regionDirective.SpanStart)
                    {
                        continue;
                    }

                    // check for inner pairs of #region/#endregion directives
                    if(regionDirectives.Exists(d => d.SpanStart > regionDirective.SpanStart && d.SpanStart < endRegionDirective.SpanStart)
                        && !endRegionDirectives.Exists(d => d.SpanStart > regionDirective.SpanStart && d.SpanStart < endRegionDirective.SpanStart))
                    {
                        continue;
                    }

                    directiveNode.EndRegionDirective = endRegionDirective;

                    // add nodes that are placed between #region and #endregion directives
                    var descendantNodes = root.DescendantNodes()
                        .Where(t => (t is MemberDeclarationSyntax || t is StatementSyntax) &&
                            t.SpanStart > regionDirective.SpanStart && 
                            t.SpanStart < endRegionDirective.SpanStart)
                        .ToList();

                    directiveNode.AddNodes(descendantNodes);
                    directiveNodes.Add(directiveNode);
                    break;
                }
            }

            return directiveNodes;
        }

        /// <summary>
        /// Filters out #region directives with omp parameter
        /// </summary>
        /// <param name="nodes">All #region nodes in a document syntax tree</param>
        /// <returns>#region nodes with omp parameter</returns>
        public List<DirectiveSyntaxNode> FilterOmpNodes(List<DirectiveSyntaxNode> nodes)
        {
            List<DirectiveSyntaxNode> result = new List<DirectiveSyntaxNode>();

            foreach(DirectiveSyntaxNode node in nodes)
            {
                // defining of directive type
                DirectiveType type = DirectiveParser.GetDirectiveType(node.RegionDirective.ToFullString());
                if(type == DirectiveType.UNKNOWN)
                {
                    continue;
                }

                DirectiveParser parser = _factory.GetParser(type);
                // parsing of OpenMP-style parameters
                node.DirectiveInfo = parser.Parse(node.RegionDirective.ToFullString());
                result.Add(node);
            }

            return result;
        }

        /// <summary>
        /// Creates compilation for testing purposes
        /// </summary>
        /// <param name="tree">Document syntax tree</param>
        /// <returns>CSharpCompilation object</returns>
        public CSharpCompilation CreateCompilationFromSyntaxTree(SyntaxTree tree)
        {
            return CSharpCompilation.Create("AnalyzerCompilation")
                .AddReferences(
                     MetadataReference.CreateFromFile(
                     typeof(object).Assembly.Location))
                .AddSyntaxTrees(tree);
        }
    }
}
