# What this project is — read before doing anything

## The goal (in the owner's words)

This is a **math library for computer science**, and its **main job is learning**. The maths is the
primary goal. Graphics obviously overlaps, but graphics is **not** the point — do not let this drift
into a graphics/game engine.

The approach: **simple data objects that represent extensive functionality.** Model basic mathematical
concepts — vectors, angles, axes, lines, planes, and basic geometric shapes — as small immutable types,
then give each one rich, well-explained mathematical functionality, the way the `Vector` class does.
The shapes are mathematical models (their area, perimeter, volume, "is a point inside", angles, …), not
rendering primitives.

It began as a first-principles tutorial that built `Vector` "as simple and understandable as possible,"
explaining the *why* at each step. Keep that spirit: code that teaches.

## How to work here (guardrails — these came from real mistakes)

1. **Maths first.** Before adding a type or method, ask: *is this a mathematical concept, or just an
   engineering/performance convenience?* Keep the former; be sceptical of the latter. (Example: a
   bounding box is a graphics/performance shortcut, not maths — it does not belong unless explicitly asked for.)
2. **Never introduce a concept or term without explaining it**, in plain language, ideally as it would
   be taught. No unexplained jargon or acronyms. If the owner wouldn't recognise it, define it first.
3. **Do not invent scope or "a plan" and then execute it.** Do one small, agreed step at a time, then
   stop and talk. "Continue" means continue the *agreed* thing, not a roadmap I made up.
4. **Do not build, test, or commit autonomously or repeatedly.** Only build/test/commit when asked.
   Keep terminal commands to a minimum — they flood the owner's screen with noise. Explain in prose instead.
5. **Check in often.** When unsure, ask rather than assume. The owner steers direction.

## Provenance note

`Vector` (and some supporting numeric files, the tests, and the visualizer) were copied from the
standalone Vector repo (https://github.com/dickiepotter/Vector.git) and are no longer synced — see
`readme.md`. `Vector` may be *deliberately* extended here (e.g. Angle-typed overloads) because this
library has a richer set of supporting types than the original.
