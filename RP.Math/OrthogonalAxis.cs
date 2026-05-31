using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RP.Math
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

            _right = right;
            _up = up;
            _forward = forward;
        }

        public OrthogonalAxis(AxisAlignment x, AxisAlignment y, AxisAlignment z)
        {
            // The readonly fields must be definitely assigned on every path; the switches below set
            // exactly one of them per alignment. Seed to default so the compiler is satisfied, and
            // rely on the orthogonality check at the end to reject incomplete/duplicate alignments.
            _right = _up = _forward = default(Vector);

            var vx = new Vector(1, 0, 0);
            var vMx = new Vector(-1, 0, 0);

            switch (x)
            {
                case AxisAlignment.FORWARD:
                    _forward = vx;
                    break;
                case AxisAlignment.BACKWARD:
                    _forward = vMx;
                    break;
                case AxisAlignment.UP:
                    _up = vx;
                    break;
                case AxisAlignment.DOWN:
                    _up = vMx;
                    break;
                case AxisAlignment.RIGHT:
                    _right = vx;
                    break;
                case AxisAlignment.LEFT:
                    _right = vMx;
                    break;
            }

            var vy = new Vector(0, 1, 0);
            var vMy = new Vector(0, -1, 0);

            switch (y)
            {
                case AxisAlignment.FORWARD:
                    _forward = vy;
                    break;
                case AxisAlignment.BACKWARD:
                    _forward = vMy;
                    break;
                case AxisAlignment.UP:
                    _up = vy;
                    break;
                case AxisAlignment.DOWN:
                    _up = vMy;
                    break;
                case AxisAlignment.RIGHT:
                    _right = vy;
                    break;
                case AxisAlignment.LEFT:
                    _right = vMy;
                    break;
            }

            var vz = new Vector(0, 0, 1);
            var vMz = new Vector(0, 0, -1);

            switch (z)
            {
                case AxisAlignment.FORWARD:
                    _forward = vz;
                    break;
                case AxisAlignment.BACKWARD:
                    _forward = vMz;
                    break;
                case AxisAlignment.UP:
                    _up = vz;
                    break;
                case AxisAlignment.DOWN:
                    _up = vMz;
                    break;
                case AxisAlignment.RIGHT:
                    _right = vz;
                    break;
                case AxisAlignment.LEFT:
                    _right = vMz;
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
        public OrthogonalAxis Roll(Angle a1) { return Roll(this, a1); }
        public OrthogonalAxis Rotate(Attitude attitude) { return Rotate(this, attitude); }

        // NOTE: The four rotation helpers below were never finished in the original source and are
        // left as flagged gaps rather than guessed implementations:
        //   * Vector.Yaw/Pitch/Roll require an (Angle, Axis) pair, but an OrthogonalAxis carries no
        //     Axis convention to supply, so the intended per-basis rotation is undetermined.
        //   * Vector.Rotate takes a Rotation (X/Y/Z angles); these receive an Attitude (yaw/pitch/roll),
        //     and the Attitude->Rotation mapping depends on an Axis convention the type does not define.
        // They throw so the gap is explicit instead of silently incorrect.

        public static OrthogonalAxis Yaw(OrthogonalAxis axis, Angle a1)
        {
            throw new NotImplementedException(
                "OrthogonalAxis.Yaw was incomplete in the original source: Vector.Yaw needs an Axis argument that this type does not define.");
        }

        public static OrthogonalAxis Pitch(OrthogonalAxis axis, Angle a1)
        {
            throw new NotImplementedException(
                "OrthogonalAxis.Pitch was incomplete in the original source: Vector.Pitch needs an Axis argument that this type does not define.");
        }

        public static OrthogonalAxis Roll(OrthogonalAxis axis, Angle a1)
        {
            throw new NotImplementedException(
                "OrthogonalAxis.Roll was incomplete in the original source: Vector.Roll needs an Axis argument that this type does not define.");
        }

        public static OrthogonalAxis Rotate(OrthogonalAxis axis, Attitude attitude)
        {
            throw new NotImplementedException(
                "OrthogonalAxis.Rotate was incomplete in the original source: converting an Attitude (yaw/pitch/roll) to the Rotation (X/Y/Z) that Vector.Rotate expects requires an Axis convention this type does not define.");
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
        LEFT = 7,

        // Depth-axis aliases referenced by Axis.cs. Mapped per this enum's own comment
        // ("0010(NEAR) => 0011(FAR)"): NEAR == FORWARD (2), FAR == BACKWARD (3).
        // FLAG: under this mapping Axis.Map treats FAR as non-inverted and NEAR as inverted, the
        // reverse of the UP/RIGHT (non-inverted) vs DOWN/LEFT (inverted) pattern. The NEAR/FAR vs
        // FORWARD/BACKWARD naming was evidently mid-rename; confirm the intended convention.
        NEAR = FORWARD,
        FAR = BACKWARD
    }
}
