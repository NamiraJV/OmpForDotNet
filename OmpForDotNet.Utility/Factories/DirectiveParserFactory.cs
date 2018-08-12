using OmpForDotNet.Utility.Entities;
using OmpForDotNet.Utility.Parsers;

namespace OmpForDotNet.Utility.Factories
{
    /// <summary>
    /// Factory for directive parser creation
    /// </summary>
    public class DirectiveParserFactory
    {
        /// <summary>
        /// Creates a corresponding directive parameters parser
        /// </summary>
        /// <param name="type">Type of the directive</param>
        /// <returns>Directive parser</returns>
        public DirectiveParser GetParser(DirectiveType type)
        {
            switch (type)
            {
                case DirectiveType.OMP_PARALLEL_FOR:
                    return new ParallelForDirectiveParser();
                case DirectiveType.OMP_PARALLEL:
                    return new ParallelDirectiveParser();
                case DirectiveType.OMP_CRITICAL:
                    return new CriticalDirectiveParser();
                default:
                    return null;
            }
        }
    }
}
