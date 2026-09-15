# Block Sort

A portrait, touch-first color sorting puzzle: move matching top block runs between wooden slots, pop completed stacks, and clear the shelf.

## Play the preview

The included browser preview is a complete, responsive first-playable slice with splash, home, level grid, 12 playable levels, undo, one extra-slot booster, confetti, coins, and local progress:

```bash
python3 -m http.server 41731 --directory .
```

Open `http://127.0.0.1:41731/preview/` on a phone-sized viewport or mobile device on the same network.

## Unity source

The primary game source is a Unity 6 portrait project:

1. Open this repository with Unity Hub using Unity `6000.0.0f1` or a compatible Unity 6 LTS editor.
2. Let Unity import packages and generated art.
3. Play or build for a portrait Android/iOS target.

The core, view-independent puzzle model is in `Assets/_Project/Code/Gameplay/BlockSortBoard.cs`; it has NUnit edit-mode coverage in `Assets/_Project/Tests/EditMode`.
Three `LevelDefinition` assets under `Assets/_Project/Data/Levels` demonstrate the data-driven level format.

## Blender art

The source-controlled generator at `tools/blender/generate_2d_assets.py` creates the rounded candy blocks, embossed icon plates, wood slot, and button ornament as transparent PNG sprites plus GLB source exports:

```bash
blender --background --python tools/blender/generate_2d_assets.py
```

Generated files are placed in `Assets/_Project/Art/Generated/` and are deliberately lightweight for mobile use.
