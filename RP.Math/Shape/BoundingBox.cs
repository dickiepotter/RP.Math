namespace RP.Math
{
    using System;
    using System.Globalization;

    using Math = System.Math;

    /// <summary>
    /// A <b>bounding box</b>: an upright (not tilted) rectangular block in 3D space — like a cardboard
    /// box, a brick, or a room — described by two opposite corners.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>What it is.</b> A bounding box is the 3D equivalent of a rectangle. It is "axis-aligned", which
    /// means its sides line up squarely with the X, Y and Z axes — it is never rotated at an angle. Because
    /// it cannot tilt, the whole box is fully described by just two points: <see cref="Min"/> (the corner
    /// with the smallest x, y and z) and <see cref="Max"/> (the corner with the largest x, y and z).
    /// Everything else — width, height, depth, centre — is worked out from those two corners.
    /// </para>
    /// <para>
    /// <b>What it is for.</b> Its main job is to <i>wrap around</i> something else: the smallest upright
    /// box that completely contains a more complicated shape (for example a <see cref="Sphere"/> or a
    /// <see cref="Circle"/>). This gives a very cheap way to ask rough questions — "is this point anywhere
    /// near the shape?" or "could these two shapes possibly be touching?" — using only simple
    /// less-than/greater-than comparisons, before doing any slower, exact maths. That is why every shape
    /// can report its <c>BoundingBox</c>.
    /// </para>
    /// <para>
    /// <b>Why upright (axis-aligned)?</b> Keeping the box un-tilted makes the maths simple enough to read:
    /// testing whether a point is inside is just "is its x between Min.X and Max.X, its y between Min.Y and
    /// Max.Y, and its z between Min.Z and Max.Z". No rotation or trigonometry is involved.
    /// </para>
    /// Follows the library's value-type design: an immutable struct with static and instance forms,
    /// tolerance-aware equality and formatting. The constructor sorts the supplied corners so
    /// <see cref="Min"/> is always component-wise less than or equal to <see cref="Max"/>.
    /// </remarks>
    /// <author>Richard Potter BSc(Hons)</author>
    [Serializable]
    public struct BoundingBox : IEquatable<BoundingBox>, IFormattable
    {
        #region Fields

        private readonly Vector min;
        private readonly Vector max;

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a bounding box from two opposite corners. The corners are sorted per-axis, so either
        /// order (or any diagonal pair) yields the same box.
        /// </summary>
        public BoundingBox(Vector corner1, Vector corner2)
        {
            this.min = new Vector(
                Math.Min(corner1.X, corner2.X),
                Math.Min(corner1.Y, corner2.Y),
                Math.Min(corner1.Z, corner2.Z));
            this.max = new Vector(
                Math.Max(corner1.X, corner2.X),
                Math.Max(corner1.Y, corner2.Y),
                Math.Max(corner1.Z, corner2.Z));
        }

        #endregion

        #region Constants

        /// <summary>The empty box at the origin (min == max == origin), with zero size.</summary>
        public static readonly BoundingBox Empty = new BoundingBox(new Vector(0, 0, 0), new Vector(0, 0, 0));

        #endregion

        #region Factories

        /// <summary>Construct a box from its centre and full size (width, height, depth) along each axis.</summary>
        /// <exception cref="ArgumentException">Thrown if any size component is negative.</exception>
        public static BoundingBox FromCenterSize(Vector center, Vector size)
        {
            if (size.X < 0 || size.Y < 0 || size.Z < 0)
            {
                throw new ArgumentException(NEGATIVE_SIZE, nameof(size));
            }

            Vector half = size / 2.0;
            return new BoundingBox(center - half, center + half);
        }

        /// <summary>Construct a box from its min and max corners (corners are sorted defensively).</summary>
        public static BoundingBox FromMinMax(Vector min, Vector max)
        {
            return new BoundingBox(min, max);
        }

        /// <summary>
        /// The smallest bounding box containing all of the supplied points. Throws if no points are given.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if <paramref name="points"/> is null or empty.</exception>
        public static BoundingBox FromPoints(params Vector[] points)
        {
            if (points == null || points.Length == 0)
            {
                throw new ArgumentException(NO_POINTS, nameof(points));
            }

            Vector lo = points[0];
            Vector hi = points[0];
            for (int i = 1; i < points.Length; i++)
            {
                lo = lo.ComponentMin(points[i]);
                hi = hi.ComponentMax(points[i]);
            }

            return new BoundingBox(lo, hi);
        }

        #endregion

        #region Accessors

        /// <summary>The minimum corner (component-wise least).</summary>
        public Vector Min { get { return this.min; } }

        /// <summary>The maximum corner (component-wise greatest).</summary>
        public Vector Max { get { return this.max; } }

        /// <summary>The centre of the box.</summary>
        public Vector Center { get { return (this.min + this.max) / 2.0; } }

        /// <summary>The full size (extent) of the box along each axis: max − min.</summary>
        public Vector Size { get { return this.max - this.min; } }

        /// <summary>Half the size of the box along each axis.</summary>
        public Vector Extents { get { return this.Size / 2.0; } }

        /// <summary>The size along the X axis.</summary>
        public double Width { get { return this.max.X - this.min.X; } }

        /// <summary>The size along the Y axis.</summary>
        public double Height { get { return this.max.Y - this.min.Y; } }

        /// <summary>The size along the Z axis.</summary>
        public double Depth { get { return this.max.Z - this.min.Z; } }

        /// <summary>The enclosed volume.</summary>
        public double Volume { get { return this.Width * this.Height * this.Depth; } }

        /// <summary>The total surface area of the six faces.</summary>
        public double SurfaceArea
        {
            get
            {
                double w = this.Width, h = this.Height, d = this.Depth;
                return 2.0 * ((w * h) + (h * d) + (w * d));
            }
        }

        /// <summary>The eight corner points of the box.</summary>
        public Vector[] Corners
        {
            get
            {
                return new[]
                {
                    new Vector(this.min.X, this.min.Y, this.min.Z),
                    new Vector(this.max.X, this.min.Y, this.min.Z),
                    new Vector(this.min.X, this.max.Y, this.min.Z),
                    new Vector(this.max.X, this.max.Y, this.min.Z),
                    new Vector(this.min.X, this.min.Y, this.max.Z),
                    new Vector(this.max.X, this.min.Y, this.max.Z),
                    new Vector(this.min.X, this.max.Y, this.max.Z),
                    new Vector(this.max.X, this.max.Y, this.max.Z),
                };
            }
        }

        #endregion

        #region Containment, intersection and queries

        /// <summary>Whether <paramref name="point"/> lies within the box (inclusive of the surface).</summary>
        public bool Contains(Vector point)
        {
            return point.X >= this.min.X && point.X <= this.max.X
                && point.Y >= this.min.Y && point.Y <= this.max.Y
                && point.Z >= this.min.Z && point.Z <= this.max.Z;
        }

        /// <summary>Whether <paramref name="point"/> lies within the box, expanded by <paramref name="tolerance"/> on every side.</summary>
        public bool Contains(Vector point, double tolerance)
        {
            return point.X >= this.min.X - tolerance && point.X <= this.max.X + tolerance
                && point.Y >= this.min.Y - tolerance && point.Y <= this.max.Y + tolerance
                && point.Z >= this.min.Z - tolerance && point.Z <= this.max.Z + tolerance;
        }

        /// <summary>Whether this box fully contains <paramref name="other"/>.</summary>
        public bool Contains(BoundingBox other)
        {
            return this.Contains(other.Min) && this.Contains(other.Max);
        }

        /// <summary>Whether this box overlaps <paramref name="other"/> (touching counts as intersecting).</summary>
        public bool Intersects(BoundingBox other)
        {
            return this.min.X <= other.Max.X && this.max.X >= other.Min.X
                && this.min.Y <= other.Max.Y && this.max.Y >= other.Min.Y
                && this.min.Z <= other.Max.Z && this.max.Z >= other.Min.Z;
        }

        /// <summary>
        /// The overlapping region of two boxes. Returns true and the intersection box when they overlap;
        /// false (with <paramref name="result"/> = <see cref="Empty"/>) when they are disjoint.
        /// </summary>
        public bool TryIntersect(BoundingBox other, out BoundingBox result)
        {
            if (!this.Intersects(other))
            {
                result = Empty;
                return false;
            }

            result = new BoundingBox(this.min.ComponentMax(other.Min), this.max.ComponentMin(other.Max));
            return true;
        }

        /// <summary>The smallest box containing both this box and <paramref name="other"/>.</summary>
        public BoundingBox Union(BoundingBox other)
        {
            return new BoundingBox(this.min.ComponentMin(other.Min), this.max.ComponentMax(other.Max));
        }

        /// <summary>The smallest box containing this box and <paramref name="point"/>.</summary>
        public BoundingBox Encapsulate(Vector point)
        {
            return new BoundingBox(this.min.ComponentMin(point), this.max.ComponentMax(point));
        }

        /// <summary>The point on or inside the box closest to <paramref name="point"/>.</summary>
        public Vector ClosestPoint(Vector point)
        {
            return point.Clamp(this.min, this.max);
        }

        /// <summary>The distance from <paramref name="point"/> to the box (zero if inside).</summary>
        public double DistanceTo(Vector point)
        {
            return this.ClosestPoint(point).Distance(point);
        }

        #endregion

        #region Transformation (returns a new box)

        /// <summary>A copy of the box translated by <paramref name="offset"/>.</summary>
        public BoundingBox Translate(Vector offset)
        {
            return new BoundingBox(this.min + offset, this.max + offset);
        }

        /// <summary>A copy of the box grown outward on every side by <paramref name="amount"/> (negative shrinks it).</summary>
        public BoundingBox Expand(double amount)
        {
            Vector a = new Vector(amount, amount, amount);
            return new BoundingBox(this.min - a, this.max + a);
        }

        #endregion

        #region Predicates

        /// <summary>Whether the box has zero (or negative-collapsed) size on any axis.</summary>
        public bool IsEmpty()
        {
            return this.Width <= 0 || this.Height <= 0 || this.Depth <= 0;
        }

        #endregion

        #region Operators

        /// <summary>Component-wise equality of the min and max corners.</summary>
        public static bool operator ==(BoundingBox b1, BoundingBox b2)
        {
            return b1.Min == b2.Min && b1.Max == b2.Max;
        }

        /// <summary>Component-wise inequality of the min and max corners.</summary>
        public static bool operator !=(BoundingBox b1, BoundingBox b2)
        {
            return !(b1 == b2);
        }

        #endregion

        #region Equality, deconstruction and formatting

        /// <summary>Equality with another object.</summary>
        public override bool Equals(object? other)
        {
            return other is BoundingBox b && this.Equals(b);
        }

        /// <summary>Equality with another box (by corners).</summary>
        public bool Equals(BoundingBox other)
        {
            return this == other;
        }

        /// <summary>Equality with another box within an absolute tolerance on the corner components.</summary>
        public bool Equals(BoundingBox other, double tolerance)
        {
            return this.min.Equals(other.Min, tolerance) && this.max.Equals(other.Max, tolerance);
        }

        /// <summary>A hash code derived from the corners.</summary>
        public override int GetHashCode()
        {
            return this.min.GetHashCode() ^ this.max.GetHashCode();
        }

        /// <summary>Deconstruct into the min and max corners, enabling <c>var (min, max) = box;</c>.</summary>
        public void Deconstruct(out Vector min, out Vector max)
        {
            min = this.min;
            max = this.max;
        }

        /// <summary>A string of the form <c>[min .. max]</c>.</summary>
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.CurrentCulture);
        }

        /// <summary>A formatted string of the form <c>[min .. max]</c> where the corner components use <paramref name="format"/>.</summary>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return string.Format(
                "[{0} .. {1}]",
                this.min.ToString(format, formatProvider),
                this.max.ToString(format, formatProvider));
        }

        #endregion

        #region Messages

        private const string NEGATIVE_SIZE = "Bounding box size cannot be negative on any axis.";
        private const string NO_POINTS = "At least one point is required to build a bounding box.";

        #endregion
    }
}
