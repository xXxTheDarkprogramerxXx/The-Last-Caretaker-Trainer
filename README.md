# Voyage trainer v5

Windows x64, .NET 8 Desktop. Intended for a local single-player save on executable build `862A560D` only.

1. Close the previous trainer and restart the game to remove any lingering old patches.
2. Load your single-player save before launching the trainer. Start it with the same privilege level as the game.
3. Open `VoyageTrainer.csproj` in Visual Studio, select x64 Release, and run it. Or execute `dotnet build -c Release` and launch `bin/Release/net8.0-windows/VoyageTrainer.exe`.
4. Click a feature or use its F-key with the game focused. F12 can conflict with Steam screenshots; use its button if necessary. Close and reopen the crafting UI when toggling free crafting.
5. The top status line reads health and electricity from the live game, and Show addresses prints additional diagnostics. Enabled toggles pause rather than silently clearing on temporary player validation failure. Click Attach / resolve after a world change. Close the trainer normally to restore reversible patches and values.

The freeze loop targets 5 ms on its own worker, checks the player and resource allocation, and distinguishes successful memory writes from subsequent game updates. The status includes a successful freeze-write count and reasons for pausing. Optional movement/camera components do not invalidate the entire player. Windows compilation and gameplay testing are still required. Keep a backup of your save. Refill health is a repeated refill, not a guarantee against instant lethal damage.

F5 now targets 999,000 kg (999 metric tonnes). It changes the local character's carry bonus and the inventory's configured and effective limits. It does not alter a shared settings asset. The live status and Show addresses log report the effective capacity. Reopen the inventory or pick up an item if the in-game text is cached. Turn F5 off before exiting to restore the captured values.

## Editing the form

In Visual Studio, right-click `TrainerForm.cs` and choose **View Designer** (Shift+F7). Controls, layout, style, captions and the nonvisual timer are declared in `TrainerForm.Designer.cs`; edit them visually through the designer and Properties window. `TrainerForm.cs` contains event handlers, global hotkeys, status updates and attachment lifecycle only. `VoyageTrainer.cs` contains the unchanged trainer engine.

The F1–F12 controls have individual names and click handlers. Captions and off-state button colours are taken from the designer, so changing them there persists when the trainer refreshes its UI.
