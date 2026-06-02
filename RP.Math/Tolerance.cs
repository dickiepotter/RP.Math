namespace RP.Math.Tolerance
{
    /// <summary>
    /// The Tolerance interface.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public interface ITolerance<T> where T : struct 
    {
        /// <summary>
        /// The is within.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        bool IsWithin(T value, T target);
    }

    namespace Double
    {
        using System;
        using System.Collections.Generic;

        public class UlpsTolerance : ITolerance<double>
        {
            public int Ulps { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public UlpsTolerance(int ulps)
            {
                this.Ulps = ulps;
            }

            public bool IsWithin(double value, double target)
            {
                // Compare the two doubles by how many representable values lie between them. Map each
                // bit pattern to a monotone ordering first (negative doubles count down from the sign
                // bit) so the subtraction is meaningful across zero.
                long a = BitConverter.DoubleToInt64Bits(value);
                if (a < 0) a = (long)(0x8000000000000000 - (ulong)a);

                long b = BitConverter.DoubleToInt64Bits(target);
                if (b < 0) b = (long)(0x8000000000000000 - (ulong)b);

                return Math.Abs(a - b) <= this.Ulps;
            }
        }

        public class PercentageTolerance : ITolerance<double>
        {
            public double Percentage { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public PercentageTolerance(double percentage)
            {
                this.Percentage = percentage;
            }

            public bool IsWithin(double value, double target)
            {
                // Percentage is expressed in percent (e.g. 5 == 5%), measured against the target's
                // magnitude. When the target is zero the band collapses to an exact match.
                return Math.Abs(value - target) <= Math.Abs(target) * (this.Percentage / 100.0);
            }
        }

        public class AbsoluteTolerance : ITolerance<double>
        {
            public double Absolute { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public AbsoluteTolerance(double absolute)
            {
                this.Absolute = absolute;
            }

            public bool IsWithin(double value, double target)
            {
                return Math.Abs(value - target) <= this.Absolute;
            }
        }

        public class RelativeTolerance : ITolerance<double>
        {
            public double Relative { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public RelativeTolerance(double relative)
            {
                this.Relative = relative;
            }

            public bool IsWithin(double value, double target)
            {
                // Relative error scaled by the larger magnitude of the pair, so the test is symmetric
                // in value/target. Exact equality (including two zeros) short-circuits to true.
                if (value == target) return true;

                double scale = Math.Max(Math.Abs(value), Math.Abs(target));
                return Math.Abs(value - target) <= scale * this.Relative;
            }
        }

        public class Range : ITolerance<double>
        {
            public double Min { get; private set; }

            public double Max { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public Range(double min, double max)
            {
                this.Min = Math.Min(min, max);
                this.Max = Math.Max(min, max);
            }

            public bool IsWithin(double value, double target)
            {
                // An (asymmetric) tolerance band offset from the target: [target + Min, target + Max].
                return value >= target + this.Min && value <= target + this.Max;
            }
        }

        public class AbsoluteAndUlpsTolerance : ITolerance<double>
        {
            public UlpsTolerance Ulps { get; private set; }

            public AbsoluteTolerance Absolute { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public AbsoluteAndUlpsTolerance(UlpsTolerance ulps, AbsoluteTolerance absolute)
            {
                this.Ulps = ulps;
                this.Absolute = absolute;
            }

            public bool IsWithin(double value, double target)
            {
                // The absolute band rescues the comparison near zero (where ULPs are meaningless),
                // otherwise the ULPs band applies — matching DoubleExtension.AlmostEqualsWithAbsOrUlps.
                return this.Absolute.IsWithin(value, target) || this.Ulps.IsWithin(value, target);
            }
        }

        public class AbsoluteAndRelativeTolerance : ITolerance<double>
        {
            public AbsoluteTolerance Absolute { get; private set; }

            public RelativeTolerance Relative { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public AbsoluteAndRelativeTolerance(AbsoluteTolerance absolute, RelativeTolerance relative)
            {
                this.Absolute = absolute;
                this.Relative = relative;
            }

            public bool IsWithin(double value, double target)
            {
                return this.Absolute.IsWithin(value, target) || this.Relative.IsWithin(value, target);
            }
        }

        public class TolerantEqualityComparer : IEqualityComparer<double>
        {
            public ITolerance<double> Tolerance { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public TolerantEqualityComparer(ITolerance<double> tolerance)
            {
                this.Tolerance = tolerance;
            }

            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <returns>
            /// true if the specified objects are equal; otherwise, false.
            /// </returns>
            /// <param name="x">The first <see cref="double"/> to compare.</param><param name="y">The second <see cref="double"/> to compare.</param>
            public bool Equals(double x, double y)
            {
                return this.Tolerance.IsWithin(x, y);
            }

            /// <summary>
            /// Returns a hash code for the specified object.
            /// </summary>
            /// <returns>
            /// A hash code for the specified object.
            /// </returns>
            /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param><exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
            public int GetHashCode(double obj)
            {
                // Tolerant equality is not transitive, so no hash can stay consistent with Equals over a
                // band of values. Returning a constant keeps the contract (equal items hash equal) by
                // routing every comparison through Equals — correct, at the cost of a single bucket.
                return 0;
            }
        }

        public class TolerantComparer : IComparer<double>
        {
            public ITolerance<double> Tolerance { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public TolerantComparer(ITolerance<double> tolerance)
            {
                this.Tolerance = tolerance;
            }

            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// </summary>
            /// <returns>
            /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
            /// </returns>
            /// <param name="x">The first object to compare.</param><param name="y">The second object to compare.</param>
            public int Compare(double x, double y)
            {
                // Values that fall within tolerance of each other are treated as equal; otherwise the
                // natural ordering applies. (Note: like any tolerant comparer this is not transitive,
                // so it must not be used to sort sequences where that property is relied upon.)
                if (this.Tolerance.IsWithin(x, y)) return 0;
                return x < y ? -1 : 1;
            }
        }
    }

}
