# Tests

- `EditMode/` — fast tests that run without entering play mode. Pure logic: the item factory,
  the event bus, state-machine transitions.
- `PlayMode/` — tests that need a running scene, physics steps, or coroutines.

> **Not wired up yet.** The project has no assembly definitions, so everything compiles into
> `Assembly-CSharp`. Unity's Test Framework only discovers tests inside an assembly that
> references `UnityEngine.TestRunner` / `UnityEditor.TestRunner`, so each of these folders
> needs its own `.asmdef` before a single test will run. That was deliberately deferred —
> adding assemblies means first breaking the existing Gameplay ↔ Visual dependency cycle.

Note: `Core/Cheats/CheatCommands.cs` is *not* a test. It is a runtime cheat console used
during manual play-testing.
