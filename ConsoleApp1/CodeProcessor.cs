using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using OmpForDotNet.Utility.CodeAnalysis;
using OmpForDotNet.Utility.Editors;
using OmpForDotNet.Utility.Generators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class CodeProcessor
    {
        private CodeAnalyzer _analyzer = new CodeAnalyzer(new OmpForDotNet.Utility.Factories.DirectiveParserFactory());
        private CodeEditor _editor = new CodeEditor();
        private CodeGenerator _generator = new CodeGenerator();

        public async Task<List<string>> ProcessSolution(string solutionPath, string solutionFile)
        {
            List<string> docNamesToReplace = new List<string>();
            StreamWriter writer = new StreamWriter(@"D:\8bit\names1.txt", true);
            writer.AutoFlush = true;
            writer.WriteLine("SOLUTION");
            writer.WriteLine(solutionPath);
            writer.Close();
            try
            {
                Solution solution = await _analyzer.GetSolutionByPath(solutionFile);

                foreach (Project project in solution.Projects)
                {
                    var docNames = await ProcessProject(solutionPath, solution, project);
                    docNamesToReplace.AddRange(docNames);
                }

                return docNamesToReplace;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new List<string> { "" };
            }
        }

        private async Task<List<string>> ProcessProject(string solutionPath, Solution solution, Project project)
        {
            List<string> docNamesToReplace = new List<string>();
            StreamWriter writer = new StreamWriter(@"D:\8bit\names.txt", true);
            writer.AutoFlush = true;
            writer.WriteLine("PROJECT");
            writer.WriteLine(project.Name);
            foreach (Document document in project.Documents)
            {
                writer.WriteLine("DOCUMENT");
                writer.WriteLine(document.Name);
                var tree = await document.GetSyntaxTreeAsync();
                var root = await tree.GetRootAsync();

                var regionNodes = _analyzer.GetRegionNodes(root);
                var ompNodes = _analyzer.FilterOmpNodes(regionNodes);

                if (!ompNodes.Any())
                {
                    continue;
                }

                var semanticModel = GetSemanticModel(tree);

                string documentCode = (await document.GetTextAsync()).ToString();
                int i = 0;
                StreamWriter bb = new StreamWriter(@"D:\8bit\bb1.txt", true);
                StreamWriter log = new StreamWriter(@"D:\8bit\event.log", true);

                int nodesAmount = ompNodes.Count;
                var ranges = new List<RegionNodeRange>();
                foreach (var node in ompNodes)
                {
                    ranges.Add(new RegionNodeRange
                    {
                        Node = node,
                        StartIndex = node.RegionSpan.Start,
                        EndIndex = node.RegionSpan.End
                    });
                }

                foreach (var node in ranges)
                {
                    string beginning = documentCode.Substring(node.StartIndex, node.EndIndex - node.StartIndex);
Console.WriteLine(beginning);

                    log.WriteLine($"Iteration {i}");
                    bb.AutoFlush = true;
                    bb.WriteLine("BEFORE");
                    bb.WriteLine(documentCode);
                    string code = _generator.Generate(node.Node.DirectiveInfo, node.Node, semanticModel);
                    bb.WriteLine("CODE");
                    bb.WriteLine(code);
                    bb.WriteLine("CODE TO REPLQCE");
                    int codeLength = code.Length;
                    bb.WriteLine(documentCode.Substring(node.StartIndex, node.EndIndex - node.StartIndex).ToString());
                    documentCode = _editor.ReplaceCodeString(documentCode, documentCode.Substring(node.StartIndex, node.EndIndex - node.StartIndex), code);
                    log.WriteLine($"RegionSpan.Start = {node.StartIndex}, RegionSpan.End = {node.EndIndex}, length = {node.EndIndex - node.StartIndex}");
                    bb.WriteLine("AFTER");
                    bb.WriteLine(documentCode);
                    log.WriteLine(documentCode);
                    i++;

                    foreach(var innerNode in ranges)
                    {
                        if(innerNode.StartIndex > node.EndIndex)
                        {
                            int oldCodeLength = node.EndIndex - node.StartIndex;
                            if (oldCodeLength < codeLength)
                            {
                                innerNode.StartIndex += codeLength - oldCodeLength;
                                innerNode.EndIndex += codeLength - oldCodeLength;
                            }
                            if(oldCodeLength > codeLength)
                            {
                                innerNode.StartIndex -= oldCodeLength - codeLength;
                                innerNode.EndIndex -= oldCodeLength - codeLength;
                            }
                        }
                    }

                    node.EndIndex += codeLength;
                }

                /*foreach (var node in ompNodes)
                {
                    log.WriteLine($"Iteration {i}");
                    bb.AutoFlush = true;
                    bb.WriteLine("BEFORE");
                    bb.WriteLine(documentCode);
                    string code = _generator.Generate(node.DirectiveInfo, node, semanticModel);
                    bb.WriteLine("CODE");
                    bb.WriteLine(code);
                    bb.WriteLine("CODE TO REPLQCE");
                    int len = code.Length;
                    bb.WriteLine(documentCode.Substring(node.RegionSpan.Start, node.RegionSpan.End - node.RegionSpan.Start).ToString());
                    documentCode = _editor.ReplaceCodeString(documentCode, documentCode.Substring(node.RegionSpan.Start, node.RegionSpan.End - node.RegionSpan.Start), code);
                    log.WriteLine($"RegionSpan.Start = {node.RegionSpan.Start}, RegionSpan.End = {node.RegionSpan.End}, length = {node.RegionSpan.End - node.RegionSpan.Start}");
                    bb.WriteLine("AFTER");
                    bb.WriteLine(documentCode);
                    log.WriteLine(documentCode);
                    i++;
                }*/

                bb.Close();
                log.Close();

                docNamesToReplace.Add(document.Name);
                string newDocumentName = $"{document.Name.Substring(0, document.Name.Length - 3)}_tmp_generated_doc";
                StreamWriter documentWriter = new StreamWriter(solutionPath + "\\" + newDocumentName + ".cs");

                documentWriter.WriteLine(documentCode);
                documentWriter.Close();
            }
            writer.Close();

            return docNamesToReplace;
        }

        private SemanticModel GetSemanticModel(SyntaxTree tree)
        {
            var compilation = CSharpCompilation.Create("TestCompilation")
                                               .AddReferences(
                                                    MetadataReference.CreateFromFile(
                                                        typeof(object).Assembly.Location))
                                               .AddSyntaxTrees(tree);

            return compilation.GetSemanticModel(tree);
        }
    }
}
