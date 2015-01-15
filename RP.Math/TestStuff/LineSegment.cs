using System;
using System.Xml.Serialization;

namespace RPUtil.Math.Math3D
{
    public class LineSegment
    {
        #region Constructors

        public LineSegment(Vector tail, Vector head)
        {
            Tail = tail;
            Head = head;
        }

        public LineSegment
        (
            double xt,
            double yt,
            double zt,
            double xh,
            double yh,
            double zh
        )
        {
            Tail = new Vector(xt, yt, zt);
            Head = new Vector(xh, yh, zh);
        }

        public LineSegment(double[] arr)
        {
            if (arr.Length != 6) throw new ArgumentException(SIX_COMPONENTS, "arr");
            Tail = new Vector(arr[0], arr[1], arr[2]);
            Head = new Vector(arr[3], arr[4], arr[5]);
        }

        public LineSegment(double[,] arr)
        {
            if (arr.GetLength(0) != 3 || arr.GetLength(1) != 2)
                throw new ArgumentException(THREE_TWO_DIMENSIONS, "arr");

            Tail = new Vector(arr[0, 0], arr[0, 1], arr[0, 2]);
            Head = new Vector(arr[1, 0], arr[1, 1], arr[1, 2]);
        }

        #endregion

        #region Accessors

        public Vector Tail { get; private set; }
        public Vector Head { get; private set; }
        public Vector Orientation { get { return (Head - Tail).Normalize(); } }
        public double Length { get{ return (Head - Tail).Magnitude; } }

        #endregion

        #region Interpolate

        public static Vector Interpolate(LineSegment l1, double control, bool allowExtrapolation)
        {
            return Vector.Interpolate(l1.Tail, l1.Head, control, allowExtrapolation);
        }

        public static Vector Interpolate(LineSegment l1, double control)
        {
            return Interpolate(l1, control, false);
        }

        public Vector Interpolate(double control)
        {
            return Interpolate(this, control);
        }

        public Vector Interpolate(double control, bool allowExtrapolation)
        {
            return Interpolate(this, control);
        }

        #endregion

        #region Rotation

        public static LineSegment Rotate(LineSegment l1, Angle angle, bool x, bool y, bool z)
        {
            if (x) l1 = RotateX(l1, angle);
            if (y) l1 = RotateY(l1, angle);
            if (z) l1 = RotateZ(l1, angle);
            return l1;
        }

        public LineSegment Rotate(Angle angle, bool x, bool y, bool z)
        {
            return Rotate(this, angle, x, y, z);
        }

        public static LineSegment RotateX(LineSegment l1, Angle angle)
        {
            return new LineSegment(l1.Tail, l1.Tail - (l1.Tail - l1.Head).RotateX(angle));
        }

        public LineSegment RotateX(Angle angle)
        {
            return RotateX(this, angle);
        }

        public static LineSegment RotateY(LineSegment l1, Angle angle)
        {
            return new LineSegment(l1.Tail, l1.Tail - (l1.Tail - l1.Head).RotateY(angle));
        }

        public LineSegment RotateY(Angle angle)
        {
            return RotateY(this, angle);
        }

        public static LineSegment RotateZ(LineSegment l1, Angle angle)
        {
            return new LineSegment(l1.Tail, l1.Tail - (l1.Tail - l1.Head).RotateZ(angle));
        }

        public LineSegment RotateZ(Angle angle)
        {
            return RotateZ(this, angle);
        }

        #endregion

        #region Axis based functions

        public static LineSegment Yaw(LineSegment l1, Angle angle, Axis axis)
        {
            return new LineSegment(l1.Tail, l1.Tail - (l1.Tail - l1.Head).Yaw(angle, axis));
        }

        public LineSegment Yaw(Angle angle, Axis axis)
        {
            return Yaw(this, angle, axis);
        }

        public static LineSegment Pitch(LineSegment l1, Angle angle, Axis axis)
        {
            return new LineSegment(l1.Tail, l1.Tail - (l1.Tail - l1.Head).Pitch(angle, axis));
        }

        public LineSegment Pitch(Angle angle, Axis axis)
        {
            return Pitch(this, angle, axis);
        }

        public static LineSegment Roll(LineSegment l1, Angle angle, Axis axis)
        {
            return new LineSegment(l1.Tail, l1.Tail - (l1.Tail - l1.Head).Roll(angle, axis));
        }

        public LineSegment Roll(Angle angle, Axis axis)
        {
            return Roll(this, angle, axis);
        }

        #endregion

        #region messages

        private const string SIX_COMPONENTS = "Array must contain exactly six components , (x1,y1,z1)(x2,y2,z2)";
        private const string THREE_TWO_DIMENSIONS = "Array must have dimensions of [3,2]";

        #endregion
    }
}