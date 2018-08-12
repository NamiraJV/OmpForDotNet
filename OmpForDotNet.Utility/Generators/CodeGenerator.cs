using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using OmpForDotNet.Utility.Entities;
using OmpForDotNet.Utility.Factories;
using OmpForDotNet.Utility.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OmpForDotNet.Utility.Generators
{
    /// <summary>
    /// Class for code generation
    /// </summary>
    public class CodeGenerator
    {
        /// <summary>
        /// Code generator factory
        /// </summary>
        private CodeGeneratorFactory _codeGeneratorFactory = new CodeGeneratorFactory();

        /// <summary>
        /// Method for code generation
        /// </summary>
        /// <param name="directiveInfo">Directive info</param>
        /// <param name="node">Syntax node to process</param>
        /// <param name="model">Semantic model</param>
        /// <returns>Generated code</returns>
        public string Generate(OmpDirectiveInfo directiveInfo, DirectiveSyntaxNode node, SemanticModel model = null)
        {
            var generator = _codeGeneratorFactory.GetCodeGenerator(directiveInfo.Type);
            return generator.Generate(directiveInfo, node, model);
        }
        
        /// <summary>
        /// Allows to replace one set of tokens with another one
        /// </summary>
        /// <param name="oldNode">Source node</param>
        /// <param name="oldTokens">Tokens to replace</param>
        /// <param name="newTokens">Tokens to replace with</param>
        /// <returns></returns>
        protected SyntaxNode ReplaceTokensInSyntaxNode(SyntaxNode oldNode, SyntaxToken[] oldTokens, SyntaxToken[] newTokens)
        {
            var oldNodeTokenList = oldNode.DescendantTokens();

            var newNodeTokenList = new List<SyntaxToken>();
            foreach (var token in oldNodeTokenList)
            {
                SyntaxToken? oldToken = null;
                int index = -1;
                for(int i = 0, length = oldTokens.Length; i < length; i++)
                {
                    // if token is in the list of tokens to replace - remember its index
                    if(token.Text == oldTokens[i].Text)
                    {
                        oldToken = token;
                        index = i;
                        break;
                    }
                }

                if(index == -1)
                {
                    // add old token to list: it does not have to be replaced
                    newNodeTokenList.Add(token);
                }
                else
                {
                    newNodeTokenList.Add(newTokens[index]);
                }
            }

            string newCode = string.Join(" ", newNodeTokenList.Select(t => t.Text));
            string[] splitted = newCode.Split(';');
            // insert semicolon to the end of each code line
            newCode = string.Join($";{Environment.NewLine}", splitted);
            SyntaxNode newNode = GenerateSyntaxNodeFromString(newCode);

            return newNode;
        }

        /// <summary>
        /// Converts string to root syntax node
        /// </summary>
        /// <param name="code">Code for conversion</param>
        /// <returns>Root of the parsed tree</returns>
        protected SyntaxNode GenerateSyntaxNodeFromString(string code)
        {
            return CSharpSyntaxTree.ParseText(
                code).GetRoot();
        }

        /// <summary>
        /// Generates variable name
        /// </summary>
        /// <returns>Variable name</returns>
        protected string GenerateVariableName()
        {
            string variableName = $"_gen_var_name_{OpenMPConstants.GeneratedVariablesCount}";
            // increase number of generated variable to avoid names duplication
            OpenMPConstants.GeneratedVariablesCount++;

            return variableName;
        }

        /// <summary>
        /// Generates name for the variable of Task type
        /// </summary>
        /// <returns>Variable name</returns>
        protected string GenerateTaskVariableName()
        {
            string variableName = $"task_gen_var_name_{ OpenMPConstants.GeneratedTaskVariablesCount}";
            // increase number of generated task variable to avoid names duplication
            OpenMPConstants.GeneratedTaskVariablesCount++;

            return variableName;
        }

        /// <summary>
        /// Generates code line with a variable declaration
        /// </summary>
        /// <param name="variableName">Variable name</param>
        /// <param name="valueToSave">Value to save in a variable</param>
        /// <returns>Syntax node with variable declaration</returns>
        protected SyntaxNode GenerateVariableDeclaration(string variableName, string valueToSave)
        {
            var expression = GenerateSyntaxNodeFromString(
                $"var {variableName} = {valueToSave};");

            return expression;
        }


        /// <summary>
        /// Generates code line with a task variable declaration
        /// </summary>
        /// <param name="taskVariableName">Task variable name</param>
        /// <param name="taskDelegateBody">Task delegate body to save in a variable</param>
        /// <returns>Syntax node with declaration</returns>
        protected SyntaxNode GenerateTaskVarDeclaration(string taskVariableName, string taskDelegateBody)
        {
            var expression = GenerateSyntaxNodeFromString(
                $"Task {taskVariableName} = Task.Factory.StartNew({taskDelegateBody});");

            return expression;
        }
    }
}
