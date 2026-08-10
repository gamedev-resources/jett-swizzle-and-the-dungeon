# Editor

Editor-only tooling: custom inspectors, property drawers, `EditorWindow`s, asset
post-processors, and menu items.

Unity treats any folder named `Editor` as special — everything in here compiles into the
editor assembly and is **stripped from player builds**. That means code in this folder can
freely `using UnityEditor;` without an `#if UNITY_EDITOR` guard, and runtime code must never
reference it.

The project currently has no editor tooling; this folder is the agreed home for the first
piece that lands.
