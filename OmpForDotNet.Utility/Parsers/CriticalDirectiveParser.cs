using OmpForDotNet.Utility.Entities;

namespace OmpForDotNet.Utility.Parsers
{
    class CriticalDirectiveParser : DirectiveParser
    {
        private const string CRITICAL_CODE = "critical";
        public override OmpDirectiveInfo Parse(string directive)
        {
            return new OmpDirectiveInfo(DirectiveType.OMP_CRITICAL,
                ParseDirectiveParameters(directive.Substring(directive.IndexOf(CRITICAL_CODE) + CRITICAL_CODE.Length)));
        }
    }
}
