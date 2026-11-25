# DEEPS - Speedrun Practice Tracker

Players break games into segments, record split times, mistakes, strategies, PBs and
consistency statistics. Built specifically for speedrunners.

**Stack:** C# / .NET 8, Blazor WebAssembly, Tailwind CSS

## Run

```bash
npm install          # once, installs Tailwind
npm run build:css    # (also runs automatically on dotnet build once node_modules exists)
dotnet run
```

For live CSS rebuilds during development: `npm run watch:css` in a second terminal.

## VS Code

1. Install the recommended extensions (C# Dev Kit, Tailwind CSS IntelliSense) and the .NET 8 SDK + Node.js.
2. Open the `deeps` folder in VS Code.
3. Press **F5** and pick **DEEPS: Debug in browser** (installs npm packages, builds CSS, launches Chrome with the debugger attached at http://localhost:5080).
   Or pick **DEEPS: Run without debugging** for `dotnet watch` hot reload.
4. Optional: run the **css: watch** task (Terminal > Run Task) to rebuild Tailwind as you edit.

## Features

- Manage games, categories, segments, goals, and strategies.
- Practice with a timer, splits, comparisons, mistake and run notes, pause/resume,
  undo/skip, countdown, segment loop mode, and keyboard/gamepad controls.
- Persist practice data and settings in browser localStorage.
- Switch between dark and light themes from Settings. The dark theme uses a
  plum-violet palette with warm pink/coral accents and a bundled Montserrat
  font with system sans-serif fallbacks.
- Review run history and statistics, including personal-best progression and
  segment consistency.
- Import and export JSON, CSV, and LiveSplit `.lss` data.
- Configure hotkeys, gamepad buttons, timing method, and timer display settings.
- Show a live, transparent overlay for a practice category in another tab in
  the same browser.

## Overlay and OBS

Open a category's Practice page and select **OBS Overlay**; it opens the
transparent overlay in a new tab. The overlay uses `BroadcastChannel`, so the
Practice page and overlay need to be open in the same browser profile. OBS
Browser Source runs in a separate browser context and may not receive those
updates; OBS integration has not been tested. To capture the overlay while
streaming, use Window Capture on the browser showing both tabs.

## Tests

Run the unit tests with:

```bash
dotnet test Deeps.sln
```

Current unit tests cover `StatsService`. Browser persistence and import/export
are not yet covered by automated tests.

## Possible future work

- Expand automated tests for persistence and import/export.
