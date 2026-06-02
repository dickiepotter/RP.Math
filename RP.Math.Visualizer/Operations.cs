namespace VectorVisualizer;

using RP.Math;

public enum ResultKind { Vector, Scalar, Boolean, Integer }

/// <summary>The inputs the user can adjust, beyond vector A.</summary>
public enum Input { B, C, Scalar, Angle, Control, Digits, Tolerance }

/// <summary>The outcome of running an operation, tagged with how to present it.</summary>
public sealed class OpResult
{
    public ResultKind Kind { get; private init; }
    public Vector Vector { get; private init; }
    public double Scalar { get; private init; }
    public bool Boolean { get; private init; }
    public int Integer { get; private init; }

    public static OpResult Vec(Vector v) => new() { Kind = ResultKind.Vector, Vector = v };
    public static OpResult Num(double d) => new() { Kind = ResultKind.Scalar, Scalar = d };
    public static OpResult Bool(bool b) => new() { Kind = ResultKind.Boolean, Boolean = b };
    public static OpResult Int(int i) => new() { Kind = ResultKind.Integer, Integer = i };
}

/// <summary>All the values an operation can draw on.</summary>
public sealed class OpContext
{
    public Vector A { get; set; }
    public Vector B { get; set; }
    public Vector C { get; set; }
    public double Scalar { get; set; } = 2;
    public double AngleDeg { get; set; } = 45;
    public double Control { get; set; } = 0.5;
    public int Digits { get; set; } = 2;
    public double Tolerance { get; set; } = 1e-6;

    public double AngleRad => AngleDeg * Math.PI / 180.0;
}

public sealed class OperationDef
{
    public required string Key { get; init; }
    public required string Name { get; init; }
    public required string Category { get; init; }
    public required string Formula { get; init; }
    public required string Description { get; init; }
    public required Func<OpContext, OpResult> Run { get; init; }
    public HashSet<Input> Inputs { get; init; } = new();

    public bool Uses(Input i) => Inputs.Contains(i);
}

public static class Operations
{
    public static readonly IReadOnlyList<OperationDef> All = Build();

