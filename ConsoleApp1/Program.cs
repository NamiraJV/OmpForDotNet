using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = @"..\..\ParallelFourierTransformator.cs";
            StreamReader reader = new StreamReader(fileName);
            string str = reader.ReadToEnd();
            reader.Close();
            List<string> result = new List<string>();
            try
            {
                string solutionPath = @"D:\Programs\git\roslyn\OmpForDotNet";//Directory.GetParent(Directory.GetCurrentDirectory()).ToString();
                var files = System.IO.Directory.GetFiles(solutionPath, "*.sln");
                string solutionFile = System.IO.Directory.GetFiles(solutionPath, "*.sln")[0];
                CodeProcessor codeProcessor = new CodeProcessor();
                //var task = Task.Run(() => codeProcessor.ProcessSolution(Directory.GetCurrentDirectory().ToString(), solutionFile));
                //task.Wait();
                result = codeProcessor.ProcessSolution(Directory.GetCurrentDirectory().ToString(), solutionFile).Result;
            }
            /*catch (AggregateException ex)
            {
                StreamWriter writer1 = new StreamWriter(@"D:\8bit\exs.txt");
                var loaderExs = ((System.Reflection.ReflectionTypeLoadException)ex.InnerException).LoaderExceptions;
                foreach (var ex1 in loaderExs)
                {
                    writer1.WriteLine("EXCEPTION");
                    writer1.WriteLine(ex1.ToString());
                }
                writer1.Close();
            }*/
            catch (Exception ex)
            {
                StreamWriter writer1 = new StreamWriter(@"D:\8bit\ex.txt");
                writer1.WriteLine(ex.ToString());

                writer1.Close();
            }
            //str = str.Replace("Program.cs", "Pum.cs");

            str = ReplaceFileNamesFromCommand(str, result);

            StreamWriter writer = new StreamWriter(fileName);
            writer.AutoFlush = true;
            writer.WriteLine(str);

            writer.Close();
            writer = new StreamWriter(@"D:\8bit\testtest.txt");
            writer.AutoFlush = true;
            writer.WriteLine(fileName);
            writer.WriteLine(str);

            // TODO: figure out a way to configure this path to the compiler
            var p = Process.Start(@"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\Roslyn\csc.exe",
                string.Join(" ", args));
            p.WaitForExit();

            foreach (string file in result)
            {
                File.Delete(file.Substring(0, file.Length - 3) + "_tmp_generated_doc.cs");
            }
        }

        private static string ReplaceFileNamesFromCommand(string command, List<string> namesToReplace)
        {
            StreamWriter writer = new StreamWriter(@"D:\8bit\log.txt");
            writer.AutoFlush = true;
            writer.WriteLine("BEFORE");
            writer.WriteLine(command);
            string[] args = command.Split(' ');
            int startIndex = 0;
            int endIndex = 0;
            for (int i = 0, n = args.Length; i < n; i++)
            {
                if (args[i] == "/utf8output")
                {
                    startIndex = i + 1;
                    writer.WriteLine("AFTER /utf8output");
                    writer.WriteLine(args[i + 1]);
                }

                if (args[i].StartsWith("\""))
                {
                    endIndex = i;
                }
            }

            for (int i = startIndex; i < endIndex; i++)
            {
                if (namesToReplace.Contains(args[i]))
                {
                    args[i] = args[i].Substring(0, args[i].Length - 3) + "_tmp_generated_doc.cs";
                    writer.WriteLine("AFTER REPLACEMENT");
                    writer.WriteLine(args[i]);
                }
            }
            writer.WriteLine(string.Join(" ", args));
            writer.Close();
            return string.Join(" ", args);
        }
    }
}
