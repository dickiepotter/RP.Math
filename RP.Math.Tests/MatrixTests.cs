namespace RP.Math.Tests
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="Matrix"/> type.
    /// NOTE: RP.Math is in an early state; some of these document the intended behaviour and may fail.
    /// </summary>
    [TestClass]
    public class MatrixTests
    {
        [TestMethod, TestCategory("Matrix")]
        public void Identity_ShouldHaveOnesOnTheDiagonal_Test()
        {
            var m = Matrix.Identity;
            for (var i = 0; i < 4; i++)
            for (var j = 0; j < 4; j++)
            {
                m[i, j].Should().Be(i == j ? 1d : 0d, "identity element [{0},{1}]", i, j);
            }
        }

        [TestMethod, TestCategory("Matrix")]
        public void Identity_TimesIdentity_ShouldBeIdentity_Test()
        {
            var product = Matrix.Identity * Matrix.Identity;
            product.IsIdentity.Should().BeTrue();
        }

        [TestMethod, TestCategory("Matrix")]
        public void IdentityTimesVector_ShouldReturnSameVector_Test()
        {
            var v = new Vector(2, 3, 4);
            var result = Matrix.Identity * v;
            result.X.Should().Be(2);
            result.Y.Should().Be(3);
            result.Z.Should().Be(4);
        }

        [TestMethod, TestCategory("Matrix")]
        public void Transpose_OfIdentity_ShouldBeIdentity_Test()
        {
            Matrix.Identity.Transpose().IsIdentity.Should().BeTrue();
        }

        [TestMethod, TestCategory("Matrix")]
        public void Transpose_ShouldSwapRowsAndColumns_Test()
        {
            var m = new Matrix(new double[,]
            {
                { 1, 2, 3, 4 },
                { 5, 6, 7, 8 },
                { 9, 10, 11, 12 },
                { 13, 14, 15, 16 },
            });

            var t = m.Transpose();

            for (var i = 0; i < 4; i++)
            for (var j = 0; j < 4; j++)
            {
                t[i, j].Should().Be(m[j, i]);
            }
        }

        [TestMethod, TestCategory("Matrix")]
        public void TranslationMatrix_ShouldTranslateAPoint_Test()
        {
            var translate = Matrix.TranslationMatrix(10, 20, 30);
            var moved = translate * new Vector(1, 2, 3);
            moved.X.Should().Be(11);
            moved.Y.Should().Be(22);
            moved.Z.Should().Be(33);
        }

        [TestMethod, TestCategory("Matrix")]
        public void Determinant_OfIdentity_ShouldBeOne_Test()
        {
            Matrix.Identity.Determinant.Should().BeApproximately(1d, 1e-9);
        }
    }
}