    private static List<OperationDef> Build()
    {
        var ops = new List<OperationDef>();

        void Add(string key, string name, string cat, string formula, string desc,
                 Func<OpContext, OpResult> run, params Input[] inputs)
            => ops.Add(new OperationDef
            {
                Key = key, Name = name, Category = cat, Formula = formula,
                Description = desc, Run = run, Inputs = new HashSet<Input>(inputs),
            });

        // ---- Arithmetic ----
        Add("add", "Add (A + B)", "Arithmetic", "A + B",
            "Adds the vectors component-wise. Geometrically, place B's tail at A's tip; the sum reaches the new tip.",
            c => OpResult.Vec(c.A + c.B), Input.B);
        Add("sub", "Subtract (A − B)", "Arithmetic", "A − B",
            "Subtracts component-wise. A − B is the vector pointing from B's tip to A's tip.",
            c => OpResult.Vec(c.A - c.B), Input.B);
        Add("neg", "Negate (−A)", "Arithmetic", "−A",
            "Flips the vector to point the opposite way, same length.",
            c => OpResult.Vec(-c.A));
        Add("mul", "Multiply by scalar (A · s)", "Arithmetic", "A · s",
            "Scales every component by s. |s| > 1 lengthens, < 1 shortens, negative flips direction.",
            c => OpResult.Vec(c.A * c.Scalar), Input.Scalar);
        Add("div", "Divide by scalar (A ÷ s)", "Arithmetic", "A ÷ s",
            "Divides every component by s.",
            c => OpResult.Vec(c.A / c.Scalar), Input.Scalar);
        Add("scaleTo", "Scale to magnitude", "Arithmetic", "Scale(A, m)",
            "Returns a vector in A's direction but with magnitude m (A normalised, then × m).",
            c => OpResult.Vec(c.A.Scale(c.Scalar)), Input.Scalar);

        // ---- Products ----
        Add("dot", "Dot product (A · B)", "Products", "A · B = |A||B|cosθ",
            "A scalar measuring how much the vectors point the same way. Zero ⇒ perpendicular; positive ⇒ acute angle.",
            c => OpResult.Num(c.A.DotProduct(c.B)), Input.B);
        Add("cross", "Cross product (A × B)", "Products", "A × B",
            "A vector perpendicular to both A and B; its length equals the area of the parallelogram they span.",
            c => OpResult.Vec(c.A.CrossProduct(c.B)), Input.B);
        Add("mixed", "Triple product A · (B × C)", "Products", "A · (B × C)",
            "The signed volume of the parallelepiped formed by A, B and C.",
            c => OpResult.Num(c.A.MixedProduct(c.B, c.C)), Input.B, Input.C);

        // ---- Geometry ----
        Add("magnitude", "Magnitude |A|", "Geometry", "|A| = √(x² + y² + z²)",
            "The length of the vector.",
            c => OpResult.Num(c.A.Magnitude));
        Add("normalize", "Normalize (Â)", "Geometry", "Â = A / |A|",
            "Returns the unit vector in A's direction (length 1). Throws for a zero vector.",
            c => OpResult.Vec(c.A.Normalize()));
        Add("distance", "Distance(A, B)", "Geometry", "|A − B|",
            "The straight-line distance between the two tips.",
            c => OpResult.Num(c.A.Distance(c.B)), Input.B);
        Add("angle", "Angle(A, B)", "Geometry", "arccos( (A·B) / (|A||B|) )",
            "The angle between the vectors, in radians (degrees shown alongside).",
            c => OpResult.Num(c.A.Angle(c.B)), Input.B);
        Add("projection", "Projection onto B", "Geometry", "(A·B / |B|²) B",
            "The component of A that lies along B — A's 'shadow' on the line of B.",
            c => OpResult.Vec(c.A.Projection(c.B)), Input.B);
        Add("rejection", "Rejection from B", "Geometry", "A − proj_B(A)",
            "The component of A perpendicular to B (A minus its projection onto B).",
            c => OpResult.Vec(c.A.Rejection(c.B)), Input.B);
        Add("reflection", "Reflection about B", "Geometry", "reflect A across B",
            "Reflects A in the plane/line defined by B (like a bounce).",
            c => OpResult.Vec(c.A.Reflection(c.B)), Input.B);
        Add("interpolate", "Interpolate(A, B, t)", "Geometry", "A(1 − t) + B·t",
            "Linear blend between A (t = 0) and B (t = 1). Values outside 0–1 extrapolate along the line.",
            c => OpResult.Vec(Vector.Interpolate(c.A, c.B, c.Control, true)), Input.B, Input.Control);
        Add("min", "Min(A, B)", "Geometry", "smaller of |A|, |B|",
            "Returns whichever of A or B has the smaller magnitude (length) — not a component-wise minimum.",
            c => OpResult.Vec(c.A.Min(c.B)), Input.B);
        Add("max", "Max(A, B)", "Geometry", "greater of |A|, |B|",
            "Returns whichever of A or B has the greater magnitude (length) — not a component-wise maximum.",
            c => OpResult.Vec(c.A.Max(c.B)), Input.B);
        Add("reflectNormal", "Reflect A about normal B", "Geometry", "A − 2(A·n̂)n̂",
            "Reflects A about the surface whose normal is B (a bounce). B is normalised internally.",
            c => OpResult.Vec(c.A.Reflect(c.B)), Input.B);
        Add("slerp", "Slerp(A, B, t)", "Geometry", "spherical interpolation",
            "Interpolates direction along the great-circle arc between A and B (magnitude blended linearly).",
            c => OpResult.Vec(c.A.Slerp(c.B, c.Control)), Input.B, Input.Control);
        Add("moveTowards", "MoveTowards(A → B, step)", "Geometry", "step from A towards B",
            "Moves A towards target B by at most 'step', stopping exactly on B.",
            c => OpResult.Vec(c.A.MoveTowards(c.B, c.Scalar)), Input.B, Input.Scalar);
        Add("distanceSq", "DistanceSquared(A, B)", "Geometry", "|A − B|²",
            "The squared distance between the tips — cheaper than Distance (no square root).",
            c => OpResult.Num(c.A.DistanceSquared(c.B)), Input.B);
        Add("componentMin", "Component min(A, B)", "Geometry", "(min xs, min ys, min zs)",
            "A true component-wise minimum (each axis independently), unlike magnitude-based Min.",
            c => OpResult.Vec(c.A.ComponentMin(c.B)), Input.B);
        Add("componentMax", "Component max(A, B)", "Geometry", "(max xs, max ys, max zs)",
            "A true component-wise maximum (each axis independently), unlike magnitude-based Max.",
            c => OpResult.Vec(c.A.ComponentMax(c.B)), Input.B);
        Add("clamp", "Clamp(A, min B, max C)", "Geometry", "clamp each component",
            "Clamps each component of A between the matching components of B (min) and C (max).",
            c => OpResult.Vec(c.A.Clamp(c.B, c.C)), Input.B, Input.C);
        Add("clampMag", "ClampMagnitude(A, max)", "Geometry", "cap |A| at max",
            "Caps A's length at 'max', keeping its direction.",
            c => OpResult.Vec(c.A.ClampMagnitude(c.Scalar)), Input.Scalar);

        // ---- Rotation (degrees in, applied as radians) ----
        Add("rotateX", "Rotate about X", "Rotation", "Rx(θ) · A",
            "Rotates A around the X axis by θ.",
            c => OpResult.Vec(c.A.RotateX(c.AngleRad)), Input.Angle);
        Add("rotateY", "Rotate about Y", "Rotation", "Ry(θ) · A",
            "Rotates A around the Y axis by θ.",
            c => OpResult.Vec(c.A.RotateY(c.AngleRad)), Input.Angle);
        Add("rotateZ", "Rotate about Z", "Rotation", "Rz(θ) · A",
            "Rotates A around the Z axis by θ.",
            c => OpResult.Vec(c.A.RotateZ(c.AngleRad)), Input.Angle);
        Add("yaw", "Yaw", "Rotation", "yaw θ (about Up)",
            "Aviation-style yaw: rotation about the convention's Up (DirectX: +Y).",
            c => OpResult.Vec(c.A.Yaw(new Angle(c.AngleRad), OrthogonalAxes.DirectX)), Input.Angle);
        Add("pitch", "Pitch", "Rotation", "pitch θ (about Right)",
            "Aviation-style pitch: rotation about the convention's Right (DirectX: +X).",
            c => OpResult.Vec(c.A.Pitch(new Angle(c.AngleRad), OrthogonalAxes.DirectX)), Input.Angle);
        Add("roll", "Roll", "Rotation", "roll θ (about Forward)",
            "Aviation-style roll: rotation about the convention's Forward (DirectX: +Z).",
            c => OpResult.Vec(c.A.Roll(new Angle(c.AngleRad), OrthogonalAxes.DirectX)), Input.Angle);

        // ---- Components ----
        Add("abs", "Abs()", "Components", "|A|",
            "Absolute value of the vector (its magnitude).",
            c => OpResult.Num(c.A.Abs()));
        Add("sumComponents", "Sum of components", "Components", "x + y + z",
            "Adds the three components together.",
            c => OpResult.Num(c.A.SumComponents()));
        Add("sumComponentSqrs", "Sum of squares", "Components", "x² + y² + z²",
            "Adds the squares of the components (magnitude squared).",
            c => OpResult.Num(c.A.SumComponentSqrs()));
        Add("magnitudeSq", "Magnitude squared", "Components", "|A|²",
            "The squared magnitude — cheaper than Magnitude when only comparing lengths.",
            c => OpResult.Num(c.A.MagnitudeSquared));
        Add("sqrtComponents", "Sqrt of components", "Components", "(√x, √y, √z)",
            "Square root of each component.",
            c => OpResult.Vec(c.A.SqrtComponents()));
        Add("sqrComponents", "Square of components", "Components", "(x², y², z²)",
            "Squares each component.",
            c => OpResult.Vec(c.A.SqrComponents()));
        Add("powComponents", "Power of components", "Components", "(xⁿ, yⁿ, zⁿ)",
            "Raises each component to the power n.",
            c => OpResult.Vec(c.A.PowComponents(c.Scalar)), Input.Scalar);
        Add("round", "Round(digits)", "Components", "round each to d digits",
            "Rounds each component to d decimal places.",
            c => OpResult.Vec(c.A.Round(c.Digits)), Input.Digits);

        // ---- Predicates ----
        Add("isUnit", "Is unit vector?", "Predicates", "|A| ≈ 1",
            "True when A's length is 1 (within tolerance).",
            c => OpResult.Bool(c.A.IsUnitVector(c.Tolerance)), Input.Tolerance);
        Add("isNaN", "Is NaN?", "Predicates", "any component NaN",
            "True when any component is Not-a-Number.",
            c => OpResult.Bool(c.A.IsNaN()));
        Add("isZero", "Is zero?", "Predicates", "|A| ≈ 0",
            "True when the vector's magnitude is within tolerance of zero.",
            c => OpResult.Bool(c.A.IsZero(c.Tolerance)), Input.Tolerance);
        Add("isPerp", "Is perpendicular to B?", "Predicates", "A · B ≈ 0",
            "True when A and B meet at a right angle (within tolerance).",
            c => OpResult.Bool(c.A.IsPerpendicular(c.B, c.Tolerance)), Input.B, Input.Tolerance);
        Add("isBackFace", "Is back face? (A normal, B sight)", "Predicates", "A · B < 0",
            "Treating A as a surface normal and B as the line of sight, true when they point against each other (dot < 0).",
            c => OpResult.Bool(c.A.IsBackFace(c.B)), Input.B);

        // ---- Comparison ----
        Add("equals", "Equals(A, B)?", "Comparison", "A == B",
            "True when the vectors are equal (within tolerance).",
            c => OpResult.Bool(c.A.Equals(c.B, c.Tolerance)), Input.B, Input.Tolerance);
        Add("compareTo", "CompareTo(A, B)", "Comparison", "sign(|A| − |B|)",
            "Compares the vectors by magnitude: −1, 0 or 1.",
            c => OpResult.Int(c.A.CompareTo(c.B)), Input.B);

        return ops;
    }
}
