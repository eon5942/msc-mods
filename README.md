# Quick Save (Y)

A tiny [My Summer Car](https://store.steampowered.com/app/516750/) mod that lets you save the game at any time by pressing **Y**.

No menu, no dependencies beyond the mod loader, nothing to configure. Press `Y`, the game saves.

| | |
|---|---|
| Mod name | Quick Save (Y) |
| Mod ID | `QuickSave` |
| Version | 2.3 |
| Author | eon |
| Works with | My Summer Car (MSCLoader) |

## How it works

The mod triggers the game's own save by broadcasting the `SAVEGAME` PlayMaker event on keypress, exactly like the in-game save, so saves are fully compatible with normal saves.

MSC only ever saves when quitting, so broadcasting `SAVEGAME` mid-game leaves every object's state machine stuck in a terminal "Save" state with no exit. That is what broke eating/drinking and the player position in earlier versions. This version records each state machine's state before saving and restores it a moment later, so the game keeps running as if nothing happened.

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

If you want to check it from the file system, press `Y` in-game and confirm the save files (`defaultES2File.txt`, `items.txt`) in the game's save folder were just updated.

---

## Troubleshooting

**Mod doesn't show up in the mod menu**
- Confirm `QuickSave.dll` is directly inside `My Summer Car\Mods\` (not in a subfolder).
- Confirm MSCLoader is installed and working (other mods load).

**Mod listed but saving does nothing**
- Make sure you are past the main menu and actually in-game. The save FSM only exists during gameplay.

**Errors in the log (`NullReferenceException`, etc.)**
- This build targets the current MSCLoader API. If the game updated recently, the mod may need a rebuild — re-download the latest from here.

---

## Building from source

The full source is `QuickSave.cs`. Build it against the game's own assemblies (the DLLs in `My Summer Car\mysummercar_Data\Managed`) with Mono's `mcs`:

```
mcs -nostdlib -target:library -out:QuickSave.dll \
  -r:"<game>/mysummercar_Data/Managed/mscorlib.dll" \
  -r:"<game>/mysummercar_Data/Managed/System.dll" \
  -r:"<game>/mysummercar_Data/Managed/System.Core.dll" \
  -r:"<game>/mysummercar_Data/Managed/UnityEngine.dll" \
  -r:"<game>/mysummercar_Data/Managed/MSCLoader.dll" \
  -r:"<game>/mysummercar_Data/Managed/PlayMaker.dll" \
  QuickSave.cs
```

`<game>` is your `My Summer Car` install folder. Copy the resulting `QuickSave.dll` into `Mods\`.
