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

## Status (initial commit)

- Models: Game, Category, Segment, Run, SplitTime, Mistake, Strategy
- `StatsService`: PB, best segment, sum of best, average, consistency (std dev)
- Pages: Dashboard, Games (add games / categories / segments), Practice (timer, splits, mistake log)
- In-memory `PracticeStore` seeded with a demo game

## Roadmap

- Persist data (localStorage/IndexedDB or API)
- Strategy notes per segment UI
- Run history + charts, import/export (LiveSplit .lss)
- Edit/delete games, categories, segments
