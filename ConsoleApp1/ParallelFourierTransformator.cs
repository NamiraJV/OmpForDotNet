using System;
using System.Numerics;

namespace ImageProcessor.Calculation
{
    public class ParallelFourierTransformator
    {
        public Complex[][] DiscreteFourierTransformation(double[][] matrix)
        {
            Complex[][] transformationResult = new Complex[matrix[0].Length][];
            int rowLength = matrix[0].Length;

            //#region n13 omp parallel for schedule(static,5)
            for (int i = 0; i < rowLength; i++)
            {
                Complex[] complexArray = ConvertDoubleArrayToComplex(matrix[i]);
                transformationResult[i] = TransformArray(complexArray);
            }
            //#endregion n13

            Complex[][] finalTransformationResult = new Complex[matrix[0].Length][];
            //#region n14 omp parallel for schedule(static,5)
            for (int i = 0; i < rowLength; i++)
            {
                Complex[] column = GetColumn(i, transformationResult);
                finalTransformationResult[i] = TransformArray(column);
            }
           // #endregion

            Complex[][] transposedMatrix = TransposeMatrix(finalTransformationResult);

            return transposedMatrix;
        }

        public Complex[] TransformArray(Complex[] array)
        {
            // generate transformation matrix
            Complex[][] fourierTransformationMatrix = GenerateTransformationMatrix(array.Length);

            // multiply array and matrix
            Complex[] result = MultiplyMatrixAndArray(fourierTransformationMatrix, array);

            return NormalizeComplexArray(result);
        }

        public Complex[] NormalizeComplexArray(Complex[] array)
        {
            double ratio = 1 / Math.Sqrt(array.Length);
            int arrayLength = array.Length;
            for (int i = 0; i < arrayLength; i++)
            {
                array[i] = new Complex(array[i].Real * ratio, array[i].Imaginary * ratio);
            }

            return array;
        }

        public Complex[][] GenerateTransformationMatrix(int size)
        {
            Complex[][] transformationMatrix = new Complex[size][];

            for (int i = 0; i < size; i++)
            {
                transformationMatrix[i] = new Complex[size];
            }

            #region n2 omp parallel for
            for (int i = 1; i <= size; i++)
            {
                for (int j = 1; j <= i; j++)
                {
                    #region omp critical
                    double multiplier = -2 * Math.PI * (i - 1) * (j - 1) / size;
                    Complex matrixElement = Complex.Exp(new Complex(0, multiplier));
                    transformationMatrix[i - 1][j - 1] = transformationMatrix[j - 1][i - 1] = matrixElement;
                    #endregion
                }
            }
            #endregion n2

            return transformationMatrix;
        }

        public Complex[] MultiplyMatrixAndArray(Complex[][] matrix, Complex[] array)
        {
            Complex[] resultArray = new Complex[array.Length];

            int matrixLength = matrix.Length;
            int rowLength = matrix[0].Length;
            for (int i = 0; i < matrixLength; i++)
            {
                for (int j = 0; j < rowLength; j++)
                {
                    resultArray[i] = Complex.Add(resultArray[i], Complex.Multiply(matrix[i][j], array[j]));
                }
            }

            return resultArray;
        }

        private Complex[] ConvertDoubleArrayToComplex(double[] array)
        {
            int length = array.Length;
            Complex[] complexArray = new Complex[length];
            //#region n1 omp parallel for schedule(static,5)
            for (int i = 0; i < length; i++)
            {
                complexArray[i] = new Complex(array[i], 0);
            }
           // #endregion n1

            return complexArray;
        }

        public Complex[] GetColumn(int index, Complex[][] matrix)
        {
            Complex[] column = new Complex[matrix.GetLength(0)];

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                column[i] = matrix[i][index];
            }

            return column;
        }

