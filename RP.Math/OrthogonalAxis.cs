using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPUtil.Math.Math3D
{
    public class OrthogonalAxis
    {
        private readonly Vector _up, _forward, _right;

        public Vector Up        { get { return _up; } }
        public Vector Forward   { get{ return _forward;} }
        public Vector Right     { get { return _right; } }

        public OrthogonalAxis()
        {
            _up = new Vector(0, 1, 0);
            _forward = new Vector(0, 0, -1);
            _right = new Vector(1, 0, 0);
        }

        public OrthogonalAxis(Vector right, Vector up, Vector forward)
        {
            if 
            (
                Vector.Angle(right, up) != Angle.Right_Angle ||
                Vector.Angle(up, forward) != Angle.Right_Angle ||
                Vector.Angle(right, forward) != Angle.Right_Angle
            )
                throw new ArgumentException("The vectors provided are not orthogonal.");

            Up = up;
            Forward = forward;
            Right = right;
        }

        public OrthogonalAxis(AxisAlignment x, AxisAlignment y, AxisAlignment z)
        {
            const Vector vx = new Vector(1, 0, 0);
            const Vector vMx = new Vector(-1, 0, 0);

            switch (x)
            {
                case AxisAlignment.FORWARD:
                    Forward = vx;
                    break;
                case AxisAlignment.BACKWARD:
                    Forward = vMx;
                    break;
                case AxisAlignment.UP:
                    Up = vx;
                    break;
                case AxisAlignment.DOWN:
                    Up = vMx;
                    break;
                case AxisAlignment.RIGHT:
                    Right = vx;
                    break;
                case AxisAlignment.LEFT:
                    Right = vMx;
                    break;
            }

            const Vector vy = new Vector(0, 1, 0);
            const Vector vMy = new Vector(0, -1, 0);

            switch (y)
            {
                case AxisAlignment.FORWARD:
                    Forward = vy;
                    break;
                case AxisAlignment.BACKWARD:
                    Forward = vMy;
                    break;
                case AxisAlignment.UP:
                    Up = vy;
                    break;
                case AxisAlignment.DOWN:
                    Up = vMy;
                    break;
                case AxisAlignment.RIGHT:
                    Right = vy;
                    break;
                case AxisAlignment.LEFT:
                    Right = vMy;
                    break;
            }

            const Vector vz = new Vector(0, 0, 1);
            const Vector vMz = new Vector(0, 0, -1);

            switch (z)
            {
                case AxisAlignment.FORWARD:
                    Forward = vz;
                    break;
                case AxisAlignment.BACKWARD:
                    Forward = vMz;
                    break;
                case AxisAlignment.UP:
                    Up = vz;
                    break;
                case AxisAlignment.DOWN:
                    Up = vMz;
                    break;
                case AxisAlignment.RIGHT:
                    Right = vz;
                    break;
                case AxisAlignment.LEFT:
                    Right = vMz;
                    break;
            }

            if
            (
                Vector.Angle(_right, _up) != Angle.Right_Angle ||
                Vector.Angle(_up, _forward) != Angle.Right_Angle ||
                Vector.Angle(_right, _forward) != Angle.Right_Angle
            )
                throw new ArgumentException("The vectors provided are not orthogonal.");
        }

        public OrthogonalAxis Yaw(Angle a1) { return Yaw(this, a1); }
        public OrthogonalAxis Pitch(Angle a1) { return Pitch(this, a1); }
        public OrthogonalAxis Roll(Angle a1) { return Yaw(Roll, a1); }
        public OrthogonalAxis Rotate(Attitude attitude) { return Rotate(this, attitude); }

        public static OrthogonalAxis Yaw(OrthogonalAxis axis, Angle a1) 
        {
            Vector vu = axis.Up.Yaw(al);
            Vector vf = axis.Forward.Yaw(al);
            Vector vr = axis.Right.Yaw(al);
            return new OrthogonalAxis(vr, vu, vf);
        }

        public static OrthogonalAxis Pitch(OrthogonalAxis axis, Angle a1) 
        {
            Vector vu = axis.Up.Yaw(al);
            Vector vf = axis.Forward.Yaw(al);
            Vector vr = axis.Right.Yaw(al);
            return new OrthogonalAxis(vr, vu, vf);
        }

        public static OrthogonalAxis Roll(OrthogonalAxis axis, Angle a1) 
        {
            Vector vu = axis.Up.Yaw(al);
            Vector vf = axis.Forward.Yaw(al);
            Vector vr = axis.Right.Yaw(al);
            return new OrthogonalAxis(vr, vu, vf);
        }

        public static OrthogonalAxis Rotate(OrthogonalAxis axis, Attitude attitude)
        {
            Vector vu = axis.Up.Rotate(attitude);
            Vector vf = axis.Forward.Rotate(attitude);
            Vector vr = axis.Right.Rotate(attitude);
            return new OrthogonalAxis(vr, vu, vf);
        }

        #region Static

        public static Vector LhsUp(Vector right, Vector forward) 
        {
            if( Vector.Angle(forward, right) != Angle.Right_Angle )
                throw new ArgumentException("The vectors provided are not orthogonal.");
            return right.CrossProduct(forward).Normalize();
        }

        public static Vector RhsUp(Vector right, Vector forward)
        {
            if (Vector.Angle(forward, right) != Angle.Right_Angle)
                throw new ArgumentException("The vectors provided are not orthogonal.");
            return forward.CrossProduct(right).Normalize();
        }

        public static Vector LhsRight(Vector up, Vector forward)
        {
            if (Vector.Angle(up, forward) != Angle.Right_Angle)
                throw new ArgumentException("The vectors provided are not orthogonal.");
            return forward.CrossProduct(up).Normalize();
        }

        public static Vector RhsRight(Vector up, Vector forward)
        {
            if (Vector.Angle(up, forward) != Angle.Right_Angle)
                throw new ArgumentException("The vectors provided are not orthogonal.");
            return up.CrossProduct(forward).Normalize();
        }

        public static Vector LhsForward( Vector right, Vector up)
        {
            if (Vector.Angle(up, right) != Angle.Right_Angle)
                throw new ArgumentException("The vectors provided are not orthogonal.");
            return up.CrossProduct(right).Normalize();
        }

        public static Vector RhsForward(Vector right, Vector up)
        {
            if (Vector.Angle(up, right) != Angle.Right_Angle)
                throw new ArgumentException("The vectors provided are not orthogonal.");
            return right.CrossProduct(up).Normalize();
        }

        #endregion

    }

    /// <summary>
    /// An enumeration of Cartesian axis identity alignments.
    /// </summary>
    /// <remarks>First bit is used as a negation flag e.g. 0010(NEAR) => 0011(FAR) </remarks>
    public enum AxisAlignment
    {
        FORWARD = 2,
        BACKWARD = 3,
        UP = 4,
        DOWN = 5,
        RIGHT = 6,
        LEFT = 7
    }
}
