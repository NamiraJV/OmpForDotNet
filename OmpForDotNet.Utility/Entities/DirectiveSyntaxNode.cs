using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;

namespace OmpForDotNet.Utility.Entities
{
    /// <summary>
    /// Entity that represents syntax node for further processing
    /// </summary>
    public class DirectiveSyntaxNode : IEquatable<DirectiveSyntaxNode>
    {
        /// <summary>
        /// OmpDirectiveInfo
        /// </summary>
        public OmpDirectiveInfo DirectiveInfo { get; set; }

        /// <summary>
        /// #region directive syntax trivia
        /// </summary>
        public SyntaxTrivia RegionDirective { get; set; }

        /// <summary>
        /// #endregion directive syntax trivia
        /// </summary>
        public SyntaxTrivia EndRegionDirective { get; set; }

        /// <summary>
        /// Entity that represents position of #region/#endregion directives in a source code
        /// </summary>
        public TextSpan RegionSpan
        {
            get
            {
                var start = RegionDirective.Span.Start;
                var end = EndRegionDirective.Span.Start + EndRegionDirective.Span.Length;
                return new TextSpan(start, end - start);
            }
        }

        /// <summary>
        /// Syntax nodes surrounded by #region/#endregion directives
        /// </summary>
        public List<SyntaxNode> Nodes = new List<SyntaxNode>();

        /// <summary>
        /// Allows to add a new syntax node
        /// </summary>
        /// <param name="node">Node to add</param>
        public void AddNode(SyntaxNode node)
        {
            if (RegionSpan.Contains(node.Span))
            {
                Nodes.Add(node);
            }
        }

        /// <summary>
        /// Allows to add several syntax nodes
        /// </summary>
        /// <param name="nodes">List of nodes to add</param>
        public void AddNodes(IEnumerable<SyntaxNode> nodes)
        {
            foreach(var node in nodes)
            {
                if (RegionSpan.Contains(node.Span))
                {
                    Nodes.Add(node);
                }
            }
        }

        public bool Equals(DirectiveSyntaxNode other)
        {
            if (this == other)
            {
                return true;
            }

            if (other == null)
            {
                return false;
            }

            return this.DirectiveInfo.Equals(other.DirectiveInfo) &&
                this.RegionDirective.Equals(other.RegionDirective) &&
                this.EndRegionDirective.Equals(other.EndRegionDirective);
        }
    }
}
