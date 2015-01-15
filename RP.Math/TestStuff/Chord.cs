
namespace RPUtil.Math.Math3D
{
    public class Chord: LineSegment
    {
        #region Properties

        public double Radius { get; set; }

        #endregion

        #region Constructors

        public Chord(Vector tail, Vector head, double radius)
            :base(tail, head)
        {
            Radius = radius;
        }

        public Chord
        (
            double xt,
            double yt,
            double zt,
            double xh,
            double yh,
            double zh,
            double radius
        ): base(xt, yt, zt, xh, yh, zh)
        {
            Radius = radius;
        }

        public Chord(double[] arr, double radius)
            : base(arr)
        {
            Radius = radius;
        }

        public Chord(double[,] arr, double radius)
            :base (arr)
        {
            Radius = radius;
        }

        #endregion
    }
}
