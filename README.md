# RP.Math — the mathematics of computer science, explained

> A double-precision 3D maths library for C#, written to be **read and understood** before it is
> used. Every type is a small, immutable value object built on Cartesian coordinates and Euclidean
> geometry, and every operation is explained with the maths behind it. The code favours clarity over
> raw speed so the *why* stays easy to follow.

*This library grew out of [A Vector Type for C#](https://www.codeproject.com/Articles/17425/A-Vector-Type-for-C),
originally published by Richard Potter on CodeProject under the
[Code Project Open License (CPOL)](https://www.codeproject.com/info/cpol10.aspx). That article walked
through a single `Vector3` type from first principles; this project keeps that teaching spirit and
extends it to a whole family of related types — lines, rays, planes, angles, rotations, quaternions and
more.*

---

## Contents

- [What this project is for](#what-this-project-is-for)
- [Conventions used across the library](#conventions-used-across-the-library)
- [The types at a glance](#the-types-at-a-glance)
- [`Vector` — the foundation](#vector--the-foundation)
- [The line family: `Line`, `Ray`, `LineSegment`, `Chord`](#the-line-family-line-ray-linesegment-chord)
- [`Plane`](#plane)
- [`Angle`](#angle)
- [Orientation: `Rotation`, `Attitude`, `Quaternion`](#orientation-rotation-attitude-quaternion)
- [`Pose`](#pose)
- [`Matrix`](#matrix)
- [`OrthogonalAxes` — coordinate conventions](#orthogonalaxes)
- [Shapes: conceptual and placed](#shapes-conceptual-and-placed)
- [Supporting numeric helpers](#supporting-numeric-helpers)
- [Points of interest](#points-of-interest)
- [Status and history](#status-and-history)

---

## What this project is for

For years I have seen people struggle with the mathematics that underpins computer science —
vectors, angles, rotations, the geometry of lines and planes. This library is, first and foremost, a
**learning resource**. The maths is the point; the code is the explanation. The resulting types are
not designed to be the fastest possible (matrix mathematics and SIMD are more efficient but much
harder to follow) — they are designed to be **as simple and understandable as possible**.

A few principles run through everything:

- **Maths first, not graphics.** The types model mathematical ideas — magnitude, projection, central
  angle, signed distance, gimbal lock — rather than rendering primitives. Graphics happens to overlap
  with a lot of this maths, but a graphics/game engine is explicitly *not* what this is.
- **No unexplained jargon.** Wherever a term, symbol or acronym appears, it is introduced in plain
  language first. You will need only a basic grasp of trigonometry and algebra; the formal names
  (Cartesian, Euclidean, and so on) are just the school-level maths dressed up.
- **Verbose on purpose.** The documentation goes into far more detail than a working programmer
  strictly needs. If any part seems patronising, please do not be offended — it is written for a wide
  audience, including people meeting these ideas for the first time.

A quick glossary you will see used throughout:

- **Operator** — the symbol that defines an operation, such as plus (`+`) in `a + b`.
- **Operand** — the values an operator works on. In `a + b`, `a` is the left-hand-side (LHS) operand
  and `b` is the right-hand-side (RHS) operand.
- **Scalar** — an ordinary single number (a `double`), as opposed to a vector or other structured
  value.

---

## Conventions used across the library

These hold for (almost) every type, so they are stated once here rather than repeated everywhere.

**Three dimensions, Cartesian coordinates.** Everything lives in 3D space described by three
perpendicular axes — x, y and z. Orientation of the axes does not affect the maths, but the
illustrations assume the common graphics convention (as in OpenGL), where z is negative as you look
down the axis:

![The assumed orientation of the x, y and z axes](docs/images/axisOrient.gif)

**Immutable value objects.** Types such as `Vector`, `Angle`, `Plane`, `Quaternion`, `Rotation`,
`Pose` and `Matrix` are immutable: an operation never changes the object it is called on, it returns a
*new* value. This makes them behave like primitive numbers — safe to share, easy to reason about.

**Static and instance forms of each operation.** Most operations exist twice, so you can write
whichever reads better:

```csharp
double d1 = Vector.Distance(a, b);   // static form
double d2 = a.Distance(b);           // instance form — calls the static one
```

The instance form almost always just calls the static form, so the two can never disagree.

**Angles are radians, wrapped in `Angle`.** Angles are stored as radians but carry a dedicated
[`Angle`](#angle) type so you never have to remember which unit a `double` is in. `Angle` converts
to and from degrees, radians and gradians, and a `double` converts implicitly to an `Angle` (treated
as radians).

**Tolerance-aware comparisons.** Floating-point numbers rarely land *exactly* on a computed value, so
many comparisons accept a tolerance (`Contains(point, tolerance)`, `Equals(other, tolerance)`,
`IsParallelTo(other, tolerance)`). Where it makes sense there is also a no-tolerance overload that uses
a sensible default.

**Safe variants: `…OrDefault`.** Normalizing a zero-length vector or quaternion is undefined. The
strict methods (`Normalize`) throw; the safe ones (`NormalizeOrDefault`) return the zero value instead.
Constructors that need a direction use the safe form, so a degenerate input produces a degenerate (but
not exploding) result.

---

## The types at a glance

| Type | What it models | How it differs from its neighbours |
|------|----------------|-------------------------------------|
| **`Vector`** | A point or direction in 3D space (x, y, z). | The foundation; almost everything else is built from it. |
| **`Line`** | An infinite straight line. | Infinite in **both** directions — no ends, no length. |
| **`Ray`** | A half-line: a start point and one direction. | Infinite in **one** direction — has a start but no end. |
| **`LineSegment`** | A finite straight line between two points. | **Finite** — two ends, so it has a length and a midpoint. |
| **`Chord`** | A line segment whose ends lie on a circle. | A `LineSegment` that *also* knows its circle's radius, unlocking arc/angle maths. |
| **`Plane`** | An infinite flat surface (`Ax + By + Cz + D = 0`). | A 2D surface in 3D space, with a front and a back. |
| **`Angle`** | An amount of turn. | A 1D quantity with unit conversion and classification (acute, reflex, …). |
| **`Rotation`** | An orientation as Euler angles about X, Y, Z. | Human-friendly to read/write, but prone to gimbal lock. |
| **`Attitude`** | An orientation as yaw / pitch / roll. | The same idea as `Rotation` in aviation naming. |
| **`Quaternion`** | An orientation as a 4-component number. | The robust form: no gimbal lock, smooth interpolation. |
| **`Pose`** | A position **and** an orientation together. | A rigid placement ("where" + "which way") in friendly form. |
| **`Matrix`** | A 4×4 transformation matrix. | The general linear-algebra workhorse for transforms. |
| **`OrthogonalAxes`** | A coordinate-system convention (which way is up, right, far, and its handedness). | A constant label chosen per project — not a position or an orientation. |
| **`Circle`**, **`Triangle`**, **`Rectangle`**, **`Ellipse`**, **`Annulus`**, **`Sector`** | *Conceptual* flat shapes — size and proportion only, no position. | Pure intrinsic geometry: area, perimeter, angles; comparable by size. |
| **`Sphere`**, **`Cuboid`**, **`Cylinder`**, **`Cone`**, **`Capsule`**, **`Ellipsoid`**, **`Torus`** | *Conceptual* solids — size and proportion only, no position. | Pure intrinsic geometry: volume, surface area; comparable by size. |
| **`Placed…`** (one per conceptual shape) | A conceptual shape **placed** in space by a `Pose`. | Add world-space maths: containment, closest point, line/ray hits. |
| **`PlacedPolygon`**, **`PlacedTetrahedron`** | Vertex-defined shapes (a planar polygon; a solid with four corners). | Always placed — defined by their corners, with no separate conceptual form. |

The shape types come in two layers — a [conceptual shape and its placed partner](#shapes-conceptual-and-placed) — so that size-only maths never has to see a position.

The four members of the **line family** are best understood together — they differ only in *where
they are allowed to stop*:

```
 Line          <──────────●──────────>      infinite both ways · no ends · no length
 Ray                      ●──────────>      starts here · infinite one way · one end
 LineSegment              ●──────────●      finite · two ends · has a length & midpoint
 Chord                    ●─────────╴●╶     a segment whose two ends sit on a circle
                          (              )      ╲___ the circle it belongs to
```

That single difference drives their "closest point" behaviour: a line never clamps (any point on it is
allowed), a ray clamps behind its start, and a segment clamps at both ends.

The three **orientation** types (`Rotation`, `Attitude`, `Quaternion`) all describe the same thing —
which way something is facing — at different trade-offs of readability versus robustness. They convert
freely between one another, and `Pose` bundles an orientation together with a position.

---

## `Vector` — the foundation

A vector represents values along a number of axes. For this library that is three: three numbers and
three axes. `Vector` is the type everything else leans on, so it gets the fullest treatment.

```csharp
public struct Vector
{
   private double x, y, z;
}
```

**Why a struct rather than a class?** A struct is a value type: it is copied by value, created and
disposed of cheaply, and behaves like a primitive — which is exactly how a vector should feel. (There
is no reason it *could* not be a class; this is a design choice.)

Unless stated otherwise a vector is treated as **positional**, originating at `(0, 0, 0)`. The
alternatives are *unit vectors* (length 1, interpreted as pure direction) and *vector pairs* (where one
vector is the origin and another is the displacement from it).

Throughout, the worked equations assume two vectors broken into components:

$A = \begin{pmatrix} a \\ b \\ c \end{pmatrix}\qquad B = \begin{pmatrix} d \\ e \\ f \end{pmatrix}$

### Accessing the components

The components are exposed as `X`, `Y`, `Z` properties, as an `Array` of three doubles, and through an
indexer (`v[0]`, `v[1]`, `v[2]`). Constructors accept three doubles, an array, or another vector:

```csharp
var v = new Vector(1, 2, 3);
var w = new Vector(new double[] { 1, 2, 3 });
var u = new Vector(v);            // copy
double y = v[1];                  // 2
```

### The basic operators

**Addition** (`v3 = v1 + v2`) — add the components pairwise (x+x, y+y, z+z).

```csharp
public static Vector operator +(Vector v1, Vector v2)
{
   return new Vector(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
}
```

**Subtraction** (`v3 = v1 - v2`) — subtract the components pairwise.

**Negation** (`v2 = -v1`) — invert direction by negating each component. **Reinforcement** (`+v1`)
returns the vector unchanged (it exists only to mirror negation).

**Equality** (`==`, `!=`) — two vectors are equal when all three component pairs are equal. (`Equals`
also offers a tolerance-aware overload, which is what you usually want with floating point.)

**Multiplication and division by a scalar** — scale each component by the number.

```csharp
public static Vector operator *(Vector v1, double s2)
{
   return new Vector(v1.X * s2, v1.Y * s2, v1.Z * s2);
}
```

The order of operands can be reversed (`s1 * v2` does the same thing). Division by a scalar divides
each component.

**Comparison** (`<`, `>`, `<=`, `>=`) — vectors are compared by **magnitude** (length), irrespective of
direction.

### Magnitude

The length, or **magnitude**, of a vector is ![the square root of the sum of the squared
components](docs/images/image003.gif):

```csharp
public static double Magnitude(Vector v1)
{
   return Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y + v1.Z * v1.Z);
}
```

In the current library this is exposed as a read-only **property**, `v.Magnitude`. To *resize* a vector
to a new length while keeping its direction, use `Scale`:

```csharp
double len = v.Magnitude;             // length of v
Vector resized = v.Scale(2.5);        // same direction, length 2.5
```

When you only need to **compare** lengths, skip the square root with `MagnitudeSquared` (and
`DistanceSquared` for distances) — squaring preserves ordering and is cheaper.

### The two kinds of multiplication: cross and dot

Multiplying vectors is trickier than scaling. There are two products, and neither is overloaded onto an
operator (to avoid confusion) — you call them by name.

The **cross product** of two vectors produces a vector that is normal (perpendicular) to the plane the
two vectors span:

![The cross product is perpendicular to both inputs](docs/images/image004.gif)

The formula (with v1 = A, v2 = B) expands from ![|A||B|sinθ
n̂](docs/images/image005.gif) — the sine of the angle accounts for direction, θ being the smallest
angle between A and B (![0 ≤ θ ≤ π](docs/images/image008.gif)):

![cross product component x](docs/images/image006.gif)
![cross product component y](docs/images/image007.gif)

In matrix-style notation:

![cross product in determinant form](docs/images/image009.gif)

Cross product is **non-commutative**: `v1 × v2` is not the same as `v2 × v1` (they point opposite ways).

```csharp
public static Vector CrossProduct(Vector v1, Vector v2)
{
   return new Vector(
      v1.Y * v2.Z - v1.Z * v2.Y,
      v1.Z * v2.X - v1.X * v2.Z,
      v1.X * v2.Y - v1.Y * v2.X);
}
```

A handy template for working a cross product by hand:

![mnemonic for the cross product](docs/images/image010.gif)

The **dot product** of two vectors is a single scalar, defined by ![|A||B|cosθ](docs/images/image005.gif)
and computed as ![the sum of the products of the components](docs/images/image011.gif):

![dot product expanded](docs/images/image012.gif)

```csharp
public static double DotProduct(Vector v1, Vector v2)
{
   return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
}
```

Cosine of the angle is what makes the dot product so useful: it is zero exactly when the vectors are
perpendicular, positive when they point broadly the same way, and negative when they oppose. (The
**mixed**, or scalar triple, product `MixedProduct(v1, v2, v3)` combines both — it is `v1 · (v2 × v3)`
and gives the signed volume of the parallelepiped the three vectors span.)

### Normalisation and unit vectors

A **unit vector** has a magnitude of 1 — a pure direction with the size removed. **Normalisation**
converts any (non-zero) vector to a unit vector by dividing by its magnitude:

![normalize by dividing each component by the magnitude](docs/images/image013.gif)

```csharp
Vector dir = v.Normalize();           // throws if v has zero length
Vector safe = v.NormalizeOrDefault();  // returns (0,0,0) instead of throwing
```

`IsUnitVector()` tests for length 1 (with a tolerance overload, since exact equality is unreliable for
doubles).

### Interpolation

`Interpolate` takes a point a fraction of the way between two vectors. A `control` of 0 returns the
first vector, 1 returns the second, and 0.5 the midpoint:

![linear interpolation between two vectors](docs/images/image014.gif)

$n = n_1(1 - t) + n_2\,t$

```csharp
Vector mid = a.Interpolate(b, 0.5);
```

By default `control` is restricted to ![0 ≤ t ≤ 1](docs/images/image023.gif); an overload allows
extrapolation beyond the endpoints.

### Distance

The distance between two positional vectors is Pythagoras' theorem in 3D:

![distance is the root of the summed squared differences](docs/images/image026.gif)
![distance expanded](docs/images/image027.gif)

```csharp
double d = a.Distance(b);
```

### Angle between two vectors

The angle between two vectors comes from normalising both and taking the arccosine of their dot
product:

![angle from normalized dot product](docs/images/image028.gif)
![angle derivation](docs/images/image030.gif)
![angle result](docs/images/image031.gif)

```csharp
double radians = a.Angle(b);
```

### Rotation: yaw, pitch and roll

`Yaw`, `Pitch` and `Roll` rotate a vector about the Y, X and Z axes respectively (Euler rotations).

**Yaw** — rotation about the **Y** axis:

![yaw setup](docs/images/image032.gif)
![yaw result x](docs/images/image043.gif)
![yaw result z](docs/images/image044.gif)

```csharp
public static Vector Yaw(Vector v1, double rad)
{
   double x = (v1.Z * Math.Sin(rad)) + (v1.X * Math.Cos(rad));
   double y = v1.Y;
   double z = (v1.Z * Math.Cos(rad)) - (v1.X * Math.Sin(rad));
   return new Vector(x, y, z);
}
```

**Pitch** — rotation about the **X** axis:

![pitch setup](docs/images/image045.gif)
![pitch result y](docs/images/image056.gif)
![pitch result z](docs/images/image057.gif)

**Roll** — rotation about the **Z** axis:

![roll setup](docs/images/image058.gif)
![roll result x](docs/images/image068.gif)
![roll result y](docs/images/image069.gif)

The library also provides axis-named `RotateX` / `RotateY` / `RotateZ` (with optional offset pivots),
and `Angle`-typed overloads so you can rotate by an `Angle` rather than a bare radian `double`. For
robust, composable orientation prefer [`Quaternion`](#orientation-rotation-attitude-quaternion).

### Projection, rejection and reflection

- **`Projection(direction)`** — the "shadow" of a vector cast along another vector's line: the part of
  it that lies *along* `direction`.
- **`Rejection(direction)`** — what is left over: the part *perpendicular* to `direction` (so that
  projection + rejection reconstructs the original).
- **`Reflection(reflector)`** — mirror the vector about the **line** of another vector.
- **`Reflect(normal)`** — mirror the vector about the **surface** described by a normal: the classic
  "bounce", where the angle of incidence equals the angle of reflection. *(Note the distinction:
  `Reflection` mirrors about a line, `Reflect` about a surface.)*

![Reflecting a vector about a surface normal](docs/images/feature-reflect.svg)

```csharp
var incoming = new Vector(1, -1, 0);
var surfaceNormal = new Vector(0, 1, 0);
var bounced = incoming.Reflect(surfaceNormal);   // (1, 1, 0)
```

### Other useful operations

**Back-face test.** Interpreting a vector as a face normal, `IsBackFace(lineOfSight)` reports whether
the face points away from the viewer (a negative dot product with the line of sight):

![back-face condition](docs/images/image070.gif)

**Perpendicularity.** `IsPerpendicular(other)` is true when the dot product is zero (with a tolerance
overload).

**Component utilities.** `SumComponents`, `SumComponentSqrs`, `PowComponents`, `SqrtComponents`,
`SqrComponents`, `Abs`, and `Round` (with digit/`MidpointRounding` overloads) apply arithmetic to each
component independently.

### Operations added for modern, everyday use

Beyond the original article, `Vector` has grown a handful of operations that round it out. Each is also
available to explore in the interactive [visualizer](RP.Math.Visualizer) — drag the vectors and watch
the result update live.

**Slerp — spherical interpolation.** Where `Interpolate` (linear) walks the straight **chord** between
two vectors, `Slerp` walks the **arc** at constant angular speed, keeping interpolated directions on
the sphere. It falls back to linear interpolation when the vectors are (anti)parallel.

![Slerp follows the arc while Lerp cuts the chord](docs/images/feature-slerp.svg)

```csharp
var halfway = Vector.XAxis.Slerp(Vector.YAxis, 0.5);   // ~ (0.707, 0.707, 0), still length 1
```

**ClampMagnitude.** Caps a vector's length at a maximum while keeping its direction (a no-op when it is
already short enough) — handy for limiting speeds and forces.

![ClampMagnitude caps the length and keeps the direction](docs/images/feature-clampmagnitude.svg)

```csharp
var limited = new Vector(3, 4, 0).ClampMagnitude(2.5);   // (1.5, 2, 0), length 2.5
```

**MoveTowards.** Steps from one point towards a target by at most a given distance, never overshooting
— the staple of frame-by-frame animation and AI movement.

![MoveTowards steps toward the target without overshooting](docs/images/feature-movetowards.svg)

```csharp
var next = Vector.Origin.MoveTowards(new Vector(10, 0, 0), 3);   // (3, 0, 0)
```

**Component-wise `ComponentMin`, `ComponentMax` and `Clamp`.** These work on **each axis
independently** — the X of the result depends only on the Xs of the inputs, and so on. Together
`ComponentMin`/`ComponentMax` give the two opposite corners of the axis-aligned box that just contains
the inputs:

![Component-wise min and max are the corners of the bounding box of A and B](docs/images/feature-componentminmax.svg)

```csharp
var lo = new Vector(1, 5, 3).ComponentMin(new Vector(4, 2, 6)); // (1, 2, 3)
var hi = new Vector(1, 5, 3).ComponentMax(new Vector(4, 2, 6)); // (4, 5, 6)
```

> **Not the same as `Min` / `Max`.** Those compare whole vectors by *magnitude* and return the shorter
> or longer one unchanged — they never mix components. Use the `Component…` versions for a per-axis
> result.

`Clamp` applies the same idea to a range, pushing each component into the `[min, max]` interval for its
axis:

![Clamp constrains each component into a box](docs/images/feature-clamp.svg)

```csharp
var inBox = new Vector(5, -5, 2).Clamp(Vector.Origin, new Vector(3, 3, 3)); // (3, 0, 2)
```

**Zero test, deconstruction and tuple conversion.**

```csharp
bool isZero   = v.IsZero();          // exactly (0, 0, 0)
bool nearZero = v.IsZero(1e-6);      // within a tolerance of zero
var (x, y, z) = v;                   // deconstruction
Vector p = (1.0, 2.0, 3.0);          // implicit from a tuple
```

### Standard constants

```csharp
Vector.Origin    // (0, 0, 0)   also Vector.Zero
Vector.XAxis     // (1, 0, 0)
Vector.YAxis     // (0, 1, 0)
Vector.ZAxis     // (0, 0, 1)
Vector.MinValue, Vector.MaxValue, Vector.Epsilon, Vector.NaN
```

`Vector` also implements `IComparable` (by magnitude), `Equals`/`GetHashCode` (with tolerance-aware
overloads), `IsNaN`, and `ToString` / `ToVerbString` for textual output.

---

## The line family: `Line`, `Ray`, `LineSegment`, `Chord`

All four describe a straight path through space. As the [overview](#the-types-at-a-glance) showed, they
differ only in *where they are allowed to stop* — and that single fact explains every difference in
their behaviour. They are immutable and built on `Vector`.

### `Line` — infinite in both directions

A line is the "no ends" member: it stretches forever both ways, so it has **no length and no
midpoint**. It is stored as a `Point` it passes through plus a unit `Direction` it runs along. Every
point on it is

```
P(t) = Point + t · Direction      for any t, positive or negative
```

Because `Direction` is unit length, `t` is a true distance. Construct one from a point and a direction,
or through two points:

```csharp
var line = new Line(new Vector(0, 0, 0), new Vector(1, 1, 0));
var same = Line.ThroughPoints(new Vector(0, 0, 0), new Vector(2, 2, 0));
```

**Closest point.** To find the point on the line nearest some `point`, project onto the direction. The
signed distance along the line is `t = (point − Point) · Direction` (no division, since `Direction` is
unit length), and the nearest point is `Point + t · Direction`. Crucially, `t` is **never clamped** —
the whole endless line is available, so the perpendicular foot is always reachable.

```csharp
Vector foot = line.ClosestPointTo(p);
double dist = line.DistanceTo(p);          // length of the perpendicular
bool on     = line.Contains(p, 1e-9);
```

**Parallelism.** Two lines are parallel when their directions point the same or exactly opposite ways.
Since `|a × b| = |a||b|sinθ` and our directions are unit length, the cross-product magnitude is just
`sinθ`, which is zero precisely at 0° or 180°:

```csharp
bool parallel = line.IsParallelTo(other, 1e-9);   // |Direction × other.Direction| ≤ tolerance
```

### `Ray` — infinite in one direction

A ray is the "one end" member — think of a beam of light or a line of sight. It has an `Origin` and a
unit `Direction`, and every point on it is

```
P(s) = Origin + s · Direction      for s ≥ 0
```

Negative `s` would be *behind* the start, which is not part of the ray. So `PointAt` clamps a negative
distance to the origin, and `ClosestPointTo` returns the origin whenever the projection falls behind
it. That is the only behavioural difference from `Line`:

```csharp
var ray = new Ray(new Vector(0, 0, 0), new Vector(1, 0, 0));
Vector hit = ray.ClosestPointTo(new Vector(-5, 2, 0));   // the origin — the point is behind the start
double d   = ray.DistanceTo(new Vector(3, 4, 0));
```

### `LineSegment` — finite, with two ends

A segment runs between a fixed `Tail` (start) and `Head` (end). Having two ends gives it things the
others lack: a `Length`, a `Midpoint`, and a `Direction`. Every point on it is written with a parameter
that runs from 0 to 1:

```
P(t) = Tail + t · (Head − Tail)      with t clamped to 0..1
```

```csharp
var seg = new LineSegment(new Vector(0, 0, 0), new Vector(3, 4, 0));
double len = seg.Length;             // 5  = |Head − Tail|
Vector mid = seg.Midpoint;           // (1.5, 2, 0) = (Tail + Head) / 2
Vector q   = seg.PointAt(0.25);      // a quarter of the way along
```

**Closest point.** Project onto the segment's direction `d = Head − Tail`. Minimising the squared
distance `|Tail + t·d − point|²` gives

```
t = ((point − Tail) · d) / (d · d)
```

then `t` is **clamped to 0..1** so the answer can never fall beyond an end, and `P(t)` is evaluated. (A
zero-length segment returns its tail.) This clamping at *both* ends is what distinguishes a segment
from a ray (one end) and a line (no ends).

**Other operations.** A segment can hand you the infinite [`Line`](#line--infinite-in-both-directions)
it lies on (`ToLine()`, useful when you want to ignore the ends), produce a `Reversed()` copy,
`Translate(offset)` to slide it, rotate its head about its tail (`RotateX/Y/Z(angle)`), and
`Interpolate(control)` along its length. It can be built from two points, six numbers, or an array of
six.

### `Chord` — a segment that knows its circle

A chord is a `LineSegment` whose two ends both lie on a circle — and which *also* knows that circle's
`Radius`. On its own a chord is just a segment; the radius is the whole reason the type exists,
because a surprising amount of circle geometry follows from the chord's length `c` and the radius `r`
alone. All of it comes from one right-angled triangle: drop a perpendicular from the circle's centre
to the chord; it meets the chord at its midpoint, with the radius `r` as the hypotenuse and the
half-chord `c/2` as one leg.

```csharp
var chord = new Chord(new Vector(-3, 0, 0), new Vector(3, 0, 0), radius: 5);
```

| Property | Meaning | Formula |
|----------|---------|---------|
| `CentralAngle` | The angle the chord subtends at the centre | `θ = 2·asin((c/2)/r)` |
| `ArcLength` | The curved distance along the edge between the ends | `r · θ` |
| `DistanceFromCentre` | Straight distance from centre to chord (the apothem) | `√(r² − (c/2)²)` |
| `Sagitta` | How far the arc bulges past the chord (its height) | `r − DistanceFromCentre` |
| `Diameter` | The longest possible chord | `2r` |

Each derivation is spelled out in the source. For example `sin(θ/2) = (c/2)/r` gives the central angle;
Pythagoras on the same triangle gives the centre-to-chord distance; and the sagitta is simply what is
left of the radius once you subtract that distance. (Ratios are clamped and roots guarded so a
full-diameter chord resolves to a clean 180° rather than a non-number. "Sagitta" is Latin for *arrow* —
the chord is the bow, the height is the drawn arrow.)

---

## `Plane`

A plane is an infinite flat surface, stored in the standard algebraic form

```
A·x + B·y + C·z + D = 0
```

where `(A, B, C)` is the plane's `Normal` (the direction it faces) and `D` is a signed offset. It is an
immutable value type following the `Vector` design.

**Building a plane.** The raw constructor stores whatever coefficients you give it; the geometry
factories produce a unit normal for you:

```csharp
var p1 = Plane.FromPointNormal(point, normal);     // a point on it + which way it faces
var p2 = Plane.FromThreePoints(a, b, c);           // through three points (throws if collinear)
var xy = Plane.XY;                                  // z = 0; also Plane.XZ, Plane.YZ
```

`FromThreePoints` builds the normal with the cross product `(b − a) × (c − a)` following the right-hand
rule for the order a → b → c, and throws if the points are collinear (they do not define a unique
plane).

**Signed distance and sides.** The key operation is the **signed** distance from a point to the plane:
positive on the side the normal points to, negative on the far side, zero on the plane itself. From it
everything else follows:

```csharp
double s   = plane.SignedDistanceTo(point);   // sign tells you which side
double d   = plane.DistanceTo(point);         // unsigned
int    side = plane.SideOf(point, 1e-9);      // +1, -1 or 0 (on the plane)
bool   on   = plane.Contains(point, 1e-9);
Vector foot = plane.ClosestPoint(point);      // orthogonal projection onto the plane
Vector mir  = plane.Reflect(point);           // mirror the point through the plane
```

**Line intersection.** `TryIntersectLine` returns where a line crosses the plane, reporting `false`
when the line is parallel (no single crossing). `IntersectLineParameter` returns just the parameter
`t` along the line, or `NaN` if parallel:

```csharp
if (plane.TryIntersectLine(linePoint, lineDirection, out Vector where)) { /* … */ }
```

**Comparing planes.** `==` is an exact component-wise comparison of the four coefficients, but two
planes can describe the *same surface* with differently scaled (or flipped) coefficients —
`Equals(other, tolerance)` checks for that by comparing both in normalized form (and negated). Other
helpers: `Normalize()` (unit normal, same surface), unary `-` (flip orientation), `IsDegenerate()`
(zero normal), `IsParallelTo(other, tolerance)`, and deconstruction into `(A, B, C, D)`.

---

## `Angle`

`Angle` is an immutable value type for *an amount of turn*. It stores radians internally but lets you
read and write degrees, radians or gradians, so a bare `double` never leaves you guessing about units.
A positive value is taken clockwise, a negative value counter-clockwise, and angles beyond a full
circle are automatically reduced.

```csharp
var a = new Angle(System.Math.PI / 2);       // radians
var b = new Angle(90, AngleUnits.DEG);        // degrees
double deg = a.Deg;   // 90
double rad = a.Rad;   // π/2
Angle implicitlyRadians = 1.5;                // double → Angle (radians)
```

**Arithmetic and comparison.** The usual operators are overloaded (`+`, `-`, `*`, `/`, unary `-`/`+`,
`!` to flip clockwise/counter-clockwise, and the comparison operators). Equality is tolerance-aware via
a shared static `Tolerance`.

**Classification.** `Angle` can tell you what *kind* of angle it is — `IsAcute`, `IsRightAngle`,
`IsObtuse`, `IsStraitAngle`, `IsReflex`, `IsFullOrZeroAngle`, `IsOblique`, `IsClockwise` — and how two
angles relate: `IsComplementOf` (sum to 90°), `IsSupplementOf` (180°), `IsExplementOf` (360°). It also
offers `Complement`, `Supplement`, `SmallAngle`, `Reflex`, `Abs`, and clockwise/counter-clockwise
conversions.

**Trigonometry.** Static and instance `Sin`, `Cos`, `Tan` (and their hyperbolic forms), plus the
inverse builders `Asin`, `Acos`, `Atan` that *return* an `Angle`.

**Constants.** `Angle.Right_Angle`, `Angle.Strait_Angle`, `Angle.Full_Angle`,
`Angle.Three_Quater_Circle`, `Angle.Zero_Angle`, and the matching `…_Rad` radian literals.

The companion `AngleUnits` enum names the three unit systems (`DEG`/`DEGREE`, `RAD`/`RADIAN`,
`GRAD`/`GRADIENT`). The `ToString` formats support `d` (degrees), `r` (radians), `g` (gradians) and `v`
(a verbose description that names the kind of angle).

---

## Orientation: `Rotation`, `Attitude`, `Quaternion`

These three types all answer the same question — *which way is something facing?* — but trade
readability against robustness. They convert freely between one another, so you can author in the
friendly form and store in the robust one.

### `Rotation` — Euler angles (X, Y, Z)

The human-friendly "front door": three `Angle`s applied about the X, then Y, then Z world axes
(equivalently the matrix product `Rz · Ry · Rx`).

```csharp
var r = new Rotation(pitch, yaw, roll);          // angles about X, Y, Z
var rx = Rotation.AboutX(angle);                 // single-axis factories: AboutX/Y/Z
Vector turned = r.Rotate(v);
```

Component arithmetic (`+`, `-`) acts **component-wise on the angles** — useful for nudging a rotation,
but *not* the same as composing two rotations. For true composition, convert to a `Quaternion` and
multiply. `ToQuaternion` / `FromQuaternion` bridge the two exactly (folding X into Z at the
gimbal-lock poles), and `Inverse()`, `ToMatrix()` and `ToAttitude()` round it out.

### `Attitude` — yaw / pitch / roll

The same idea as `Rotation` in aviation naming: `Yaw` (heading, about Y), `Pitch` (elevation, about X)
and `Roll` (bank, about Z). It shares a single composition convention with `Rotation` (it delegates
through `ToRotation().ToQuaternion()`), so the two never disagree.

```csharp
var att = new Attitude(yaw, pitch, roll);
var y   = Attitude.FromYaw(angle);               // FromYaw / FromPitch / FromRoll
Vector v2 = att.Rotate(v);
```

### `Quaternion` — the robust form

A quaternion `(x, y, z, w)` is a four-component number used to represent rotations **without** the
gimbal-lock and interpolation problems of Euler angles. The scalar (real) part is `W`; the vector
(imaginary) part is `(X, Y, Z)`. Multiplication uses the Hamilton convention. It follows the `Vector`
design — static and instance forms, strict and `…OrDefault` normalisation, tolerance-aware equality.

```csharp
var q = Quaternion.FromAxisAngle(axis, angle);   // a turn about an axis
Vector rotated = q.Rotate(v);
q.ToAxisAngle(out Vector ax, out Angle ang);
```

Why it is the dependable representation:

- **No gimbal lock** — it never loses a degree of freedom the way three stacked Euler angles can.
- **Composes cleanly** — multiplying two quaternions composes their rotations (`q1 * q2`).
- **Interpolates smoothly** — `Slerp` walks the shortest arc at constant angular speed (with `Lerp`
  as the cheaper straight-line approximation).

It also offers `Conjugate`/`Inverse`, `DotProduct`, `AngleBetween`/`AngleTo`, `FromYawPitchRoll`,
`ToMatrix`, the identity/zero/NaN constants, and predicates `IsUnit`, `IsIdentity`, `IsZero`, `IsNaN`.

---

## `Pose`

A `Pose` is a rigid placement in space — a **position together with an orientation** ("where" *and*
"which way"). It is the human-meaningful form of a rigid transform, as opposed to a 4×4
[`Matrix`](#matrix). Internally the orientation is kept as a unit `Quaternion` (the robust
representation), but you can construct it from, and read it back as, the friendlier `Rotation` or
`Attitude`.

```csharp
var pose = new Pose(position, rotation);          // also accepts a Rotation or an Attitude
var at   = Pose.At(position);                      // no rotation
var fa   = Pose.FromAxisAngle(position, axis, angle);
```

**Applying a pose** transforms between the pose's local space and world space:

```csharp
Vector world  = pose.Apply(localPoint);           // rotate, then translate
Vector dirW   = pose.ApplyDirection(localDir);    // rotate only (directions ignore position)
Vector local  = pose.ApplyInverse(worldPoint);    // the inverse transform
Matrix m      = pose.ToMatrix();                   // the equivalent 4×4 transform
```

**Composition.** `Compose` (and the `*` operator) chains two poses — `outer * inner` applies `inner`
first, then `outer` — and `Inverse()` undoes a pose. Modifier methods return a new pose:
`Translate(offset)`, `RotateBy(delta)`, `WithPosition(p)`, `WithRotation(q)`. Reading helpers expose
the orientation as Euler angles (`RotationAsEuler`) or yaw/pitch/roll (`RotationAsAttitude`), and
`Pose.Identity` is the pose at the origin with no rotation.

---

## `Matrix`

`Matrix` is the general-purpose 4×4 transformation matrix — the linear-algebra workhorse behind
translation, scaling and rotation, and the form that composes all of them by multiplication. Whereas
`Pose` is the *readable* form of a rigid transform, `Matrix` is the *general* form (it can also scale
and shear).

It can be built from a 3×3 or 4×4 array (smaller inputs are promoted to homogeneous 4×4 form), from
sixteen explicit values, from a `Vector`, or copied. It supports the arithmetic operators (`+`, `-`,
scalar `*` and `/`, matrix `*`, and matrix–vector `*` to transform a vector), `Transpose()`,
`IsIdentity` / `IsZero`, tolerance-aware equality and formatting, and the `Identity` / `Zero`
constants.

Factory builders create the standard transforms:

```csharp
var t = Matrix.TranslationMatrix(new Vector(1, 2, 3));
var s = Matrix.ScalingMatrix(2, 2, 2);
var rx = Matrix.RotationMatrixAboutXAxis(angle);   // …AboutYAxis, …AboutZAxis
Vector moved = t * v;                               // apply the transform to a vector
```

---

## `OrthogonalAxes`

An `OrthogonalAxes` is a **coordinate-system convention** — a constant that records, for a given project or
ecosystem, *which way is which*: which Cartesian axis means right, which means up, which means
forward, and which direction along each is positive. It answers the question "when this project says
`y`, does it mean *up* or *into the screen*?"

Different worlds disagree, and that is the whole reason the type exists. OpenGL is y-up and
right-handed; Direct3D is y-up and left-handed; Blender and most CAD/engineering maths are z-up;
Unreal is z-up and left-handed. An `OrthogonalAxes` pins that choice down in one immutable value so the rest of
your maths never has to guess.

**It is a label, not a motion.** An `OrthogonalAxes` is meant to be chosen once, at the start, and handed to any
function that needs to know your frame. It never gets rotated — arbitrary orientations are the job of
[`Quaternion`](#orientation-rotation-attitude-quaternion) and [`Pose`](#pose). (This single type
replaces two earlier ones: a label-only `Axis` and a free-vector `OrthogonalAxis` that — by storing
three arbitrary vectors — let you build skewed, non-unit, meaningless "axes". Merging them and keeping
only genuine conventions is what makes a bad frame impossible to construct. The name is the old
`OrthogonalAxis` corrected to the plural it always should have been: it describes a *set* of axes, not
one — and so does not collide with the single direction vector that an axis–angle rotation
(`Quaternion.FromAxisAngle`) calls an "axis".)

### How a convention is built

Each of the three Cartesian axes is given a role from one of the three **opposed pairs**:

- `UP` / `DOWN`
- `LEFT` / `RIGHT`
- `NEAR` / `FAR`

```csharp
var openGl = new OrthogonalAxes(AxisAlignment.RIGHT, AxisAlignment.UP, AxisAlignment.NEAR);
```

The **sign** is carried by *which* member of the pair you choose: `Y = UP` means +y is up; `Y = DOWN`
means +y is down (so up is −y). `Z = FAR` means +z points into the scene; `Z = NEAR` means +z points
back toward the viewer.

A convention must use **each pair exactly once** — so two axes can never both mean "up", and no pair
can be left out. With three axes and three pairs, simply forbidding duplicates is enough to guarantee a
complete, non-contradictory frame, which is why a meaningless `OrthogonalAxes` cannot be constructed at all.

> **Why `NEAR`/`FAR` and not `FORWARD`/`BACKWARD`?** `near` and `far` are *positional* labels — they
> say where a thing **is**, just like `up`/`down` and `left`/`right`. `forward`/`backward` carry a
> sense of **motion**, which belongs to a velocity or a rotation, not to a fixed frame. Naming all
> three pairs as positional opposites keeps the type consistent in kind. (`far` = into the scene, away
> from the viewer; `near` = toward the viewer.)

### The basis vectors and handedness

From the labels alone, an `OrthogonalAxes` derives the signed unit vectors `Right`, `Up` and `Forward` (forward
being the *far* direction — into the scene), and its `Handedness`:

```csharp
OrthogonalAxes.OpenGL.Forward;      // (0, 0, -1) — far is -z, because z is NEAR
OrthogonalAxes.OpenGL.Handedness;   // Handedness.Right
```

These are *derived*, so they can never drift out of step with the labels.

**Handedness, in one gesture.** Point your right hand's fingers along `Right` and curl them toward
`Up`; your thumb gives the right-handed third direction. In numbers:

```
X × Y = (1, 0, 0) × (0, 1, 0) = (0, 0, 1) = +Z
```

With right = +x and up = +y, the right-hand rule puts the third axis at +z. So whether the *far*
direction is +z or −z is exactly what decides handedness:

| Far direction (with right = +x, up = +y) | Handedness |
|------------------------------------------|------------|
| far = −z | **right**-handed |
| far = +z | **left**-handed |

That single fact is the entire difference between OpenGL and Direct3D: same right, same up, opposite
depth. (It is also why "OpenGL is right-handed, yet you look down −z" is not a contradiction — the
*axes* obey the right-hand rule with +z pointing toward you; the camera simply faces the other way.
Where the camera looks is a rendering concern, not part of the convention.) The type computes
handedness as the sign of `(Right × Up) · Forward` — negative is right-handed — so it is read off the
geometry, never typed in by hand.

### The predefined conventions

There is deliberately **no default**. A project must name the convention it wants, because the right
answer depends on the ecosystem the maths is feeding. The common systems are provided as constants;
several share a frame, so the constants are intentional aliases:

| Constant(s) | x | y | z | Up axis | Handed |
|-------------|---|---|---|---------|--------|
| `OrthogonalAxes.OpenGL`, `OrthogonalAxes.Maya`, `OrthogonalAxes.Godot`, `OrthogonalAxes.MathsYUp` | right | up | near | y | right |
| `OrthogonalAxes.DirectX`, `OrthogonalAxes.Direct3D`, `OrthogonalAxes.Unity` | right | up | far | y | left |
| `OrthogonalAxes.Blender`, `OrthogonalAxes.Max3ds`, `OrthogonalAxes.MathsZUp` | right | far | up | z | right |
| `OrthogonalAxes.Unreal` | far | right | up | z | left |

```csharp
OrthogonalAxes.OpenGL.Z;    // NEAR  — +z toward the viewer
OrthogonalAxes.DirectX.Z;   // FAR   — +z into the screen
```

### Translating between conventions

The point of holding a convention as a value is to *feed it into maths that must respect it*.
`OrthogonalAxes.Map` converts coordinates between a stored convention and a canonical right/up/far frame, so a
function can take your `OrthogonalAxes`, map its inputs into the easy canonical frame, do the work once there,
and map the result back — instead of branching on the convention in every method.

---

## Shapes: conceptual and placed

The shape types are built around one deliberate split: **what a shape *is*** is kept separate from
**where it *is***.

- A **conceptual shape** — `Triangle`, `Rectangle`, `Cuboid`, `Cylinder` — describes only the intrinsic
  geometry: size and proportion, in no particular place. It has no centre, no orientation, no position
  of any kind. Everything it computes (area, volume, angles, diagonals, classification) depends solely
  on its own dimensions.
- A **placed shape** — `PlacedTriangle`, `PlacedRectangle`, `PlacedCuboid`, `PlacedCylinder` — is a
  conceptual shape **together with a [`Pose`](#pose)** that puts it somewhere in the world. Only the
  placed form answers world-space questions: does it contain this point, what is the closest point on
  it, where does this line or ray strike it.

### Why keep them apart

Because position confuses size-only questions. If you want to know which of two circles is bigger, the
honest comparison is between their areas — and a position has no business being part of that answer; it
can only muddy what "bigger" means. Keeping the conceptual shape position-free makes those comparisons
exact and unambiguous:

```csharp
var a = new Cylinder(radius: 2, height: 5);
var b = new Cylinder(radius: 3, height: 1);
bool aIsBigger = a.Volume > b.Volume;   // pure size — no placement anywhere in sight
bool same      = a == b;                 // equality means "same shape", not "same place"
```

Each conceptual shape implements `IComparable<T>` and the comparison operators (`<`, `>`, `<=`, `>=`)
**by size** — area for the flat shapes, volume for the solids — exactly as [`Vector`](#vector--the-foundation)
compares by magnitude. As with `Vector`, `==` still means *equal dimensions*, not *equal size*.

### How a placement is applied

Each conceptual shape has **one hand-written placed partner**, named `Placed` + the shape name (no
cleverer name than that). A placed shape stores the conceptual `Shape` and a `Pose`, and delegates:

```csharp
var disc  = new Rectangle(4, 6);
var board = new PlacedRectangle(disc, Pose.At(new Vector(0, 0, 10)));
board.Area;                       // 24 — delegated straight to the conceptual shape
board.Contains(somePoint, 1e-9);  // a world-space question only the placed form can answer
```

The placed form does its world-space maths by working **in the shape's own local frame**. It maps the
incoming world point back into that frame with `Pose.ApplyInverse`, runs the simple canonical maths
there (where a rectangle is axis-aligned at the origin, a cylinder runs along local +Z, and so on), and
maps any returned point forward again with `Pose.Apply`. Writing the geometry once, in the easy frame,
is the whole reason the two layers are kept apart.

### Placement and symmetry

How much of an orientation actually *matters* depends on the shape's symmetry — which is why the placed
types differ in what their pose meaningfully controls:

| Symmetry | What the placement pins down | Shapes |
|----------|------------------------------|--------|
| Fully symmetric (a point) | only a centre; orientation is irrelevant | `PlacedSphere` |
| Rotationally symmetric about an axis | a centre and one axis direction; spin about it is irrelevant | `PlacedCircle`, `PlacedCylinder`, `PlacedCone`, `PlacedCapsule`, `PlacedAnnulus`, `PlacedTorus` |
| No symmetry to spare | a full orientation | `PlacedRectangle`, `PlacedCuboid`, `PlacedEllipse`, `PlacedEllipsoid`, `PlacedSector` |
| Defined by its corners | placement is implied by the corner positions | `PlacedTriangle`, `PlacedPolygon`, `PlacedTetrahedron` |

A `PlacedSphere` therefore ignores its pose's orientation entirely, and a `PlacedCircle` cares only about
its normal (any spin about that normal leaves the disc unchanged) — the symmetry simply makes the surplus
parts of the `Pose` inert.

### Filled regions

Every shape is treated as a **filled region**, not just an outline or surface. `Contains` means "on or
within"; a `Rectangle` is the solid rectangle, a `Cylinder` is the solid cylinder. This matches the
existing `Sphere`, whose `Contains` has always meant "on or within".

### The shapes

**Flat shapes** (a `Placed…` partner lays each on a plane in space):

| Conceptual shape | Defined by | Placement notes |
|------------------|-----------|-----------------|
| `Circle` | radius | a flat disc; axis symmetry, so the placed form needs only a centre and a normal |
| `Triangle` | three side lengths (A, B, C) | Heron area, angles by the law of cosines, classification; placed form adds corners, containment, closest point, line/ray hits (see special case below) |
| `Rectangle` | width, height | full orientation; placed form lies on a plane with width along local +X, height along +Y |
| `Ellipse` | two semi-axes | full orientation; eccentricity and foci; placed closest-point uses Eberly's robust method |
| `Annulus` | inner & outer radius | axis symmetry; the filled ring between two concentric circles |
| `Sector` | radius, sweep angle | a pie slice; the placed form is anchored at its apex (not its centroid) |

**Solids** (a `Placed…` partner positions each in space):

| Conceptual shape | Defined by | Placement notes |
|------------------|-----------|-----------------|
| `Sphere` | radius | fully symmetric, so the placed form needs only a centre (its pose's orientation is inert) |
| `Cuboid` | width, height, depth | full orientation; oriented box (the slab test runs in its local, axis-aligned frame) |
| `Cylinder` | radius, height | axis symmetry; runs along local +Z, curved side + two caps |
| `Cone` | base radius, height | axis symmetry; placed form stands on its base, closest-point solved in the axial–radial half-plane |
| `Capsule` | radius, cylinder height | axis symmetry; a swept segment (cylinder with two hemispherical caps) |
| `Ellipsoid` | three semi-axes | full orientation; intersection by scaling to a unit sphere, closest-point by a Lagrange-multiplier bisection |
| `Torus` | major & minor radius | axis symmetry; line/ray intersection is an exact **quartic** (see below) |

**Vertex-defined shapes** (positioned in their own right — no conceptual partner):

| Shape | Defined by | Notes |
|-------|-----------|-------|
| `PlacedPolygon` | ordered coplanar corners | any simple polygon (convex or concave); robust area/normal/centroid, crossing-number containment |
| `PlacedTetrahedron` | four corners | the simplest solid; barycentric containment, per-face intersection |

> **"Cuboid", not "Box".** The solid box is named `Cuboid` so the name `Box` stays free for a future
> axis-aligned *bounding* box (a different idea — the box that just contains something, used for quick
> rejection — and the reason the earlier `BoundingBox` was removed rather than renamed).

### Vertex-defined shapes

`PlacedPolygon` and `PlacedTetrahedron` are defined directly by their corner points, so — like the
corners of `PlacedTriangle` — they are **always placed**: their position is implicit in the corners and
there is no separate conceptual form (hence they carry the `Placed` prefix but have no plain-named
partner). `PlacedPolygon` handles any simple planar polygon, convex or concave (area, normal and centroid
come from summed edge cross-products; containment is a crossing-number test done in the polygon's own
plane). `PlacedTetrahedron` is the simplest solid, with barycentric containment and intersection tested
face by face.

### Exact intersections and the quartic solver

Line/ray intersection is exact for every placed solid. Most reduce to a quadratic in the line parameter
(sphere, cylinder, cone, capsule, and the ellipsoid after scaling space to a unit sphere). The `Torus`
is the exception: substituting the line into its implicit equation gives a **quartic**, so the library
includes a small real-root solver, [`PolynomialRoots`](RP.Math/PolynomialRoots.cs) (quadratic by formula,
cubic by Cardano, quartic by Ferrari). A line can pierce a torus up to four times; the solver returns all
real crossings and the placed torus reports the nearest and farthest.

### `Triangle`, the special case

A triangle is the one shape whose natural placed form is its **three corners**, not a tidy centre and
size. Its conceptual form is therefore its three **side lengths** — the position-free description that
still fixes area, angles and classification. `PlacedTriangle` stores that `Triangle` plus a `Pose` whose
position is the triangle's centroid; in the local frame the centroid sits at the origin, the A→B edge
runs along local +X and corner C lies on the local +Y side (a layout recovered from the side lengths via
the law of cosines).

The everyday way to build one is from three world corners:

```csharp
var t = PlacedTriangle.FromVertices(a, b, c);   // recovers both the side lengths and the placing pose
t.A; t.B; t.C;                                   // round-trips back to the original corners
```

`FromVertices` has to turn three points into a `Pose`, which means recovering an orientation from a
coordinate frame. The library has no matrix-to-quaternion conversion, so the rotation is built by
**composing the library's own `Quaternion.FromAxisAngle` turns** — first swinging local +Z onto the face
normal, then spinning about that normal to line local +X up with the A→B edge. Building it from the
existing primitives keeps it consistent with the quaternion conventions used everywhere else (and the
round-trip is covered by tests).

### `Circle` and `Sphere`

`Circle` and `Sphere` follow the same split as everything else: the conceptual `Circle` is just a radius
(area, circumference) and the conceptual `Sphere` is just a radius (volume, surface area), each comparable
by size, while `PlacedCircle` and `PlacedSphere` add the placement and the world-space maths. They were
originally written as positioned types and have been converted to match — `PlacedCircle.InXYPlane(circle,
centre)` and `PlacedSphere.At(sphere, centre)` are the everyday factories.

### Decisions captured here

- **Conceptual and placed are separate types.** Size-only maths lives on the conceptual shape and never
  sees a position; world-space maths lives on the placed shape.
- **Paired concrete types, not a generic wrapper.** Each shape has its own hand-written placed partner
  rather than a single `Placed<T>` over an interface (an earlier `IShape` abstraction was removed).
- **Named `Placed` + the shape name.** `PlacedTriangle`, `PlacedRectangle`, and so on — no invented names.
- **A placed shape is a conceptual shape plus a `Pose`.** World queries are done in the shape's local
  frame via `ApplyInverse`/`Apply`.
- **Shapes are filled regions.** `Contains` means "on or within".
- **Conceptual shapes are comparable by size** (`IComparable`, comparison operators), mirroring `Vector`.
- **Vertex-defined shapes are always placed.** `PlacedPolygon` and `PlacedTetrahedron` are defined by
  general corner points (not regularised), so they have no conceptual partner; they carry the `Placed`
  prefix because they only ever exist positioned — matching how `PlacedTriangle`'s corners already work.
- **`Circle` and `Sphere` follow the split too.** Originally positioned, they are now a conceptual radius
  plus `PlacedCircle` / `PlacedSphere`, consistent with every other shape.
- **Intersections are exact, including the torus.** Rather than approximate the torus's line/ray hits, a
  real quartic solver (`PolynomialRoots`) was added so its crossings are exact like the other solids'.

---

## Supporting numeric helpers

A small amount of plumbing supports the maths above and is not usually called directly:

- **`DoubleExtension`** — floating-point comparison helpers (e.g.
  `AlmostEqualsWithAbsTolerance`) used throughout for tolerance-aware equality. Compares doubles
  sensibly rather than with the unreliable `==`.
- **`ExpandedDouble`, `UlongExtension`** — bit-level inspection of doubles, used by the ULP
  ("units in the last place") comparison strategy.
- **`Tolerance`** — interfaces and strategies (`UlpsTolerance` and others) describing *how close is
  close enough* when comparing values.
- **`Exceptions/NormalizeVectorException`, `Exceptions/NormalizeQuaternionException`** — the errors
  thrown by the strict `Normalize` methods when asked to normalise a zero-length value (the
  `…OrDefault` variants avoid these).

> The `Shape` namespace is documented above under [Shapes: conceptual and
> placed](#shapes-conceptual-and-placed).

---

## Points of interest

A number of resources informed the original vector article and the maths in this library:

- [CSOpenGL](http://sourceforge.net/projects/csopengl/) — Lucas Viñas Livschitz
- [Exocortex](http://www.exocortex.org/) — Ben Houston
- *Essential Mathematics for Computer Graphics* — John Vince (ISBN 1-85233-380-4)

---

## Status and history

The library began as the single `Vector3` type from the CodeProject article (since renamed `Vector`)
and has grown to cover the line family, planes, angles, the orientation types, poses and matrices —
all in the spirit of the original: small immutable values, rich and well-explained mathematical
functionality, clarity ahead of speed.

It is an early-state, actively-developing project: the core numeric types are well covered by tests.
The former `Axis` and `OrthogonalAxis` types are now merged into a single `OrthogonalAxes` convention,
which carries the coordinate-frame logic they left unfinished and exposes the major systems (OpenGL,
Direct3D, Unity, Unreal, Blender, …) as named constants. The `Shape`
types follow a settled conceptual/placed split — `Circle`, `Sphere`, `Triangle`, `Rectangle`, `Ellipse`,
`Annulus`, `Sector`, `Cuboid`, `Cylinder`, `Cone`, `Capsule`, `Ellipsoid` and `Torus`, each with a
`Placed…` partner, plus the always-placed `PlacedPolygon` and `PlacedTetrahedron`.
