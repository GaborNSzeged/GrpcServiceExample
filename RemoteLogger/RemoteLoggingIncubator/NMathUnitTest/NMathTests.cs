using CenterSpace.NMath.Core;
using System.Text;

namespace NMathUnitTest
{
    internal class NMathTests : BasicTest
    {
        [SetUp]
        public void Setup()
        {
            NMathConfiguration.LicenseKey = "2DC9CA276C4FD0F";
            NMathConfiguration.Init();

            NMathConfiguration.Reproducibility = true;
            NMathConfiguration.SetMKLNumThreads(1);
        }

        [Test]
        public void TestRightEigenVectors()
        {
            DoubleComplexMatrix? inputDupleDoubleComplexMatrix = LoadDuplexComplexMatrix("InputDoubleComplex.csv");

#if IsLatesNMath
            DoubleComplexMatrix? expectedRightEigenVectors = LoadDuplexComplexMatrix("RightEigenVectors_7_4_3_2.csv");
#else
            DoubleComplexMatrix? expectedRightEigenVectors = LoadDuplexComplexMatrix("RightEigenVectors_7_4_1_11.csv");
#endif

            var eigDecomp = new DoubleComplexEigDecomp(inputDupleDoubleComplexMatrix);

            // act
            DoubleComplexMatrix actualRightEigenVectors = eigDecomp.RightEigenVectors;
            

            Assert.That(actualRightEigenVectors, Is.EqualTo(expectedRightEigenVectors), "Creating RightEigenVectors filed.");
        }
    }
}