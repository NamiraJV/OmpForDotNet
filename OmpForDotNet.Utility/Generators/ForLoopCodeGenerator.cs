using OmpForDotNet.Utility.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using OmpForDotNet.Utility.Entities;
using OmpForDotNet.Utility.Settings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OmpForDotNet.Utility.CodeAnalysis;
using System.IO;

namespace OmpForDotNet.Utility.Generators
{
    /// <summary>
    /// Generator of parallel for loop code
    /// </summary>
    public class ForLoopCodeGenerator : CodeGenerator, ICodeGenerator
    {
        /// <summary>
        /// Generates parallel for loop
        /// </summary>
        /// <param name="directiveInfo">Directive info</param>
        /// <param name="node">Syntax node that represents the for loop</param>
        /// <param name="model">Semantic model</param>
        /// <returns>Generated code</returns>
        public new string Generate(OmpDirectiveInfo directiveInfo, DirectiveSyntaxNode node, SemanticModel model = null)
        {
            StreamWriter debug = new StreamWriter(@"D:\8bit\debug_some_bug.txt");
            debug.WriteLine("New Version");

            debug.WriteLine(node.Nodes.Count);
            debug.Close();
            // get all tokens that form for loop
            IList<SyntaxToken> descendantTokens = node.Nodes[0].DescendantTokens().ToList<SyntaxToken>();

            //suppose for now that there can be only one for loop variable
            if (descendantTokens[0].ToString() == "for")
            {
                // save initial number of iterations in for loop
                SyntaxToken? initialNumberOfIterationsValue = GetNumberOfIterations(descendantTokens);

                // get loop variable
                SyntaxToken loopVariable = GetLoopVariable(descendantTokens);

                // get for loop body
                List<SyntaxNode> forLoopBody = node.Nodes.GetRange(2, node.Nodes.Count - 2);

                // TODO: in case of a mistake we probably should return the initial version of code, not an empty string
                if (initialNumberOfIterationsValue == null)
                {
                    return "";
                }

                // get max possible number of threads in a thread pool
                int threadsAmountValue = GetNumberOfThreads(directiveInfo);

                // TODO: in case of a mistake we probably should return the initial version of code, not an empty string
                if (directiveInfo == null)
                {
                    return string.Empty;
                }

                // if there are no options specified by the user, add static scheduling by default
                if(directiveInfo.Options == null || !directiveInfo.Options.Any())
                {
                    directiveInfo.Options = new Dictionary<string, string[]>()
                    {
                        {"schedule", new string[] { "static" }}
                    };
                }

                // TODO: remove redundant check
                if (directiveInfo?.Options != null)
                {
                    // if user did not specify schedule option in directive parameters, add static scheduling by default
                    if (!directiveInfo.Options.ContainsKey(OpenMPConstants.SCHEDULE_OPTION))
                    {
                        directiveInfo.Options.Add("schedule", new string[] { "static" });
                    }
                    var options = directiveInfo.Options[OpenMPConstants.SCHEDULE_OPTION];

                    // TODO: verify that default value for chunk in static and dynamic is correct
                    // according to the standard in the following code
                    string chunkValue = options.Count() > 1
                        ? options[1]
                        : options[0] == "static" ? $"{initialNumberOfIterationsValue} / {threadsAmountValue}" : "1";

                    // generate chunk size variable name and declaration
                    string chunkName = GenerateVariableName();

                    string chunkVariableDeclaration =
                        $"var {chunkName} = {chunkValue};";

                    // generate calculation of iteration portions amount
                    string numberOfPortionsValue = $"{initialNumberOfIterationsValue} / {chunkName} + " +
                        $"({initialNumberOfIterationsValue} % {chunkName} > 0 ? 1 : 0)";

                    string numberOfPortionsName = GenerateVariableName();

                    string numberOfPortionsVariableDeclaration =
                    $"var {numberOfPortionsName} = {numberOfPortionsValue};";

                    // variables declarations
                    string taskListName = GenerateTaskVariableName();
                    string threadsAmountName = GenerateVariableName();
                    string threadsAmountIndexName = GenerateVariableName();
                    string threadsAmountIndexInnerName = GenerateVariableName();
                    string taskVariableName = GenerateTaskVariableName();
                    string innerIterationIndexName = GenerateVariableName();
                    string threadPortionIndexName = GenerateVariableName();
                    string threadPortionIndexInnerName = GenerateVariableName();

                    // the size of portion that is executed by all threads one by one
                    string bigPortionSizeName = GenerateVariableName();

                    SyntaxToken innerIterationIndexToken = SyntaxFactory.IdentifierName(innerIterationIndexName).GetFirstToken();

                    string threadsAmountDeclaration =
                        $"var {threadsAmountName} = {threadsAmountValue};";

                    string numberOfPortionsPerThreadValue = $"{numberOfPortionsName} / {threadsAmountName} + " +
                        $"({numberOfPortionsName} % {threadsAmountName} == 0 ? 0 : 1)";

                    string numberOfPortionsPerThreadName = GenerateVariableName();

                    string numberOfPortionsPerThreadDeclaration = 
                        $"var {numberOfPortionsPerThreadName} = {numberOfPortionsPerThreadValue};";

                    string bigPortionSizeValue = $"{chunkName} * {threadsAmountName}";

                    string bigPortionSizeDeclaration = $"var {bigPortionSizeName} = {bigPortionSizeValue};";

                    /**/
                    var threadBody = GetThreadBody(forLoopBody, directiveInfo, node, model, loopVariable, innerIterationIndexToken);
                    /**/

                    string finalCode = string.Empty;

                    /*CRITICAL SECTION*/
                    CodeAnalyzer analyzer = new CodeAnalyzer(new Factories.DirectiveParserFactory());
                    var innerOmpNodes = analyzer.GetRegionNodes(node.Nodes[1]);

                    // for now suppose that there is only one inner openmp directive
                    if (innerOmpNodes != null && innerOmpNodes.Count > 0)
                    {
                        var innerOmpNode = innerOmpNodes[0];
                        var directiveString = innerOmpNode.RegionDirective.ToFullString().Trim();

                        var lockObjectName = GenerateVariableName();

                        if (directiveString.Contains(OpenMPConstants.CRITICAL_DIRECTIVE))
                        {
                            finalCode += $"var {lockObjectName} = new object();";

                            threadBody = $"lock({lockObjectName}){Environment.NewLine}{{" +
                                $"{Environment.NewLine}{threadBody}{Environment.NewLine}}}";
                        }
                    }

                    /*CRITICAL SECTION*/

                    string foreachLoopVariableName = GenerateVariableName();
                    
                    if (options[0] == "static")
                    {
                        List<string> finalCodeList = new List<string>
                        {
                            finalCode,
                            chunkVariableDeclaration,
                            numberOfPortionsVariableDeclaration,
                            threadsAmountDeclaration,
                            numberOfPortionsPerThreadDeclaration,
                            bigPortionSizeDeclaration,
                            $"System.Threading.ThreadPool.SetMaxThreads({threadsAmountName},{threadsAmountName});",
                            $" System.Collections.Generic.List<System.Threading.Tasks.Task> {taskListName} = new  System.Collections.Generic.List<System.Threading.Tasks.Task>();",
                            $"for (var {threadsAmountIndexName} = 0; {threadsAmountIndexName} < {threadsAmountName}; {threadsAmountIndexName}++)",
                            "{",
                            $"var {threadsAmountIndexInnerName} = {threadsAmountIndexName};",
                            $"var {taskVariableName} = System.Threading.Tasks.Task.Factory.StartNew(() => {{",
                            $"for (var {threadPortionIndexName} = 0; {threadPortionIndexName} < {numberOfPortionsPerThreadName}; {threadPortionIndexName}++)",
                            "{",
                            $"var {threadPortionIndexInnerName} = {threadPortionIndexName};",
                            $"for (int {innerIterationIndexName} = {threadsAmountIndexInnerName} * {chunkName} + {threadPortionIndexInnerName} * {bigPortionSizeName}; ",
                            $"{innerIterationIndexName} < {threadsAmountIndexInnerName} * {chunkName} + {threadPortionIndexInnerName} * {bigPortionSizeName} + ",
                            $"{chunkName} && {innerIterationIndexName} < {initialNumberOfIterationsValue}; {innerIterationIndexName}++)",
                            "{",
                            threadBody,
                            "}",
                            "}",
                            "}",
                            ");",
                            $"{taskListName}.Add({taskVariableName});",
                            "}",
                            $"foreach(var {foreachLoopVariableName} in {taskListName}){{",
                            $"{foreachLoopVariableName}.Wait();",
                            "}"
                        };

                        return string.Join(Environment.NewLine, finalCodeList);
                    }

                    if (options[0] == "dynamic")
                    {
                        string globalIterationIndexName = GenerateVariableName();
                        string numberOfGlobalIterationsValue = $"{numberOfPortionsName} / {threadsAmountName} + " +
                            $"({numberOfPortionsName} % {threadsAmountName} == 0 ? 0 : 1 )";
                        string numberOfGlobalIterationsName = GenerateVariableName();
                        string numberOfGlobalIterationsDeclaration = $"var {numberOfGlobalIterationsName} = {numberOfGlobalIterationsValue};";

                        string globalIterationIndexInnerName = GenerateVariableName();
                        string globalIterationIndexInnerDeclaration =
                            $"var {globalIterationIndexInnerName} = {globalIterationIndexName};";

                        string threadsAmountIndexInnerDeclaration =
                            $"var {threadsAmountIndexInnerName} = {threadsAmountIndexName};";

                        List<string> finalCodeList = new List<string>
                        {
                             finalCode,
                             chunkVariableDeclaration,
                             numberOfPortionsVariableDeclaration,
                             threadsAmountDeclaration,
                             numberOfGlobalIterationsDeclaration,
                             bigPortionSizeDeclaration,
                             $"System.Threading.ThreadPool.SetMaxThreads({threadsAmountName},{threadsAmountName});",
                             $" System.Collections.Generic.List<System.Threading.Tasks.Task> {taskListName} = new  System.Collections.Generic.List<System.Threading.Task>();",
                             $"for (var {globalIterationIndexName} = 0; {globalIterationIndexName} < {numberOfGlobalIterationsName}; {globalIterationIndexName}++)",
                             "{",
                             globalIterationIndexInnerDeclaration,
                             $"for (var {threadsAmountIndexName} = 0; {threadsAmountIndexName} < {threadsAmountName}; {threadsAmountIndexName}++)",
                              "{",
                              threadsAmountIndexInnerDeclaration,
                              $"var {taskVariableName} = System.Threading.Tasks.Task.Factory.StartNew(() => ",
                              "{",
                              $"for (int {innerIterationIndexName} = {threadsAmountIndexInnerName} * {chunkName} + {globalIterationIndexInnerName} * {bigPortionSizeName}; ",
                              $"{innerIterationIndexName} < {threadsAmountIndexInnerName} * {chunkName} + {globalIterationIndexInnerName} * ",
                              $"{bigPortionSizeName} + {chunkName} && {innerIterationIndexName} < {initialNumberOfIterationsValue}; {innerIterationIndexName}++)",
                              "{",
                              threadBody,
                              "}",
                              "}",
                              ");",
                              $"{taskListName}.Add({taskVariableName});",
                              "}",
                              "}",
                              $"foreach(var {foreachLoopVariableName} in {taskListName}){{",
                              $"{foreachLoopVariableName}.Wait();",
                              "}"
                        };

                        return string.Join(Environment.NewLine, finalCodeList);
                    }

                    if(options[0] == "guided")
                    {
                        string portionSizeName = GenerateVariableName();
                        string portionSizeDeclaration =
                            $"var {portionSizeName} = {initialNumberOfIterationsValue} / {threadsAmountName};";

                        string numberOfLeftIterationsName = GenerateVariableName();
                        string numberOfLeftIterationsDeclaration =
                            $"var {numberOfLeftIterationsName} = {initialNumberOfIterationsValue};";

                        string threadIndexName = GenerateVariableName();

                        string numberOfExecutedIterationsName = GenerateVariableName();
                        string numberOfExecutedIterationsDeclaration =
                            $"var {numberOfExecutedIterationsName} = 0;";

                        string numberOfExecutedIterationsInnerName = GenerateVariableName();

                        string numberOfIterationsName = GenerateVariableName();
                        string numberOfIterationsValue =
                            $"Math.Max({numberOfLeftIterationsName} - {portionSizeName}, " +
                            $"{threadsAmountName} * {chunkName})";

                        List<string> finalCodeList = new List<string>
                        {
                            finalCode,
                            chunkVariableDeclaration,
                            threadsAmountDeclaration,
                            portionSizeDeclaration,
                            numberOfLeftIterationsDeclaration,
                            numberOfExecutedIterationsDeclaration,
                            $"System.Threading.ThreadPool.SetMaxThreads({threadsAmountName},{threadsAmountName});",
                            $" System.Collections.Generic.List<System.Threading.Tasks.Task> {taskListName} = new  System.Collections.Generic.List<System.Threading.Tasks.Task>();",
                            $"while({numberOfLeftIterationsName} > 0)",
                            "{",
                            $"for(int {threadIndexName} = 0; {threadIndexName} < {threadsAmountName} && {numberOfLeftIterationsName} > 0;",
                            $" {threadIndexName}++)",
                            "{",
                            $"int {numberOfIterationsName};",
                            $"if({portionSizeName} >= {numberOfLeftIterationsName})",
                            "{",
                            $"{numberOfIterationsName} = {numberOfLeftIterationsName};",
                            "}",
                            $"else",
                            "{",
                            $"{numberOfIterationsName} = {numberOfIterationsValue};",
                            "}",
                            $"{numberOfLeftIterationsName} -= {numberOfIterationsName};",
                            $"var {numberOfExecutedIterationsInnerName} = {numberOfExecutedIterationsName};",
                            $"var {taskVariableName} = System.Threading.Tasks.Task.Factory.StartNew(() => ",
                            "{",
                            $"for (int {innerIterationIndexName} = {numberOfExecutedIterationsInnerName}; ",
                            $"{innerIterationIndexName} < {numberOfExecutedIterationsInnerName} + {numberOfIterationsName}; {innerIterationIndexName}++)",
                            "{",
                            threadBody,
                            "}",
                            "});",
                            $"{taskListName}.Add({taskVariableName});",
                            $"{numberOfExecutedIterationsName} += {numberOfIterationsName};",
                            "}",
                            "}",
                            $"foreach(var {foreachLoopVariableName} in {taskListName}){{",
                            $"{foreachLoopVariableName}.Wait();",
                            "}"
                        };

                        return string.Join(Environment.NewLine, finalCodeList);
                    }
                }
            }

            return "";
        }

