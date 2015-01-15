using RPUtil.Math.Math3D;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass()]
    public class OrthogonalAxisTest
    {
        Vector rhsR = new Vector(-1, 0, 0);
        Vector rhsF = new Vector(0, 0, -1);
        Vector rhsU = new Vector(0, 1, 0);
        Vector lhsR = new Vector(1, 0, 0);
        Vector lhsF = new Vector(0, 0, -1);
        Vector lhsU = new Vector(0, 1, 0);

        [TestMethod()]
        public void RhsUpTest()
        {
            Vector expected = rhsU;
            Vector actual = OrthogonalAxis.RhsUp(rhsR, rhsF);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void RhsRightTest()
        {
            Vector expected = rhsR;
            Vector actual= OrthogonalAxis.RhsRight(rhsU, rhsF);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void RhsForwardTest()
        {
            Vector expected = rhsF;
            Vector actual = OrthogonalAxis.RhsForward(rhsR, rhsU);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void LhsUpTest()
        {
            Vector expected = lhsU;
            Vector actual = OrthogonalAxis.LhsUp(lhsR, lhsF);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void LhsRightTest()
        {
            Vector expected = lhsR;
            Vector actual = OrthogonalAxis.LhsRight(lhsU, lhsF);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void LhsForwardTest()
        {
            Vector expected = lhsF;
            Vector actual = OrthogonalAxis.LhsForward(lhsR, lhsU);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void InvertedForwardRhsUpTest()
        {
            Vector expected = -rhsU;
            Vector actual = OrthogonalAxis.RhsUp(rhsR, -rhsF);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void InvertedForwardRhsRightTest()
        {
            Vector expected = rhsR;
            Vector actual = OrthogonalAxis.RhsRight(-rhsU, -rhsF);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void InvertedForwardRhsForwardTest()
        {
            Vector expected = -rhsF;
            Vector actual = OrthogonalAxis.RhsForward(rhsR, -rhsU);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void InvertedForwardLhsUpTest()
        {
            Vector expected = -lhsU;
            Vector actual = OrthogonalAxis.LhsUp(lhsR, -lhsF);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void InvertedForwardLhsRightTest()
        {
            Vector expected = lhsR;
            Vector actual = OrthogonalAxis.LhsRight(-lhsU, -lhsF);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void InvertedForwardLhsForwardTest()
        {
            Vector expected = -lhsF;
            Vector actual = OrthogonalAxis.LhsForward(lhsR, -lhsU);
            Assert.AreEqual(expected, actual);
        }
    }
}
