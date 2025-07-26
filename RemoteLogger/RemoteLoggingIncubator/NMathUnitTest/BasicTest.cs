using CenterSpace.NMath.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMathUnitTest
{
    internal class BasicTest
    {
        public DoubleComplexMatrix? LoadDuplexComplexMatrix(string fileName)
        {
            string sourcePath = GetResourcePath(fileName);
            string[] content = File.ReadAllLines(sourcePath);
            int rows = content.Length;
            int cols = content[0].Split('#').Length;

            var matrix = new DoubleComplexMatrix(rows, cols);

            for (int rowIndex = 0; rowIndex < rows; rowIndex++)
            {
                string[] rowRealImagPairs = content[rowIndex].Split('#');

                for (int colIndex = 0; colIndex < cols; colIndex++)
                {
                    string[] realImag = rowRealImagPairs[colIndex].Split(';');
                    matrix[rowIndex, colIndex] = new DoubleComplex(double.Parse(realImag[0]), double.Parse(realImag[1]));
                }
            }

            return matrix;
        }

        public string GetResourcePath(string resourceFileName)
        {
            return Path.Combine(Environment.CurrentDirectory, "TestResources", resourceFileName);
        }
    }
}
