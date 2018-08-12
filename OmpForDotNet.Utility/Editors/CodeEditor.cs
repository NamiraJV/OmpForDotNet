namespace OmpForDotNet.Utility.Editors
{
    /// <summary>
    /// Class for source code editing
    /// </summary>
    public class CodeEditor
    {
        /// <summary>
        /// Replaces string in a document source code
        /// </summary>
        /// <param name="document">Document with source code</param>
        /// <param name="oldCode">Old piece of code to replace</param>
        /// <param name="newCode">Newly generated code to insert instead of old one</param>
        /// <returns></returns>
        public string ReplaceCodeString(string document, string oldCode, string newCode)
        {
            return document.Replace(oldCode, newCode);
        }
    }
}
