using OmpForDotNet.Utility.Entities;

namespace OmpForDotNet.Utility.Parsers
{
    public class ParallelForDirectiveParser : DirectiveParser
    {
        private const string FOR_CODE = "for";
        public override OmpDirectiveInfo Parse(string directive)
        {
            return new OmpDirectiveInfo(DirectiveType.OMP_PARALLEL_FOR, 
                ParseDirectiveParameters(directive.Substring(directive.IndexOf(FOR_CODE) + FOR_CODE.Length)));
        }
    }
}
