using OmpForDotNet.Utility.Interfaces;
using System;
using Microsoft.CodeAnalysis;
using OmpForDotNet.Utility.Entities;

namespace OmpForDotNet.Utility.Generators
{
    public class SectionCodeGenerator : CodeGenerator, ICodeGenerator
    {
        public new string Generate(OmpDirectiveInfo directiveInfo, DirectiveSyntaxNode node, SemanticModel model = null)
        {
            throw new NotImplementedException();
        }
    }
}