        private SyntaxToken GetLoopVariable(IList<SyntaxToken> descendantTokens)
        {
            return OpenMPConstants.ForLoopVariableTypes
                    .Contains(descendantTokens[2].Text)
                    ? descendantTokens[3]
                    : descendantTokens[2];
        }

        private SyntaxToken? GetNumberOfIterations(IList<SyntaxToken> tokens)
        {
            if (tokens[0].ToString() == "for")
            {
                int indexOfLoopPart = 0;
                int index = 0;
                for (int i = 0; i < tokens.Count; i++)
                {
                    if (tokens[i].ToString() == ";")
                    {
                        if (indexOfLoopPart == 1)
                        {
                            index = i - 1;
                            break;
                        }
                        indexOfLoopPart++;
                        
                    }
                }

                return tokens[index];
            }

            return null;
        }

        private int GetNumberOfThreads(OmpDirectiveInfo directiveInfo)
        {
            int threadsAmountValue = OpenMPConstants.OMP_NUM_THREADS;

            if (directiveInfo?.Options != null && directiveInfo.Options.ContainsKey(OpenMPConstants.NUM_THREADS_OPTION))
            {
                threadsAmountValue = int.Parse(directiveInfo.Options[OpenMPConstants.NUM_THREADS_OPTION][0].ToString());
            }

            return threadsAmountValue;
        }

