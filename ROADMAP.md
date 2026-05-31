# RP.Math — Roadmap

> Documentation only. Not compiled, not packed into the NuGet package. It records the design
> direction so implementation stays consistent.

## Vision

`RP.Math` is a library of **simple, immutable data objects that expose extensive mathematical
functionality** — small value types that hold minimal state but provide a rich, discoverable API.
`Vector` is the finished exemplar; every other type should grow toward the same standard.

We model basic mathematical concepts as first-class types — `Vector`, `Angle`, `Axis`, `Matrix`,
`Quaternion`, lines, planes — and a family of **geometric shapes** with extensive behaviour
(area, containment, intersection, …).

**Dimensionality: this is a 3D library.** `Vector` is `(x, y, z)`; there is no 2D point type. "Flat"
shapes (circle, rectangle, triangle) are modelled as **planar shapes that live in 3D space** — a shape
plus the plane it lies in. There is no `Vector2`, no `Pose2D`, and no "z = 0 by convention" fudge.

## The house style (the "Vector template")

Every rich type should aim to provide:

- **Immutable value type** (`struct`), `[Serializable]`, readonly backing fields.
- Implements **`IEquatable<T>`**, **`IFormattable`**, and (where ordering is meaningful) `IComparable`/`IComparable<T>`.
- **Dual static + instance** form of each operation; the instance form delegates to the static form.
- **Tolerance-aware** equality/comparison (via `DoubleExtension.AlmostEqualsWithAbsTolerance`).
- Deliberate **NaN / infinity / degenerate** handling: a strict variant that throws and a safe
  `…OrDefault` variant that returns a sensible default.
- Multiple **constructors**, useful **static factory** methods, and **static constants**
  (identity / zero / unit values).
- **Conversion operators** and **deconstruction** where natural (tuples, related types).
- `ToString()` + `ToString(format, IFormatProvider)`, `GetHashCode`, `==` / `!=`.
- Concise **XML docs** on public members; operations grouped into `#region`s by category.
- Custom exceptions for domain failures (e.g. `NormalizeVectorException`, `NormalizeQuaternionException`).

## Current state (snapshot)

| Tier | Types |
| --- | --- |
| **Rich (done)** | `Vector`, `Angle`, `Matrix`, `Quaternion`, `Rotation`, `Attitude`, `Pose`, `Plane` |
| **Moderate** | `Axis`, `LineSegment` |
| **Stub (data only)** | `Rectangle`, `Ray`, `Chord`, `OrthogonalAxis` |
| **Dormant** | `VectorPair` (fully commented out) |

Orientation is now layered as intended: `Rotation` (Euler X/Y/Z) and `Attitude` (yaw/pitch/roll) are the
human-friendly front doors; both convert to/from `Quaternion`, which is the robust internal form. `Pose`
(position + orientation) accepts any of the three but **stores a normalized `Quaternion` internally**.

