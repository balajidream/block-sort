# Block Sort — Unity 6

Portrait hybrid-casual color sort. Open this **repository root** in Unity Hub (Unity 6 LTS, `6000.0`).

## Open once

1. Unity Hub → **Open** → this folder (the one with `Assets/`, `Packages/`, `ProjectSettings/`).
2. Install **iOS Build Support** if you plan to ship to a device.
3. Wait for Package Manager to import URP and uGUI.
4. Unity will run `Tools → Block Sort → Complete Project Setup` on first editor load (or run that menu yourself).
5. Open `Assets/_Project/Scenes/Boot.unity` and press **Play**.

If Play shows a blank camera, the runtime bootstrap still creates the app (`BlockSortApp`). Press Play on any scene.

## Layout

| Path | What lives there |
| --- | --- |
| `Assets/_Project/Code/Gameplay` | Pure board rules (`Board`, `Slot`, tap session) |
| `Assets/_Project/Code/Levels` | Level definitions, catalog, reverse generator, JSON import |
| `Assets/_Project/Code/Presentation` | Portrait UI shell that plays the puzzle |
| `Assets/_Project/Code/Meta` | Coins and cleared-level save |
| `Assets/_Project/Code/Core` | Tiny event bus / game states |
| `Assets/_Project/Data/Levels/World01` | Baked ScriptableObject levels (created by the setup menu) |
| `Assets/_Project/Resources/Levels/world-01.json` | Source of truth for the 12 hand levels |
| `Assets/_Project/Scenes` | `Boot`, `Meta`, `Game` |
| `Assets/_Project/Art/Generated` | Blender PNG + GLB cubes, slot, button |
| `Assets/_Project/Editor` | Setup, bake, generate menus |
| `preview/` | Web prototype used to tune feel (not required to Play in Unity) |

## Levels

World 01 is 12 authored puzzles (same layouts as the web preview). They import from JSON into `LevelDefinition` assets:

- `Tools → Block Sort → Bake 12 Hand Levels From JSON`
- `Tools → Block Sort → Generate Extra Practice Levels (13-30)` uses reverse generation (solved board, then legal reverse moves) so extras stay solvable.

Create more worlds as extra `LevelCatalog` assets and add them to `WorldDatabase`.

## iOS

After the project opens cleanly:

1. File → Build Settings → iOS → Switch Platform.
2. Player Settings already target portrait, IL2CPP, ARM64, bundle id `com.blocksort.game`.
3. Build, open the Xcode project, pick your team, run on a device.

## Blender

```bash
blender --background --python tools/blender/generate_2d_assets.py
```

Writes sprites and GLBs into `Assets/_Project/Art/Generated/`.
