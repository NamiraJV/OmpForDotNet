using Microsoft.CodeAnalysis;
using OmpForDotNet.Utility.Entities;

namespace OmpForDotNet.Utility.Interfaces
{
    public interface ICodeGenerator
    {
        string Generate(OmpDirectiveInfo directiveInfo, DirectiveSyntaxNode node, SemanticModel model = null);
    }
}
