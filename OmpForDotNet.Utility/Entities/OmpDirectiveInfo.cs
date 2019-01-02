using System;
using System.Collections.Generic;
using System.Linq;

namespace OmpForDotNet.Utility.Entities
{
    /// <summary>
    /// Entity that represents information about OpenMP directive
    /// </summary>
    public class OmpDirectiveInfo : IEquatable<OmpDirectiveInfo>
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

        public bool Equals(OmpDirectiveInfo other)
        {
            if (this == other)
            {
                return true;
            }

            if (other == null)
            {
                return false;
            }

            if (this.Type != other.Type)
            {
                return false;
            }

            if (this.Options == null && other.Options != null ||
                this.Options != null && other.Options == null)
            {
                return false;
            }

            return this.Options
                    .OrderBy(kvp => kvp.Key)
                    .SequenceEqual(other.Options.OrderBy(kvp => kvp.Key));
        }
    }
}
