namespace RP.Math
{
    /// <summary>
    /// Common surface shared by all geometric shapes in 3D space, regardless of whether they are flat
    /// (planar) or enclose a volume (solid).
    /// </summary>
    /// <remarks>
    /// Shapes follow the library's value-type design: immutable structs, centre-anchored (the canonical
    /// origin is the geometric centre; for triangles/polygons the area centroid), with tolerance-aware
    /// containment. See <c>ROADMAP.md</c> for the layered architecture.
    /// </remarks>
    public interface IShape
    {
        /// <summary>The geometric centre (the shape's canonical origin) in world space.</summary>
        Vector Centroid { get; }

        /// <summary>The smallest <see cref="BoundingBox"/> that contains the shape.</summary>
        BoundingBox BoundingBox { get; }

        /// <summary>Whether <paramref name="point"/> lies on or within the shape.</summary>
        bool Contains(Vector point);

        /// <summary>Whether <paramref name="point"/> lies on or within the shape, within <paramref name="tolerance"/>.</summary>
        bool Contains(Vector point, double tolerance);
    }

    /// <summary>
    /// A flat shape occupying a plane in 3D space (for example a circle, rectangle or triangle).
    /// </summary>
    public interface IPlanarShape : IShape
    {
        /// <summary>The area enclosed by the shape's outline.</summary>
        double Area { get; }

        /// <summary>The length of the shape's outline.</summary>
        double Perimeter { get; }

        /// <summary>The unit normal of the plane the shape lies in.</summary>
        Vector Normal { get; }

        /// <summary>The supporting <see cref="Plane"/> the shape lies in.</summary>
        Plane Plane { get; }
    }

    /// <summary>
    /// A solid shape enclosing a volume in 3D space (for example a sphere or box).
    /// </summary>
    public interface ISolidShape : IShape
    {
        /// <summary>The enclosed volume.</summary>
        double Volume { get; }

        /// <summary>The total area of the shape's boundary surface.</summary>
        double SurfaceArea { get; }
    }
}
