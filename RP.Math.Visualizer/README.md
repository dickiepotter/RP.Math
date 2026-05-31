# RP.Math — Interactive Visualizer

A small **Blazor WebAssembly** app for exploring the `RP.Math` library. It runs the
**actual compiled library** in the browser, so the visualisation and every result reflect the
real code — not a re-implementation.

![Operations covered](https://img.shields.io/badge/operations-38-4be0a8)

## What it does

- Pick any operation (add, dot, cross, projection, rejection, reflection, rotations, interpolate,
  predicates, …) from a grouped list — it covers the public surface of the `Vector` type.
- Adjust vectors **A**, **B**, **C** with sliders/number boxes, or **drag a vector's tip** in the
  3D scene. Drag the background to **orbit**, scroll to **zoom**.
- See the result recompute live: the result vector is drawn in the scene, and scalars / booleans /
  integers are shown in the result card alongside the formula and a plain-English explanation.

## Run it

```bash
dotnet run --project RP.Math.Visualizer
```

Then open the printed `http://localhost:<port>` URL.

## Publish (static site)

```bash
dotnet publish RP.Math.Visualizer -c Release
```

The output under `bin/Release/net8.0/publish/wwwroot` is a static site that can be hosted anywhere
(GitHub Pages, any static host).

## How it's wired

- `Operations.cs` — a data-driven catalogue of every operation, each with a `Func<OpContext, OpResult>`
  that calls the real `Vector` method.
- `Geometry.cs` — a tiny orthographic camera (kept independent of the library) that projects the 3D
  scene onto the SVG canvas.
- `Components/VectorEditor.razor` — the reusable slider/number editor for a vector.
- `Pages/Home.razor` — the scene (inline SVG) and the control panel.
