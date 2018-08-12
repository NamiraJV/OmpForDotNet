using OmpForDotNet.Utility.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class RegionNodeRange
    {
        public DirectiveSyntaxNode Node { get; set; }

        public int StartIndex { get; set; }

        public int EndIndex { get; set; }
    }
}