        public Complex[][] TransposeMatrix(Complex[][] matrix)
        {
            Complex[][] transposedMatrix = new Complex[matrix.Length][];
            for (int i = 0; i < matrix[0].Length; i++)
            {
                transposedMatrix[i] = new Complex[matrix[0].Length];
            }

            int matrixLength = matrix.Length;
            int rowLength = matrix[0].Length;

           // #region n5 omp parallel for schedule(static,5)
            for (int i = 0; i < matrixLength; i++)
            {
                for (int j = 0; j < rowLength; j++)
                {
                    transposedMatrix[j][i] = matrix[i][j];
                }
            }
           // #endregion n5

            return transposedMatrix;
        }

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
            }

            //#region n6 omp parallel for
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    var d = (-1) * ((i + 1) * (i + 1) + (j + 1) * (j + 1)) / (2 * sigma * sigma);

                    matrix[i][j] = 1 / (2 * Math.PI * sigma * sigma) * Math.Exp(d);
                }
            }
           // #endregion n6

            return matrix;
        }

        public double[][] MultiplyMatrices(double[][] matrix, double[][] filter)
        {
            double[][] result = new double[matrix.Length][];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new double[matrix[0].Length];
            }

            int resultLength = result.Length;
            int rowLength = result[0].Length;

            for (int i = 0; i < resultLength; i++)
            {
                for (int j = 0; j < rowLength; j++)
                {
                    for (int k = 0; k < rowLength; k++)
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

            int matrixLength = matrix.Length;
            int rowLength = matrix[0].Length;

            for (int i = 0; i < matrixLength; i++)
            {
                result[i] = new double[matrix[0].Length];
            }


            //#region n8 omp parallel for schedule(static,5)
            for (int i = 0; i < matrixLength; i++)
            {
                for (int j = 0; j < rowLength; j++)
                {
                    result[i][j] = matrix[i][j] * filter[i][j];
                }
            }
            //#endregion n8

            return result;
        }

        public Complex[][] MultiplyMatricesByElements(Complex[][] filter, Complex[][] matrix)
        {
            Complex[][] result = new Complex[matrix.Length][];

            int matrixLength = matrix.Length;
            int rowLength = matrix[0].Length;

            for (int i = 0; i < matrixLength; i++)
            {
                result[i] = new Complex[matrix[0].Length];
            }

            for (int i = 0; i < matrixLength; i++)
            {
                for (int j = 0; j < rowLength; j++)
                {
                    result[i][j] = matrix[i][j] * filter[i][j];
                }
            }

            return result;
        }

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

        public Complex[][] InverseFourierTransformation(Complex[][] matrix)
        {
            Complex[][] transformationResult = new Complex[matrix[0].Length][];
            int rowLength = matrix[0].Length;
            //#region n15 omp parallel for schedule(static,5)
            for (int i = 0; i < rowLength; i++)
            {
                transformationResult[i] = InverseTransformArray(matrix[i]);
            }
            //#endregion n15

            Complex[][] finalTransformationResult = new Complex[matrix[0].Length][];
            //#region n16 omp parallel for schedule(static,5)
            for (int i = 0; i < rowLength; i++)
            {
                Complex[] column = GetColumn(i, transformationResult);
                finalTransformationResult[i] = InverseTransformArray(column);
            }
            //#endregion n16
            Complex[][] transposedMatrix = TransposeMatrix(finalTransformationResult);

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

            for (int i = 0; i < size; i++)
            {
                transformationMatrix[i] = new Complex[size];
            }

            for (int i = 1; i <= size; i++)
            {
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

            int matrixLength = matrix.Length;
            int rowLength = matrix[0].Length;

            for (int i = 0; i < matrixLength; i++)
            {
                for (int j = 0; j < rowLength; j++)
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

        public double[][] Log(double[][] matrix)
        {
            int matrixLength = matrix.Length;
            int rowLength = matrix[0].Length;

            //#region n12 omp parallel for schedule(static,5)
            for (int i = 0; i < matrixLength; i++)
            {
                for (int j = 0; j < rowLength; j++)
                {
                    matrix[i][j] = Math.Log(1 + matrix[i][j]);
                }
            }
            //#endregion n12
            return matrix;
        }
    }
}
