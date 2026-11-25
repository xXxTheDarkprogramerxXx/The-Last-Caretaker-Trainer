# Voyage Trainer — The Last Caretaker
![Trainer](https://i.imgur.com/2APS2Yr.png)

**A standalone trainer for The Last Caretaker on Windows.**

Voyage Trainer gives you more control over your single-player experience, from unlimited health and battery power to faster movement, free crafting, weather control and vehicle assistance.

It includes a classic desktop trainer, an optional in-game overlay and an interactive navigation map.

No WeMod, Cheat Engine or separate trainer service is required.

## System requirements

- **Operating system:** Windows 10 or 11, 64-bit
- **Game:** The Last Caretaker (PC)
- **Supported game build:** `862A560D`
- **Runtime:** .NET 8 Desktop Runtime (x64), unless included with the prebuilt release
- **Game mode:** Local single-player

**Important:** This trainer is designed for a specific version of the game. Future game updates may change how features work. Do not use it with an unsupported build.

## Getting started

Using the prebuilt trainer is straightforward.

1. Download the latest Voyage Trainer release and extract the ZIP file to a folder of your choice.
2. Launch **The Last Caretaker** and load your single-player save.
3. Run `VoyageTrainer.exe` from the extracted folder.
4. Allow the trainer to detect and attach to the running game.
5. Enable your preferred features by clicking their buttons or using the keyboard shortcuts.

You can leave the trainer open while playing. Most features can be enabled or disabled at any time.

If the trainer cannot attach, make sure the game is running, a save is loaded and both programs are running at the same Windows privilege level.

## Trainer controls

The trainer uses the numeric keypad and a few additional shortcuts.

**Num** refers to the number pad on the right-hand side of your keyboard, not the number row above the letters.

### Player and movement

| Shortcut | Feature | Description |
|---|---|---|
| Num 1 | Refill Health | Restore your health |
| Num 2 | Refill Battery | Recharge your electricity |
| Num 3 | Unlimited Health | Continually replenish health |
| Num 4 | Unlimited Battery | Keep your battery charged |
| Num 5 | Increased Carry Capacity | Carry up to 999 metric tonnes |
| Num 6 | Walking Speed | Adjust your walking speed |
| Num 7 | Swimming Speed | Adjust your swimming speed |
| Num 8 | Jump Height | Adjust your jumping ability |
| Num 9 | Gravity | Adjust the gravity effect |
| Num 0 | Game Speed | Speed up or slow down gameplay |
| Num + | Camera FOV | Adjust your field of view |

Movement, gravity, game speed and FOV can be fine-tuned using the sliders in the trainer window.

### Crafting and environment

| Shortcut | Feature | Description |
|---|---|---|
| Num − | Free & Instant Crafting | Remove material requirements and accelerate crafting |
| Num * | Clear Skies | Reduce rain, clouds and fog |
| Num / | Always Daytime | Keep the world in daylight |
| Ctrl + Num 1 | Calm Ocean | Reduce ocean wave intensity |
| Ctrl + Num 2 | Disable Lightning | Suppress thunder and lightning conditions |

**Crafting:** After enabling free crafting, you may need to close and reopen the crafting interface to refresh the available recipes.

Some crafting stations may still require power, available output space or other game-specific conditions.

**Weather:** These features modify the game's environment calculations. They are experimental and may require a weather refresh to become visible. If you notice unusual behaviour, disable the affected feature and restart the game.

### Vehicles and survival

| Shortcut | Feature | Description |
|---|---|---|
| Ctrl + Num 3 | Refill Boat Fuel | Replenish boat fuel |
| Ctrl + Num 4 | Unlimited Boat Fuel | Prevent boat fuel from running out |
| Ctrl + Num 5 | Unlimited Boat Electricity | Keep boat electricity supplied |
| Ctrl + Num 6 | Maximum Boat Throttle | Maintain maximum boat throttle |
| Ctrl + Num 7 | Unlimited Human Seed Lifetime | Prevent human seeds from expiring |

Boat-specific controls are intended for supported boats and their systems. They do not automatically apply to every vehicle in the game.

**Unlimited Human Seed Lifetime:** This feature is intended to stop the remaining lifetime countdown on human seeds. It does not revive seeds that have already expired, and it is separate from human growth and development.

## In-game overlay

Voyage Trainer includes a floating interface that can be displayed over the game.

This allows you to view and adjust supported trainer features without repeatedly switching to the desktop window.

| Shortcut | Action |
|---|---|
| Alt + W | Show or hide the trainer overlay |
| Alt + E | Enable or disable overlay editing |

The in-game overlay separates controls into three categories:

- **Player:** Health, battery, movement, crafting and character settings
- **World:** Weather and environmental settings
- **Vessel:** Boat-related controls

When editing is enabled, overlay panels can be repositioned. Once you're happy with their positions, switch off editing to return them to their normal non-interactive state.

The overlay is intended primarily for windowed and borderless-windowed gameplay. Fullscreen exclusive mode may prevent it from appearing correctly.

## Minimap and world map

The trainer also includes an external navigation overlay.

| Shortcut | Action |
|---|---|
| Alt + M | Show or hide the minimap |
| Alt + N | Change the minimap zoom |
| Alt + G | Open or close the larger world map |
| Alt + E | Unlock map interaction and repositioning |

The minimap is designed to display your current position and heading using information read from the game.

Depending on the available game data, it may also display nearby points of interest.

The larger world map provides more space for navigation and exploration, including zooming and panning.

**Note:** This is a trainer-generated navigation display, not a copy of the game's original illustrated world map. Map data and markers may be incomplete, particularly in unexplored areas.

The minimap is intended to appear automatically when the game is detected, with Alt + M available to change its visibility.

## Switching vehicles

Voyage Trainer is designed to follow your player character even when you take control of a vehicle.

This includes support for vehicles that use the game's shared vehicle system, such as boats and the gyrocopter.

If the game introduces additional vehicles in future updates, player tracking may continue to work where those vehicles use the same underlying character and possession structure.

However, vehicle-specific functions such as fuel, throttle or electrical systems may require additional support.

If the trainer temporarily loses your player during a vehicle transition, allow it to recover or use **Attach / Resolve** from the main trainer window.

## Disabling features

Press **Home** to request that the trainer disable its active modifications and restore original values where supported.

You can also turn individual features off using their original shortcuts or buttons.

Always close Voyage Trainer normally before exiting the game whenever possible.

If either the trainer or the game crashes while a feature is active, restart the game before using the trainer again.

## Troubleshooting

### The trainer cannot find the game

Make sure:

- The game has started and a single-player save is loaded.
- Only one copy of the game is running.
- You are using the supported game build.
- The trainer has sufficient permission to access the game's process.

Try restarting both programs if the problem continues.

### A shortcut does not work

Check that the game window is focused and Num Lock is enabled.

Some keyboards require additional keys to access the numeric keypad.

You can use the trainer's desktop buttons instead.

### A feature switches on but has no effect

Some game values are recalculated continuously, while others are only refreshed when a particular action occurs.

Try reopening the relevant game menu or interacting with the affected object again.

You can also disable and re-enable the feature.

If the problem persists, restart the game. Do not repeatedly toggle a feature that is causing instability.

### The overlay or minimap is missing

Try running the game in borderless-windowed mode.

Make sure the game is focused, then use Alt + W or Alt + M.

If the display appears behind the game or becomes unresponsive, restart the trainer.

### The trainer stops responding after changing vehicles or loading a save

Some transitions temporarily invalidate the current player or world references.

The trainer is designed to recover from temporary changes. If it remains disconnected, use **Attach / Resolve** or restart it after the save has loaded.

### Windows reports a missing runtime

Install the official **.NET 8 Desktop Runtime (x64)** from Microsoft, unless your release includes the runtime.

### Windows or antivirus software displays a warning

Trainer applications access and modify another process's memory, which may trigger security warnings.

Download releases only from a trusted source. Verify the file before allowing it to run, and do not disable your computer's security protections globally.

## Important information

Voyage Trainer is an unofficial, independent utility
