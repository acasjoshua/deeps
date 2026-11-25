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
