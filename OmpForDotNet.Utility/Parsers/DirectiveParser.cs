using OmpForDotNet.Utility.Entities;
using System.Collections.Generic;

namespace OmpForDotNet.Utility.Parsers
{
    public abstract class DirectiveParser
    {
        public static DirectiveType GetDirectiveType(string directive)
        {
            if(directive.Contains("omp parallel for"))
            {
                return DirectiveType.OMP_PARALLEL_FOR;
            }

            if (directive.Contains("omp parallel"))
            {
                return DirectiveType.OMP_PARALLEL;
            }
            
            return DirectiveType.UNKNOWN;
        }

        public virtual Dictionary<string, string[]> ParseDirectiveParameters(string parameters)
        {
            parameters = parameters?.Trim();
            if (string.IsNullOrEmpty(parameters))
            {
                return null;
            }

            var resultDictionary = new Dictionary<string, string[]>();

            string[] splittedParameterStrings = parameters.Split(' ');
            foreach(string paramString in splittedParameterStrings)
            {
                if (string.IsNullOrEmpty(paramString.Trim()))
                {
                    continue;
                }

                var parsedParameter = ParseOneParameter(paramString);

                resultDictionary.Add(parsedParameter.Key, parsedParameter.Value);
            }

            return resultDictionary;
        }

        private KeyValuePair<string, string[]> ParseOneParameter(string parameter)
        {
            int indexOfOpenBracket = parameter.IndexOf('(');
            int indexOfCloseBracket = parameter.IndexOf(')');
        
            string parameterName = parameter.Substring(0, indexOfOpenBracket);
            string[] parameterValues = parameter.Substring(indexOfOpenBracket + 1, indexOfCloseBracket - indexOfOpenBracket - 1).Split(',');

            return new KeyValuePair<string, string[]>(parameterName, parameterValues);
        }

        public abstract OmpDirectiveInfo Parse(string directive);
    }
}
