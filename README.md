# 📖 Codex

> ⚠️ **Experimental Project** - This is a personal experiment and learning project, not intended for production use.

Codex is an interactive fiction engine inspired by classic "Choose Your Own Adventure" books and "Fighting Fantasy" style gamebooks. It allows you to create and play text-based adventure games with branching narratives, dice rolls, combat, and player stats.

## ✨ Features

- 📜 **YAML-based story format** - Write adventures in a simple, human-readable format
- 🎲 **Dice rolling system** - Built-in dice functions for skill checks, combat, and random events
- ⚔️ **Fighting Fantasy mechanics** - Support for classic attributes like Skill, Stamina, and Luck
- 🔀 **Branching narratives** - Create complex story paths with conditional options
- 📝 **Embedded Lua scripting** - Add custom logic and game mechanics
- 🌐 **Runs in the browser** - Built with .NET and WebAssembly (WASM)

## 🛠️ Technology Stack

- **.NET 10** - Core runtime
- **WebAssembly (WASM)** - Browser-based execution WebAssembly (without Blazor 🤢)
- **Lua** - Embedded scripting for game logic
- **YAML** - Story definition format

## 📁 Project Structure

```
codex/
├── compiler/     # Story compiler - parses YAML and builds story objects
├── console/      # Console test harness for local development
├── frontend/     # WebAssembly frontend for browser play
│   └── wwwroot/
│       └── stories/  # Example story files
```

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Installation

```bash
make install
```

### Running Locally

**Console mode** (for testing):

```bash
make run
```

**Web mode** (browser):

```bash
make run-web
```

Then open your browser to the displayed URL.

## 📖 Story Format

Stories are written in YAML with sections, options, and embedded Lua for game logic:

```yaml
title: My Adventure
author: Your Name

vars:
  skill: 10
  stamina: 20
  gold: 0

sections:
  start:
    title: "The Beginning"
    text: "You stand at a crossroads..."
    options:
      go_north:
        text: "Head north"
        goto: forest
      go_south:
        text: "Head south"
        if: skill > 8
        goto: mountains
```

### Key Concepts

- **Sections** - Named story segments with text and options
- **Options** - Choices available to the player, can be conditional
- **Variables** - Track player stats, inventory, and game state
- **Lua scripting** - Custom logic via `run:` and `if:` blocks
- **Dice functions** - `d(6)` for a d6 roll, `dice(sides, count, modifier)` for complex rolls

For complete documentation on writing stories, including all available functions, option properties, and variable scopes, see the [Story Development Guide](story-dev.md).

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
