# RP.Math

A C# math library of **simple, immutable data objects that expose extensive mathematical
functionality** — small value types that hold minimal state but provide a rich, discoverable API.
The library grew out of a standalone `Vector3` type (see [History](#history)); that type is now
`Vector`, and the same design is being extended across a family of mathematical and geometric types.

Open **`Math.sln`**. See **`ROADMAP.md`** for the design direction and planned work.

## Projects

| Project              | Kind                              | Description                                                  |
| -------------------- | --------------------------------- | ------------------------------------------------------------ |
| `RP.Math`            | library (`net8.0`; `netstandard2.0`) | The core types (below).                                    |
| `RP.Math.Tests`      | MSTest + FluentAssertions         | Unit tests for the library.                                  |
| `RP.Math.Visualizer` | Blazor WebAssembly app            | Interactive visualizer that runs the real compiled library.  |

```sh
dotnet build Math.sln
dotnet test  RP.Math.Tests/RP.Math.Tests.csproj
dotnet run --project RP.Math.Visualizer
```

## The types

**Primitives & algebra**
- **`Vector`** — an immutable 3D vector `(x, y, z)` with an extensive API: arithmetic operators,
  dot/cross/mixed products, normalization (strict and safe variants), interpolation and slerp,
  projection/rejection/reflection, rotation, component-wise min/max/clamp, rounding, tolerance-aware
  equality/comparison, predicates, formatting, deconstruction and tuple conversion. This is the
  exemplar the other types follow.
- **`Angle`** — an immutable angle stored in radians, with degree/gradian access, full operator set,
  trigonometry, classification (acute/obtuse/reflex/right) and identities.
- **`Axis`** / **`OrthogonalAxis`** — Cartesian axis alignment and an orthogonal right/up/forward basis.
- **`Matrix`** — a 4×4 homogeneous transform matrix (multiply, transpose, determinant, inverse,
  translation/scaling/rotation factories).

**Orientation & placement**
- **`Quaternion`** — the robust orientation representation: Hamilton product, conjugate, inverse,
  normalize, `FromAxisAngle`/`ToAxisAngle`, `FromYawPitchRoll`, `ToMatrix`, `Rotate`, `Slerp`/`Lerp`.
- **`Rotation`** (Euler X/Y/Z) and **`Attitude`** (yaw/pitch/roll) — the human-friendly "front doors"
  for orientation; both convert to/from `Quaternion`.
- **`Pose`** — a rigid placement: a position (`Vector`) plus an orientation. It **accepts**
  `Quaternion`, `Rotation` or `Attitude`, but **stores a normalized `Quaternion` internally** (the
  mathematically correct form). Provides `Apply`/`ApplyInverse`, `Compose`, `Inverse` and `ToMatrix`.

**Geometry** (early state — being built out; see `ROADMAP.md`)
- `Plane`, `LineSegment`, `Ray`, `Chord`, `Rectangle`, and a planned shape family (`Circle`,
  `Triangle`, `Sphere`, `Box`, …).

**Numeric helpers**
- `DoubleExtension`, `ExpandedDouble`, `UlongExtension`, `Tolerance` — ULP / tolerance-based
  floating-point comparison underpinning the equality semantics throughout.

## Design (the "house style")

Every rich type aims to be: an immutable `struct`; `IEquatable<T>` + `IFormattable` (and `IComparable`
where ordering is meaningful); both **static and instance** forms of each operation; **tolerance-aware**
equality; deliberate NaN/degenerate handling (a strict variant that throws and a safe `…OrDefault`
variant); useful constructors, static factories and constants; `ToString(format, provider)`,
`GetHashCode`, `==`/`!=`, deconstruction and tuple / related-type conversions. `ROADMAP.md` has the full
template and the layered architecture for geometry (primitives → orientation → `Pose` → shapes).

## ⚠️ Provenance

`Vector` (originally `Vector3`), its supporting numeric helpers, the tests and the visualizer were
copied from the standalone **Vector** repository:

> **https://github.com/dickiepotter/Vector.git**

They were previously embedded as a git subtree; that subtree has been **removed** and the code is **no
longer synced**. When working on those files, **check the Vector repository for newer versions first** —
there is no automatic sync, so bring changes across by hand.

## Status

`Vector`, `Angle`, `Matrix`, `Quaternion`, `Rotation`, `Attitude` and `Pose` are complete and tested.
The geometry types are in an early state. The suite currently passes (a couple of inherited tests are
skipped). Work is tracked in `ROADMAP.md`.

## History

This library began as a CodeProject article building a reusable `Vector3` type from first principles.
That long-form tutorial (operator overloading, products, normalization, interpolation, etc.) now lives
in the **Vector** repository linked above; this repository carries the type forward as `Vector` and
extends the approach to the wider type family described here.

## Acknowledgements

- [CSOpenGL](http://sourceforge.net/projects/csopengl/) — Lucas Viñas Livschitz
- [Exocortex](http://www.exocortex.org/) — Ben Houston
- *Essential Mathematics for Computer Graphics* — John Vince (ISBN 1-85233-380-4)
