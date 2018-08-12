using OmpForDotNet.Utility.Entities;

namespace OmpForDotNet.Utility.Parsers
{
    public class ParallelDirectiveParser : DirectiveParser
    {
        private const string PARALLEL_CODE = "parallel";
        public override OmpDirectiveInfo Parse(string directive)
        {
            return new OmpDirectiveInfo(DirectiveType.OMP_PARALLEL_FOR,
                ParseDirectiveParameters(directive.Substring(directive.IndexOf(PARALLEL_CODE) + PARALLEL_CODE.Length)));
        }
    }
}