        private string GetThreadBody(List<SyntaxNode> forLoopBody, OmpDirectiveInfo directiveInfo, 
            DirectiveSyntaxNode node, SemanticModel model, SyntaxToken loopVariable, SyntaxToken innerIterationIndexToken)
        {
            List<SyntaxNode> nodesForReplacement = new List<SyntaxNode>();

            // in case there is a for loop inside for loop...
            var internalFor = forLoopBody
                .FirstOrDefault(n => n.GetType() == typeof(ForStatementSyntax));
            if (internalFor != null)
            {
                nodesForReplacement.Add(internalFor);
            }
            if (!nodesForReplacement.Any())
            {
                nodesForReplacement = forLoopBody
                    .FindAll(n =>
                    n.GetType() == typeof(ExpressionStatementSyntax) ||
                    n.GetType() == typeof(LocalDeclarationStatementSyntax));
            }

            string[] variables = null;
            string[] threadPrivateVariables = null;
            string[] variableTypes = null;
            if (directiveInfo?.Options != null && directiveInfo.Options.ContainsKey(OpenMPConstants.FIRST_PRIVATE_OPTION))
            {
                variables = directiveInfo.Options[OpenMPConstants.FIRST_PRIVATE_OPTION];
                int length = variables.Count();
                threadPrivateVariables = new string[length];
                variableTypes = new string[length];

                for (int k = 0; k < length; k++)
                {
                    SyntaxNode nodeToGetType = node.Nodes[0].DescendantNodes().FirstOrDefault(
                        i =>
                        i is IdentifierNameSyntax && i.ToString() == variables[k]);

                    var semanticInfo = model.GetSymbolInfo(nodeToGetType);
                    ILocalSymbol symbol = (ILocalSymbol)semanticInfo.Symbol;
                    var symbolType = symbol.Type.ToString();

                    variableTypes[k] = symbolType;

                    string threadPrivateVarName = GenerateVariableName();

                    threadPrivateVariables[k] = threadPrivateVarName;
                }
            }

            string threadPrivateVariablesDeclaration = string.Empty;
            if (threadPrivateVariables != null)
            {
                for (int i = 0, length = threadPrivateVariables.Length; i < length; i++)
                {
                    threadPrivateVariablesDeclaration += Environment.NewLine;
                    // currently we support only primitive types to avoid
                    // problems with complex object replication
                    if (variableTypes[i] == "int[]" || variableTypes[i] == "double[]" ||
                        variableTypes[i] == "string[]")
                    {
                        threadPrivateVariablesDeclaration +=
                            $"var {threadPrivateVariables[i]} = new " +
                            $"{variableTypes[i].Substring(0, variableTypes[i].Length - 1)}{variables[i]}.Length];{Environment.NewLine}" +
                            $"Array.Copy({variables[i]}, {threadPrivateVariables[i]}, {variables[i]}.Length);";
                    }
                    else
                    {
                        threadPrivateVariablesDeclaration +=
                            "var " +
                            threadPrivateVariables[i] +
                            " = " +
                            variables[i] +
                            ";";
                    }
                }
            }

            var threadBody = new List<SyntaxNode>();

            var oldTokens = new List<SyntaxToken>
                {
                    loopVariable
                };

            var newTokens = new List<SyntaxToken>
                {
                    innerIterationIndexToken
                };

            if (threadPrivateVariables != null)
            {
                for (int k = 0, length = threadPrivateVariables.Count(); k < length; k++)
                {
                    oldTokens.Add(node.Nodes[0].DescendantTokens().FirstOrDefault(t =>
                       t.Text == variables[k]));
                    newTokens.Add(SyntaxFactory.IdentifierName(threadPrivateVariables[k]).GetFirstToken());
                }
            }

            foreach (SyntaxNode n in nodesForReplacement)
            {
                var replacedNode = ReplaceTokensInSyntaxNode(n, oldTokens.ToArray(), newTokens.ToArray());

                threadBody.Add(replacedNode);
            }

            return threadPrivateVariablesDeclaration +
                    Environment.NewLine +
                    string.Join(Environment.NewLine, threadBody.Select(n => n.ToFullString()));
        }
    }
}