Provenance note: `Vector` and its supporting numeric helpers, the tests, and the visualizer were copied
from the standalone **Vector** repo (https://github.com/dickiepotter/Vector.git) and are no longer
synced — check upstream before changing them. See `readme.md`.

## Cross-cutting decisions

1. **Axis → rotation convention (blocker).** `OrthogonalAxis`, `LineSegment`, and the
   `Vector.Yaw/Pitch/Roll(Angle, Axis)` overloads all `throw NotImplementedException` for the same
   reason: there is no agreed mapping from an `Axis` to a rotation. Resolving this one decision
   unblocks all three. (Quaternion already sidesteps it with the unambiguous `FromAxisAngle`.)
2. **`TestStuff/` → real folder.** `Ray`, `LineSegment`, `Chord`, `VectorPair` are production-intent
   geometry types living under a misleading `TestStuff/` folder. Move them to `Shape/` (or `Geometry/`).
3. **`VectorPair` fate.** Decide: revive to the house standard, or delete. Currently ~860 lines commented out.
4. **3D-only (decided).** No `Vector2` / `Pose2D`. Flat shapes are planar shapes in 3D (see below).

---

# Layered architecture for geometric shapes

A shape can be described **in relation to itself** (width, height, radius, internal angles) but in
most real use it is **also placed somewhere** (a position, and an orientation). We separate those
concerns into layers so functionality composes cleanly instead of every shape re-implementing placement.

## The layer stack

```
Layer 0  Primitives          Vector (point/direction), Angle, Axis              [exist]
Layer 1  Orientation/algebra  Matrix, Quaternion, Rotation, Plane               [Vector/Matrix/Quat done]
Layer 2  Placement (Pose)     Pose = (position: Vector, rotation: Quaternion)   [NEW]
Layer 3  Intrinsic shapes     size/proportion only, in LOCAL space,             [NEW]
                              canonically centred at the origin
Layer 4  Placed shapes        an intrinsic shape expressed in WORLD space       [NEW]
                              (carries its centre + orientation, i.e. a Pose)
Layer 5  Compound/queries     intersections, unions, distance, closest point,   [NEW]
                              bounding volumes, sweeps
```

Layers 0–1 already follow the house style (Plane still needs fleshing out). The new work is Pose (2),
shapes (3–4), and queries (5).

## Placement: a single `Pose` (3D)

Because the library is 3D, there is **one** placement type:

```csharp
public readonly struct Pose   // position + orientation ("where + which way")
{
    public Vector     Position { get; }
    public Quaternion Rotation { get; }
}
```

`Pose` carries the transform machinery written once for every shape: `Apply(Vector localPoint)` (place
a local point into world space), `Inverse()`, `Compose(Pose)`, `ToMatrix()`, plus `Pose.Identity`. It is
the human-meaningful form of a rigid transform (a point + a rotation) rather than a 4×4 `Matrix`.

A **planar** shape additionally needs the plane it lies in. Two equivalent encodings — pick one and be
consistent:
- **Centre + normal** (compact; the in-plane "spin" of the shape is a separate `Angle` where it matters,
  e.g. a rectangle's rotation within its plane), or
- **Centre + full `Quaternion`/`Pose`** (uniform with solids; the shape's local XY plane is mapped into
  world space by the rotation). **Recommended** for uniformity — a planar shape is "an intrinsic 2D
  outline expressed through a `Pose`", with its local +Z as the plane normal.

## The origin / anchor convention (the "good answer that already exists")

The origin **does not change arbitrarily per shape** — it follows one rule:

- **Area / volume shapes are centre-anchored.** Circle, Ellipse, Rectangle, Box, Sphere, RegularPolygon
  → the canonical local origin is the **geometric centre**. Triangle / Polygon → the **area centroid**.
  This is the maths/CAD/engine convention. (The **top-left / min-corner** origin used by *screen/UI*
  frameworks like `System.Drawing.Rectangle` or CSS exists only because pixel coordinates grow
  down-and-right — a UI artifact, not the mathematical choice.)

  Why centre, uniformly:
  - Rotation and scaling about the centre is the intuitive default.
  - Symmetric shapes have an unambiguous centre; one convention covers every shape.
  - Containment / distance / bounding-volume maths is simplest relative to the centre.
  - It removes per-shape special cases ("where is a triangle's origin?" → its centroid).

- **Linear / ray primitives are defined by their own points**, not a centre. `LineSegment` =
  (start, end); `Ray` = (origin, direction); `Line` = (point, direction). These already carry a natural
  defining point, so we don't impose a centre (a `Midpoint` accessor is still offered).

Other anchors (a corner, min/max) are **never stored** — they are *derived accessors* or *named
factories* that convert to the canonical centre:

```csharp
var r = Rectangle.FromCenter(centerPose, width, height);   // canonical
var r = Rectangle.FromCorners(p0, p1, p2);                  // convenience -> centre + plane
Vector[] v = r.Vertices;        // derived from centre + size + orientation
Box bb     = r.BoundingBox;     // derived, axis-aligned 3D box
```

## How "intrinsic" vs "placed" is modelled

Recommendation: **Model B**.

- **Model A — separate intrinsic and placed types** (`RectangleSize(w,h)` + `Rectangle(size, pose)`).
  Purest separation, but doubles the type count and is clumsy for the common case.
- **Model B — one positioned struct per shape, centre-anchored, sharing a shape contract, with the
  reusable `Pose` for transforms (recommended).** A shape *carries* its placement because that's how it
  is used ~95% of the time, but the convention (centre anchor) and the transform machinery (`Pose`,
  `Translate`/`RotateAbout`/`Scale`/`TransformedBy`) are uniform and written once. The "intrinsic-only"
  view is simply the shape at `Pose.Identity` — no separate type needed.
- **Model C — generic `Shape<TIntrinsic>` composition.** Elegant on paper, awkward with value-type
  maths in C#; over-engineered here.

## Shared contracts (planar vs solid, both in 3D)

The split is **planar vs solid**, not 2D vs 3D — every shape lives in 3D space.

```csharp
/// A flat shape occupying a plane in 3D space.
public interface IPlanarShape
{
    double Area      { get; }
    double Perimeter { get; }
    Vector Centroid  { get; }   // the canonical origin, in world space
    Vector Normal    { get; }   // unit normal of the plane it lies in
    Plane  Plane     { get; }   // the supporting plane
    Box    BoundingBox { get; } // axis-aligned 3D bounds
    bool Contains(Vector point);              // point on the plane and inside the outline
    bool Contains(Vector point, double tolerance);
}

/// A solid shape enclosing a volume in 3D space.
public interface ISolidShape
{
    double Volume      { get; }
    double SurfaceArea { get; }
    Vector Centroid    { get; }
    Box    BoundingBox { get; }
    bool Contains(Vector point);
    bool Contains(Vector point, double tolerance);
}
```

Each concrete shape also gets the full Vector-style surface: constructors + named factories,
equality/tolerance, `ToString`, operators where meaningful, and shape-specific maths (closest point,
intersection, transforms). Transforms return a new shape (immutability): `Translate(Vector)`,
`RotateAbout(Vector pivot, Quaternion)`, `Scale(double)`, `TransformedBy(Pose)`.

`Box` (an axis-aligned 3D bounding box) is needed early because it is the `BoundingBox` return type for
both contracts — build it alongside the first shape.

---

# Phased plan

**Phase 0 — Foundations / decisions**
- Resolve the Axis→rotation convention (decision 1).
- Decide `VectorPair`'s fate (decision 3); move geometry types out of `TestStuff/` (decision 2).
- 3D-only confirmed (decision 4); confirm centre-anchor + `Pose` + planar/solid contracts above.

**Phase 1 — Finish the near-complete core**
- `Quaternion` ✅ done (Hamilton product, conjugate, inverse, normalize, `FromAxisAngle`/`ToAxisAngle`,
  `FromYawPitchRoll`, `ToMatrix`, `Rotate`, `Slerp`/`Lerp`, equality+tolerance, `ToString`,
  deconstruction, constants, 23 tests).
- `Rotation` / `Attitude` ✅ done (Euler X/Y/Z and yaw/pitch/roll front-door types; `ToQuaternion`/
  `FromQuaternion`, `ToMatrix`, `Rotate`, the `Rotation`↔`Attitude` bridge, equality+tolerance,
  `ToString`, deconstruction; the old `Attitude` factory bug is gone). 
- `Pose` ✅ done (position + orientation; accepts `Quaternion`/`Rotation`/`Attitude`, stores a normalized
  quaternion; `Apply`/`ApplyDirection`/`ApplyInverse`, `Compose`/`*`/`Inverse`, `ToMatrix`,
  `Translate`/`RotateBy`/`With…`, equality+tolerance, `ToString`, deconstruction).
- `Plane` ✅ done (normal / signed+unsigned `DistanceTo`, `ClosestPoint`, `Reflect`, `SideOf`, line
  intersection `TryIntersectLine`/`IntersectLineParameter`, `FromPointNormal`/`FromThreePoints`,
  `Normalize`, parallel/degenerate predicates, scale-and-direction-invariant geometric equality,
  `ToString`, deconstruction, constants, 20 tests). Line intersection is expressed parametrically
  `(point, direction)` so `Ray`/`LineSegment` can delegate once built out.
- Raise `Axis`, `Matrix`, `Angle` test coverage toward `Vector` level.
- Enhancement ✅ done (deliberate divergence from upstream Vector repo): added `Angle`-typed
  `Yaw`/`Pitch`/`Roll` and `RotateX`/`RotateY`/`RotateZ` overloads (static + instance) to `Vector`, so it
  speaks the `Angle` vocabulary directly instead of relying on the implicit `Angle`→`double` conversion.
  Overload resolution is unambiguous (a `double`/`int` still binds to the radian form); 12 tests.
  Confirmed no such overloads were lost in the rename — current `Vector.cs` matched the upstream
  `Vector3.cs` public API exactly; this genuinely extends it.

**Phase 2 — Geometry shapes (headline goal)**
- Add **`Box`** (axis-aligned bounds) — the `BoundingBox` return type.
- Define `IPlanarShape` / `ISolidShape`.
- Promote `LineSegment`, `Ray`, `Chord` to full rich types (closest point, intersections, parametric
  evaluation, equality, `ToString`).
- Build the centre-anchored shape family — planar first (`Circle`, `Rectangle`, `Triangle`, `Ellipse`),
  then solid (`Sphere`, `Box` ops). Each implements its contract and the Vector model.

**Phase 3 — Queries + polish**
- Cross-shape queries: intersection/overlap, distance, closest point, bounding volumes.
- Visualizer pages for the new types.
- Consolidate shared conventions (single tolerance source, common formatting); document the house
  style for contributors.

## Testing conventions

MSTest + FluentAssertions. Test names: `Member_Condition_ExpectedResult_Test`; tag with
`[TestCategory(...)]`. Each new rich type gets a dedicated `<Type>Tests.cs` covering construction,
operators, core operations, edge cases (NaN/zero/degenerate), equality/tolerance, and formatting.
