using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmpForDotNet.Utility.Entities
{
    /// <summary>
    /// Possible directive types
    /// </summary>
    public enum DirectiveType
    {
        UNKNOWN,
        OMP_PARALLEL,
        OMP_PARALLEL_FOR,
        OMP_CRITICAL
    }
}
