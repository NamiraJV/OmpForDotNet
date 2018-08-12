using System.Collections.Generic;

namespace OmpForDotNet.Utility.Entities
{
    /// <summary>
    /// Entity that represents information about OpenMP directive
    /// </summary>
    public class OmpDirectiveInfo
    {
        /// <summary>
        /// Type of the directive
        /// </summary>
        public DirectiveType Type { get; }

        /// <summary>
        /// Parsed options of the directive
        /// </summary>
        public Dictionary<string, string[]> Options { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Directive type</param>
        /// <param name="options">Directive options</param>
        public OmpDirectiveInfo(DirectiveType type, Dictionary<string, string[]> options)
        {
            Type = type;
            Options = options;
        }
    }
}
