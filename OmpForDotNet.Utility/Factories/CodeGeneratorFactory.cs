using OmpForDotNet.Utility.Entities;
using OmpForDotNet.Utility.Generators;
using OmpForDotNet.Utility.Interfaces;

namespace OmpForDotNet.Utility.Factories
{
    /// <summary>
    /// Factory for code generator creation
    /// </summary>
    public class CodeGeneratorFactory
    {
        /// <summary>
        /// Creates a corresponding code generator
        /// </summary>
        /// <param name="directiveType">Type of the directive</param>
        /// <returns>Code generator</returns>
        public ICodeGenerator GetCodeGenerator(DirectiveType directiveType)
        {
            switch (directiveType)
            {
                case DirectiveType.OMP_PARALLEL:
                    return new SectionCodeGenerator();
                case DirectiveType.OMP_PARALLEL_FOR:
                    return new ForLoopCodeGenerator();
                default:
                    return null;
            }
        }
    }
}
