using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using OmpForDotNet.Utility.CodeAnalysis;
using OmpForDotNet.Utility.Entities;
using OmpForDotNet.Utility.Generators;
using System;
using System.Collections.Generic;

namespace OmpForDotNet.Test
{
    [TestFixture]
    class CodeGeneratorTest
    {
        [Test]
        [TestCaseSource(nameof(TestData))]
        public void Test(string code, string expectedCode)
        {
            CodeAnalyzer analyzer = new CodeAnalyzer(new Utility.Factories.DirectiveParserFactory());
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
            SyntaxNode root = tree.GetRoot();
            var compilation = analyzer.CreateCompilationFromSyntaxTree(tree);
            List<DirectiveSyntaxNode> nodes = analyzer.GetRegionNodes(root);

            List<DirectiveSyntaxNode> ompNodes = analyzer.FilterOmpNodes(nodes);

            CodeGenerator generator = new CodeGenerator();

            SemanticModel model = compilation.GetSemanticModel(tree);

            string resultCode = generator.Generate(ompNodes[0].DirectiveInfo, ompNodes[0], model);
            Console.WriteLine(resultCode);

            Assert.AreEqual(expectedCode, resultCode);
        }

        private static TestCaseData[] TestData =
        {
            new TestCaseData(
@"
namespace N
{
    #region n1
    class C
    {
        void method()
        {
            int n = 10;

            int sharedVar = 0;

            Console.WriteLine(""Sequential"");
            for (int i = 0; i<n; i++)
            {
                sharedVar += i;
                Console.WriteLine(sharedVar);
            }

    Console.WriteLine(""Result = "" + sharedVar);

            sharedVar = 0;

            Console.WriteLine(""Parallel (with thread private)"");
            #region n3 omp parallel for firstprivate(sharedVar)
            for(int i = 0; i<n; i++)
            {
                sharedVar += i;
                Console.WriteLine(sharedVar);
            }
            #endregion n3
        }
    }
    #endregion n1
}",
@"ThreadPool.SetMaxThreads(4,4);
List<Task> task_gen_var_name_1 = new List<Task>();
for ( int i = 0 ; i < n ; i ++ )
{
var _gen_var_name_0 = i;
Task task_gen_var_name_0 = Task.Factory.StartNew(() => {
var _gen_var_name_1 = new int[]
Array.Copy(array, _gen_var_name_1, array.Length);
_gen_var_name_1 [ _gen_var_name_0 ] = _gen_var_name_0 ;
int k = 0 ;
});
task_gen_var_name_1.Add(task_gen_var_name_0);
}
foreach(var _gen_task in task_gen_var_name_1)
{
_gen_task.Wait();
}")
.SetName("Loop with array"),
new TestCaseData(
@"
namespace N
{
    #region n1
    class C
    {
        void method()
        {
            int n = 10;
            int[] array = new int[n];
            #region n2 omp parallel for num_threads(10)
            for (int i = 0; i < n; i++)
            { 
                array[i] = i;
                int k = 0;
            }
            #endregion n2
        }
    }
    #endregion n1
}",
@"ThreadPool.SetMaxThreads(10,10);
List<Task> task_gen_var_name_1 = new List<Task>();
for ( int i = 0 ; i < n ; i ++ )
{
var _gen_var_name_0 = i;
Task task_gen_var_name_0 = Task.Factory.StartNew(() => {
array [ _gen_var_name_0 ] = _gen_var_name_0 ;
int k = 0 ;
});
task_gen_var_name_1.Add(task_gen_var_name_0);
}
foreach(var _gen_task in task_gen_var_name_1)
{
_gen_task.Wait();
}")
.SetName("Loop with array (10 threads)"),

            new TestCaseData(
@"
namespace N
{
    class C
    {
        void method()
        {
            int n = 10;
            int[][] array = new int[n][];

            int[][] array1 = GenerateMatrix(n);
            int[][] array2 = GenerateMatrix(n);

            #region n1 omp parallel for
            for(int i = 0; i < n; i++)
            {
                for(int j = 0; j < n; j++)
                {
                    array[i][j] = array1[i][j] + array2[i][j];
                }
            }              
            #endregion n1
        }
    }
}",
@"ThreadPool.SetMaxThreads(4,4);
List<Task> _gen_task_list0 = new List<Task>();
for ( int i = 0 ; i < n ; i ++ )
{
var _gen_var_name_0 = i;
Task task_gen_var_name_0 = Task.Factory.StartNew(() => {
for ( int j = 0 ; j < n ; j ++ ) { array [ _gen_var_name_0 ] [ j ] = array1 [ _gen_var_name_0 ] [ j ] + array2 [ _gen_var_name_0 ] [ j ] ; }
});
_gen_task_list0.Add(task_gen_var_name_0);
}
foreach(var _gen_task_00 in _gen_task_list0)
{
_gen_task_00.Wait();
}")
.SetName("Matrix addition"),

            new TestCaseData(
@"
namespace N
{
    class C
    {
        void method()
        {
            int n = 10;
            int[][] array = new int[n][];

            int[][] array1 = GenerateMatrix(n);
            int[][] array2 = GenerateMatrix(n);

            #region n1 omp parallel for schedule(guided,5)
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    for (int k = 0; k < n; k++)
                    {
                        array[i][j] += array1[k][j] * array2[i][k];
                    }
                }
            }        
            #endregion n1
        }
    }
}",
@"ThreadPool.SetMaxThreads(4,4);
List<Task> _gen_task_list0 = new List<Task>();
for ( int i = 0 ; i < n ; i ++ )
{
var _gen_var_name_0 = i;
Task task_gen_var_name_0 = Task.Factory.StartNew(() => {
for ( int j = 0 ; j < n ; j ++ ) { for ( int k = 0 ; k < n ; k ++ ) { array [ _gen_var_name_0 ] [ j ] += array1 [ k ] [ j ] * array2 [ _gen_var_name_0 ] [ k ] ; } }
});
_gen_task_list0.Add(task_gen_var_name_0);
}
foreach(var _gen_task_00 in _gen_task_list0)
{
_gen_task_00.Wait();
}")
.SetName("Matrix multiplication"),
            new TestCaseData(
@"
namespace N
{
    class C
    {
        void method()
        {
            int sharedVar = 0;

            #region n3 omp parallel for threadprivate(sharedVar)
            for(int i = 0; i < n; i++)
            {
                sharedVar += i;
                Console.WriteLine(sharedVar);
            }
            #endregion n3
        }
    }
}",
@"ThreadPool.SetMaxThreads(4,4);
List<Task> _gen_task_list0 = new List<Task>();
for ( int i = 0 ; i < n ; i ++ )
{
var _gen_var_name_0 = i;
Task task_gen_var_name_0 = Task.Factory.StartNew(() => {
for ( int j = 0 ; j < n ; j ++ ) { for ( int k = 0 ; k < n ; k ++ ) { array [ _gen_var_name_0 ] [ j ] += array1 [ k ] [ j ] * array2 [ _gen_var_name_0 ] [ k ] ; } }
});
_gen_task_list0.Add(task_gen_var_name_0);
}
foreach(var _gen_task_00 in _gen_task_list0)
{
_gen_task_00.Wait();
}")
.SetName("Shared Var"),

                        new TestCaseData(
@"
namespace N
{
    class C
    {
        void method()
        {
            int sharedVar = 0;

            #region n3 omp parallel for schedule(static,5)
            for(int i = 0; i < n; i++)
            {
                #region omp critical
                sharedVar += i;
                Console.WriteLine(sharedVar);
                #endregion
            }
            #endregion n3
        }
    }
}",
@"")
.SetName("critical"),

new TestCaseData(
@"
namespace N
{
    class C
    {
        void method()
        {
            Complex[] resultArray = new Complex[array.Length];

            int matrixLength = matrix.Length;
            int rowLength = 0;
            #region n3 omp parallel for
            for (int i = 0; i < matrixLength; i++)
            {
                rowLength = matrix[0].Length;
                for (int j = 0; j < rowLength; j++)
                {
                    resultArray[i] = Complex.Add(resultArray[i], Complex.Multiply(matrix[i][j], array[j]));
                }
            }
            #endregion n3
        }
    }
}
",
@"")
.SetName("convert array"),
        };
    }
}
