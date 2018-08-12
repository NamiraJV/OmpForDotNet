using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Test
    {
        // TODO: test
        public Complex[][] DiscreteFourierTransformation(double[][] matrix)
        {
            matrix = /*Centralize(*/matrix/*)*/;
            Complex[][] transformationResult = new Complex[matrix[0].Length][];
            for (int i = 0; i < matrix[0].Length; i++)
            {
                Complex[] complexArray = ConvertDoubleArrayToComplex(matrix[i]);
                transformationResult[i] = TransformArray(complexArray);
            }

            Complex[][] finalTransformationResult = new Complex[matrix[0].Length][];
            for (int i = 0; i < matrix[0].Length; i++)
            {
                Complex[] column = GetColumn(i, transformationResult);
                finalTransformationResult[i] = TransformArray(column);
            }

            Complex[][] transposedMatrix = TransposeMatrix(finalTransformationResult);

            //var m = ConvertComplexMatrixToDouble(transposedMatrix);

            //return m;
            return transposedMatrix;
        }

        private double[][] Centralize(double[][] matrix)
        {
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < matrix[0].Length; j++)
                {
                    matrix[i][j] *= (i + j) / 2 == 0 ? 1 : -1;
                }
            }

            return matrix;
        }

        public Complex[] TransformArray(Complex[] array)
        {
            // generate transformation matrix
            Complex[][] fourierTransformationMatrix = GenerateTransformationMatrix(array.Length);

            // multiply array and matrix
            Complex[] result = MultiplyMatrixAndArray(fourierTransformationMatrix, array);

            return NormalizeComplexArray(result);
        }

        // TODO: test (DONE)
        public Complex[] NormalizeComplexArray(Complex[] array)
        {
            double ratio = 1 / Math.Sqrt(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new Complex(array[i].Real * ratio, array[i].Imaginary * ratio);
            }

            return array;
        }

        // TODO: test (DONE)
        public Complex[][] GenerateTransformationMatrix(int size)
        {
            Complex[][] transformationMatrix = new Complex[size][];

            for (int i = 0; i < size; i++)
            {
                transformationMatrix[i] = new Complex[size];
            }


            var _gen_var_name_0 = size / 4;
            var _gen_var_name_1 = size / _gen_var_name_0 + (size % _gen_var_name_0 > 0 ? 1 : 0);
            var _gen_var_name_2 = 4;
            var _gen_var_name_9 = _gen_var_name_1 / _gen_var_name_2 + (_gen_var_name_1 % _gen_var_name_2 == 0 ? 0 : 1);
            var _gen_var_name_8 = _gen_var_name_0 * _gen_var_name_2;
            System.Threading.ThreadPool.SetMaxThreads(_gen_var_name_2, _gen_var_name_2);
            List<Task> task_gen_var_name_0 = new List<Task>();
            for (var _gen_var_name_3 = 0; _gen_var_name_3 < _gen_var_name_2; _gen_var_name_3++)
            {
                var _gen_var_name_4 = _gen_var_name_3;
                var task_gen_var_name_1 = Task.Factory.StartNew(() => {
                    for (var _gen_var_name_6 = 0; _gen_var_name_6 < _gen_var_name_9; _gen_var_name_6++)
                    {
                        var _gen_var_name_7 = _gen_var_name_6;
                        for (int _gen_var_name_5 = _gen_var_name_4 * _gen_var_name_0 + _gen_var_name_7 * _gen_var_name_8;
                        _gen_var_name_5 < _gen_var_name_4 * _gen_var_name_0 + _gen_var_name_7 * _gen_var_name_8 +
                        _gen_var_name_0 && _gen_var_name_5 < size; _gen_var_name_5++)
                        {

                            for (int j = 1;
                             j <= _gen_var_name_5;
                             j++)
                            {
                                double multiplier = -2 * Math.PI * (_gen_var_name_5 - 1) * (j - 1) / size;
                                Complex matrixElement = Complex.Exp(new Complex(0, multiplier));
                                transformationMatrix[_gen_var_name_5 - 1][j - 1] = transformationMatrix[j - 1][_gen_var_name_5 - 1] = matrixElement;
                            }
                        }
                    }
                }
                );
                task_gen_var_name_0.Add(task_gen_var_name_1);
            }
            foreach (var _gen_var_name_10 in task_gen_var_name_0)
            {
                _gen_var_name_10.Wait();
            }

            return transformationMatrix;
        }

        // TODO: test (DONE)
        public Complex[] MultiplyMatrixAndArray(Complex[][] matrix, Complex[] array)
        {
            Complex[] resultArray = new Complex[array.Length];

            int matrixLength = matrix.Length;
            int rowLength = 0;

            var _gen_var_name_11 = matrixLength / 4;
            var _gen_var_name_12 = matrixLength / _gen_var_name_11 + (matrixLength % _gen_var_name_11 > 0 ? 1 : 0);
            var _gen_var_name_13 = 4;
            var _gen_var_name_20 = _gen_var_name_12 / _gen_var_name_13 + (_gen_var_name_12 % _gen_var_name_13 == 0 ? 0 : 1);
            var _gen_var_name_19 = _gen_var_name_11 * _gen_var_name_13;
            System.Threading.ThreadPool.SetMaxThreads(_gen_var_name_13, _gen_var_name_13);
            List<Task> task_gen_var_name_2 = new List<Task>();
            for (var _gen_var_name_14 = 0; _gen_var_name_14 < _gen_var_name_13; _gen_var_name_14++)
            {
                var _gen_var_name_15 = _gen_var_name_14;
                var task_gen_var_name_3 = Task.Factory.StartNew(() => {
                    for (var _gen_var_name_17 = 0; _gen_var_name_17 < _gen_var_name_20; _gen_var_name_17++)
                    {
                        var _gen_var_name_18 = _gen_var_name_17;
                        for (int _gen_var_name_16 = _gen_var_name_15 * _gen_var_name_11 + _gen_var_name_18 * _gen_var_name_19;
                        _gen_var_name_16 < _gen_var_name_15 * _gen_var_name_11 + _gen_var_name_18 * _gen_var_name_19 +
                        _gen_var_name_11 && _gen_var_name_16 < matrixLength; _gen_var_name_16++)
                        {

                            for (int j = 0;
                             j < rowLength;
                             j++)
                            {
                                resultArray[_gen_var_name_16] = Complex.Add(resultArray[_gen_var_name_16], Complex.Multiply(matrix[_gen_var_name_16][j], array[j]));
                            }
                        }
                    }
                }
                );
                task_gen_var_name_2.Add(task_gen_var_name_3);
            }
            foreach (var _gen_var_name_21 in task_gen_var_name_2)
            {
                _gen_var_name_21.Wait();
            }

            return resultArray;
        }

        private Complex[] ConvertDoubleArrayToComplex(double[] array)
        {
            int length = array.Length;
            Complex[] complexArray = new Complex[length];

            var _gen_var_name_22 = length / 4;
            var _gen_var_name_23 = length / _gen_var_name_22 + (length % _gen_var_name_22 > 0 ? 1 : 0);
            var _gen_var_name_24 = 4;
            var _gen_var_name_31 = _gen_var_name_23 / _gen_var_name_24 + (_gen_var_name_23 % _gen_var_name_24 == 0 ? 0 : 1);
            var _gen_var_name_30 = _gen_var_name_22 * _gen_var_name_24;
            System.Threading.ThreadPool.SetMaxThreads(_gen_var_name_24, _gen_var_name_24);
            List<Task> task_gen_var_name_4 = new List<Task>();
            for (var _gen_var_name_25 = 0; _gen_var_name_25 < _gen_var_name_24; _gen_var_name_25++)
            {
                var _gen_var_name_26 = _gen_var_name_25;
                var task_gen_var_name_5 = Task.Factory.StartNew(() => {
                    for (var _gen_var_name_28 = 0; _gen_var_name_28 < _gen_var_name_31; _gen_var_name_28++)
                    {
                        var _gen_var_name_29 = _gen_var_name_28;
                        for (int _gen_var_name_27 = _gen_var_name_26 * _gen_var_name_22 + _gen_var_name_29 * _gen_var_name_30;
                        _gen_var_name_27 < _gen_var_name_26 * _gen_var_name_22 + _gen_var_name_29 * _gen_var_name_30 +
                        _gen_var_name_22 && _gen_var_name_27 < length; _gen_var_name_27++)
                        {

                            complexArray[_gen_var_name_27] = new Complex(array[_gen_var_name_27], 0);

                        }
                    }
                }
                );
                task_gen_var_name_4.Add(task_gen_var_name_5);
            }
            foreach (var _gen_var_name_32 in task_gen_var_name_4)
            {
                _gen_var_name_32.Wait();
            }

            return complexArray;
        }

        // TODO: test (DONE)
        public Complex[] GetColumn(int index, Complex[][] matrix)
        {
            Complex[] column = new Complex[matrix.GetLength(0)];

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                column[i] = matrix[i][index];
            }

            return column;
        }

        // TODO: test (DONE)
        public Complex[][] TransposeMatrix(Complex[][] matrix)
        {
            Complex[][] transposedMatrix = new Complex[matrix.Length][];
            for (int i = 0; i < matrix[0].Length; i++)
            {
                transposedMatrix[i] = new Complex[matrix[0].Length];
            }

            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < matrix[0].Length; j++)
                {
                    transposedMatrix[j][i] = matrix[i][j];
                }
            }

            return transposedMatrix;
        }

        // TODO: test (DONE)
        public double[][] ConvertComplexMatrixToDouble(Complex[][] matrix)
        {
            double[][] doubleMatrix = new double[matrix.Length][];

            for (int i = 0; i < matrix.Length; i++)
            {
                doubleMatrix[i] = new double[matrix[0].Length];
                for (int j = 0; j < matrix[0].Length; j++)
                {
                    doubleMatrix[i][j] = Complex.Abs(matrix[i][j]);
                }
            }

            return doubleMatrix;
        }

        public double[][] GetGaussianFilterMatrix(int n, int m, double sigma)
        {
            double[][] matrix = new double[n][];
            for (int i = 0; i < n; i++)
            {
                matrix[i] = new double[m];
                for (int j = 0; j < m; j++)
                {
                    //var d = DFunction(n, m, i + 1, j + 1);
                    //matrix[i][j] = 1 - Math.Exp((-1) * d * d / 2 / DFunction(n, m, 0, 0));

                    var d = (-1) * ((i + 1) * (i + 1) + (j + 1) * (j + 1)) / (2 * sigma * sigma);

                    matrix[i][j] = 1 / (2 * Math.PI * sigma * sigma) * Math.Exp(d);
                }
            }

            return matrix;

            /*double[][] matrix = new double[n][];
            for (int i = 0; i < n; i++)
            {
                matrix[i] = new double[m];
            }
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    //var d = DFunction(n, m, i + 1, j + 1);
                    //matrix[i][j] = 1 - Math.Exp((-1) * d * d / 2 / DFunction(n, m, 0, 0));

                    var d = (-1) * ((i + 1) * (i + 1) + (j + 1) * (j + 1)) / (2 * sigma * sigma);

                    matrix[i][j] = 1 / (2 * Math.PI * sigma * sigma) * Math.Exp(d);
                }
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    matrix[i][j] = matrix[i % 5][j % 5];
                }
            }

            return matrix;*/

            /*double[][] matrix = new double[n][];
            for (int i = 0; i < n; i++)
            {
                matrix[i] = new double[m];
            }

            matrix[0][0] = 0.003;
            matrix[0][1] = 0.013;
            matrix[0][2] = 0.022;
            matrix[0][3] = 0.013;
            matrix[0][4] = 0.003;

            matrix[1][0] = 0.013;
            matrix[1][1] = 0.059;
            matrix[1][2] = 0.097;
            matrix[1][3] = 0.059;
            matrix[1][4] = 0.013;

            matrix[2][0] = 0.022;
            matrix[2][1] = 0.097;
            matrix[2][2] = 0.159;
            matrix[2][3] = 0.097;
            matrix[2][4] = 0.022;

            matrix[3][0] = 0.013;
            matrix[3][1] = 0.059;
            matrix[3][2] = 0.097;
            matrix[3][3] = 0.059;
            matrix[3][4] = 0.013;

            matrix[4][0] = 0.003;
            matrix[4][1] = 0.013;
            matrix[4][2] = 0.022;
            matrix[4][3] = 0.013;
            matrix[4][4] = 0.003;

            for (int i = 0; i < n; i++)
            {
                for(int j = 0; j < m; j++)
                {
                    matrix[i][j] = matrix[i % 5][j % 5];
                }
            }

            return matrix;*/
        }

        public double GetGaussianFilterValue(int i, int j, double sigma)
        {
            var d = (-1) * ((i + 1) * (i + 1) + (j + 1) * (j + 1)) / (2 * sigma * sigma);
            return 1 / (2 * Math.PI * sigma * sigma) * Math.Exp(d);
        }

        public double DFunction(int n, int m, int i, int j)
        {
            return Math.Sqrt((i - n / 2) * (i - n / 2) + (j - m / 2) * (j - m / 2));
        }

        public double[][] MultiplyMatrices(double[][] matrix, double[][] filter)
        {
            double[][] result = new double[matrix.Length][];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new double[matrix[0].Length];
            }

            for (int i = 0; i < result.Length; i++)
            {
                for (int j = 0; j < result[0].Length; j++)
                {
                    for (int k = 0; k < result[0].Length; k++)
                    {
                        result[i][j] += filter[i][k] * matrix[k][j];
                    }
                }
            }

            return result;
        }

        public double[][] MultiplyMatricesByElements(double[][] filter, double[][] matrix)
        {
            double[][] result = new double[matrix.Length][];

            for (int i = 0; i < matrix[0].Length; i++)
            {
                result[i] = new double[matrix[0].Length];
                for (int j = 0; j < matrix[0].Length; j++)
                {
                    result[i][j] = matrix[i][j] * filter[i][j];
                }
            }

            return result;
        }

        public Complex[][] MultiplyMatricesByElements(Complex[][] filter, Complex[][] matrix)
        {
            Complex[][] result = new Complex[matrix.Length][];

            for (int i = 0; i < matrix[0].Length; i++)
            {
                result[i] = new Complex[matrix[0].Length];
                for (int j = 0; j < matrix[0].Length; j++)
                {
                    result[i][j] = matrix[i][j] * filter[i][j];
                }
            }

            return result;
        }

        /*       public double[][] Transform(double[][] image)
               {
                   var matrix = DiscreteFourierTransformation(image);
                   var filter = DiscreteFourierTransformation(GetGaussianFilterMatrix(image.Length, image[0].Length, 0.9));
                   //var filter = DiscreteFourierTransformation(GaussCrenel(1, image.Length));

                   var result = MultiplyMatricesByElements(matrix, filter);
                   return InverseFourierTransformation(result);
               }
       */
        public double[][] GaussCrenel(float sigma, int radius)
        {
            int k = 2 * radius + 1;
            double coeff = 1.0f / (2.0f * (double)Math.PI * sigma * sigma);
            double[][] A = new double[k][];
            for (int i = 0; i < k; i++)
            {
                A[i] = new double[k];
            }
            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < k; j++)
                {
                    A[i][j] = coeff * (double)Math.Exp(-(Math.Pow(k - i - 1, 2) + Math.Pow(k - j - 1, 2)) / (2 * sigma * sigma));
                }
            }
            return A;
        }


        public double[][] InverseFourierTransformation(double[][] matrix)
        {
            Complex[][] transformationResult = new Complex[matrix[0].Length][];
            for (int i = 0; i < matrix[0].Length; i++)
            {
                Complex[] complexArray = ConvertDoubleArrayToComplex(matrix[i]);
                transformationResult[i] = InverseTransformArray(complexArray);
            }

            Complex[][] finalTransformationResult = new Complex[matrix[0].Length][];
            for (int i = 0; i < matrix[0].Length; i++)
            {
                Complex[] column = GetColumn(i, transformationResult);
                finalTransformationResult[i] = InverseTransformArray(column);
            }
            Complex[][] transposedMatrix = TransposeMatrix(finalTransformationResult);

            var m = /*Centralize(*/ConvertComplexMatrixToDouble(transposedMatrix);//);

            return m;
        }

        public Complex[][] InverseFourierTransformation(Complex[][] matrix)
        {
            Complex[][] transformationResult = new Complex[matrix[0].Length][];
            for (int i = 0; i < matrix[0].Length; i++)
            {
                transformationResult[i] = InverseTransformArray(matrix[i]);
            }

            Complex[][] finalTransformationResult = new Complex[matrix[0].Length][];
            for (int i = 0; i < matrix[0].Length; i++)
            {
                Complex[] column = GetColumn(i, transformationResult);
                finalTransformationResult[i] = InverseTransformArray(column);
            }
            Complex[][] transposedMatrix = TransposeMatrix(finalTransformationResult);

            //var m = /*Centralize(*/ConvertComplexMatrixToDouble(transposedMatrix);//);

            //return m;
            return transposedMatrix;
        }

        public Complex[] InverseTransformArray(Complex[] array)
        {
            // generate transformation matrix
            Complex[][] fourierTransformationMatrix = GenerateInverseTransformationMatrix(array.Length);

            // multiply array and matrix
            Complex[] result = MultiplyMatrixAndArray(fourierTransformationMatrix, array);

            return NormalizeComplexArray(result);
        }

        public Complex[][] GenerateInverseTransformationMatrix(int size)
        {
            Complex[][] transformationMatrix = new Complex[size][];

            for (int i = 1; i <= size; i++)
            {
                transformationMatrix[i - 1] = new Complex[size];
                for (int j = 1; j <= i; j++)
                {
                    double multiplier = 2 * Math.PI * (i - 1) * (j - 1) / size;
                    Complex matrixElement = Complex.Exp(new Complex(0, multiplier));
                    transformationMatrix[i - 1][j - 1] = transformationMatrix[j - 1][i - 1] = matrixElement;
                }
            }

            return transformationMatrix;
        }

        private double[][] Shift(double[][] matrix)
        {
            double[][] result = new double[matrix.Length][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new double[matrix[0].Length];
            }

            int middleX = matrix.Length / 2;
            int middleY = matrix[0].Length / 2;

            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < matrix[0].Length; j++)
                {
                    if (i < middleX && j < middleY)
                    {
                        result[i][j] = matrix[i + middleX][j + middleY];
                    }

                    if (i >= middleX && j >= middleY)
                    {
                        result[i][j] = matrix[i - middleX][j - middleY];
                    }

                    if (i < middleX && j >= middleY)
                    {
                        result[i][j] = matrix[i + middleX][j - middleY];
                    }

                    if (i >= middleX && j < middleY)
                    {
                        result[i][j] = matrix[i - middleX][j + middleY];
                    }
                }
            }

            return result;
        }

        private double[][] Log(double[][] matrix)
        {
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < matrix[0].Length; j++)
                {
                    matrix[i][j] = Math.Log(1 + matrix[i][j]);
                }
            }

            return matrix;
        }
    }
}




