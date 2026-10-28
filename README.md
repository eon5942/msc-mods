# Quick Save (Y)

A tiny [My Summer Car](https://store.steampowered.com/app/516750/) mod that lets you save the game at any time by pressing **Y**.

No menu, no dependencies beyond the mod loader, nothing to configure. Press `Y`, the game saves.

| | |
|---|---|
| Mod name | Quick Save (Y) |
| Mod ID | `QuickSave` |
| Version | 2.2 |
| Author | eon |
| Works with | My Summer Car (MSCLoader) |

## How it works

The mod finds the game's save FSM (the `SAVEGAME` PlayMaker event) and broadcasts it on keypress. It does the same thing the in-game save triggers do, so saves are fully compatible with normal saves.

---

## Requirements

- **My Summer Car** (Steam, Windows)
- **MSCLoader** — the mod loader for My Summer Car (see below)

The game is Windows-only. On Linux / Steam Deck it runs through Proton, and MSCLoader works there too.

---

## 1. Install MSCLoader

If you already run other MSCLoader mods, skip to step 2.

1. Download the latest MSCLoader from the official releases page:
   https://github.com/piotrulos/MSCModLoader/releases
2. Extract the zip to a folder of its own.
3. Run `MSCLInstaller.exe` (or `MSCPatcher.exe` on older builds).
4. In the installer, pick your My Summer Car install and click **Install MSCLoader**.
5. Wait for it to finish. It creates a `Mods` folder inside the game directory and patches the game to load mods.

> Confirm it worked: the game folder should now contain `winhttp.dll`, `doorstop_config.ini`, and a `Mods` folder.

## 2. Install this mod

1. Download `QuickSave.dll` from this repository.
2. Copy it into the game's `Mods` folder:

| Platform | Mods folder |
|---|---|
| Windows | `C:\Program Files (x86)\Steam\steamapps\common\My Summer Car\Mods\` |
| Linux / Steam Deck (Proton) | `~/.local/share/Steam/steamapps/common/My Summer Car/Mods/` |

The folder may be elsewhere if you installed Steam to a different drive — just look for the `My Summer Car\Mods` directory.

3. Launch the game.

## 3. Use it

Press **Y** to save. That's it.

There are no settings and no other keys. The key is fixed to `Y` in this build.

## 4. Verify it's loaded

1. Launch the game and open the MSCLoader mod menu.
2. You should see **Quick Save (Y)** listed under Mods, enabled.

If you want to check it from the file system, press `Y` in-game and confirm a new save appears, or look at `output_log.txt` in the game folder for the line:

```
QuickSave: broadcast SAVEGAME
```

---

## Troubleshooting

**Mod doesn't show up in the mod menu**
- Confirm `QuickSave.dll` is directly inside `My Summer Car\Mods\` (not in a subfolder).
- Confirm MSCLoader is installed and working (other mods load).

**Mod listed but saving does nothing**
- Make sure you are past the main menu and actually in-game. The save FSM only exists during gameplay.
- Check `output_log.txt` for `QuickSave:` messages or errors.

**Errors in the log (`NullReferenceException`, etc.)**
- This build targets the current MSCLoader API. If the game updated recently, the mod may need a rebuild — re-download the latest from here.

---

## Building from source

Source is not published here yet. If you have the project and want to build it:

1. Open the solution in Visual Studio (or build with `msbuild`).
2. Reference the MSCLoader assemblies (`MSCLoader.dll`, `MSCLoader.References.dll`) from your MSCLoader install's `Mods\References` folder.
3. Set the output to `QuickSave.dll` and copy it into `Mods\`.
