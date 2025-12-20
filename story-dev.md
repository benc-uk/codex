# Story Development Guide

This document describes how to write story files for the Codex interactive fiction engine.

## File Format

Story files are written in **YAML** format with the `.yaml` extension. They are placed in the `frontend/wwwroot/stories/` directory.

## Story Structure

A story file has the following top-level properties:

```yaml
title: "Your Story Title"
author: "Author Name"
version: "1.0"

vars:
  # Global variables

init: |
  # Lua initialization code

sections:
  # Story sections/passages
```

### Top-Level Properties

| Property   | Required | Description                                                   |
| ---------- | -------- | ------------------------------------------------------------- |
| `title`    | No       | The title of the story displayed to the player                |
| `author`   | No       | Author name (for metadata)                                    |
| `version`  | No       | Version string (for metadata)                                 |
| `vars`     | No       | Initial global variables available throughout the story       |
| `init`     | No       | Lua code block run once when the story starts                 |
| `sections` | Yes      | Map of section IDs to section definitions (the story content) |

---

## Variables (`vars`)

Define initial state that persists across the entire story. Variables can be:

- **Numbers**: `stamina: 20`
- **Strings**: `player_name: "Hero"`
- **Booleans**: `has_key: false`
- **Arrays/Lists**: `bag: ["sword", "shield", "potion"]`

```yaml
vars:
  skill: 9
  luck: 5
  stamina: 20
  gold: 0
  bag: ["rusty dagger", "torch"]
  player_name: "Adventurer"
```

Variables are accessible and modifiable in any Lua code blocks using their name directly.

---

## Initialization Code (`init`)

The `init` block runs **once** when the story first loads. Use it to:

- Define custom Lua functions
- Set up random starting values
- Create helper utilities

```yaml
init: |
  function battle(name, enemy_skill, enemy_stamina)
    -- Custom battle logic
    return enemy_stamina
  end

  -- Randomize starting stats
  stamina = d(6) + 10
  skill = d(6) + 4
```

### Special Callback Function: `post_option()`

You can define a special function called `post_option()` in your init block. This function is called **after every option is selected** but before navigation occurs. If it returns a section ID string, the player will be redirected to that section instead.

This is useful for implementing death checks or other game-over conditions:

```yaml
init: |
  function post_option()
    if stamina <= 0 then
      return "dead"  -- Redirect to "dead" section
    end
    return nil  -- Continue normally
  end
```

---

## Sections

Sections are the core building blocks of your story. Each section represents a passage, scene, or location.

### Section Structure

```yaml
sections:
  section_id:
    title: "Optional Section Title"
    text: "The narrative text shown to the player."
    run: |
      -- Lua code run every time this section is entered
    run_once: |
      -- Lua code run only the first time this section is entered
    options:
      # Player choices
```

### Section Properties

| Property   | Required | Description                                           |
| ---------- | -------- | ----------------------------------------------------- |
| `title`    | No       | Display title for the section                         |
| `text`     | Yes      | Main narrative text (supports variable interpolation) |
| `run`      | No       | Lua code executed every time the section is visited   |
| `run_once` | No       | Lua code executed only on the first visit             |
| `options`  | No       | Map of option IDs to option definitions               |

### Reserved Section IDs

- **`start`** - Required. The entry point of your story. The player begins here.
- **`restart`** - Reserved keyword. Cannot be used as a section ID, but can be used as a `goto` target to restart the story.

### Section Variables

Each section has its own persistent variable namespace accessed via `section.*`:

```yaml
sections:
  treasure_room:
    text: "You see a chest. You've opened it {section.times_opened} times."
    run: |
      if not section.times_opened then
        section.times_opened = 0
      end
```

The special variable `section.visits` is automatically tracked and contains the number of times the current section has been visited.

---

## Options

Options are the player's choices. They define what actions are available and where they lead.

### Short Form

For simple navigation without conditions or side effects:

```yaml
options:
  go_north: ["Go north", cave_entrance]
  go_south: ["Head south", forest_path]
```

Format: `option_id: ["Display Text", target_section_id]`

### Long Form

For options with conditions, side effects, or notifications:

```yaml
options:
  option_id:
    text: "Display text for the option"
    goto: target_section_id
    if: lua_condition_expression
    run: |
      -- Lua code to execute when selected
    notify: "Message shown to the player after selection"
```

### Option Properties

| Property | Required | Description                                                          |
| -------- | -------- | -------------------------------------------------------------------- |
| `text`   | Yes      | The text displayed to the player for this choice                     |
| `goto`   | Yes      | The section ID to navigate to (or `restart` to restart the story)    |
| `if`     | No       | Lua expression that must be truthy for the option to appear          |
| `run`    | No       | Lua code executed when the option is selected (before navigation)    |
| `notify` | No       | Message displayed to the player after selecting (supports variables) |

### Conditional Options (`if`)

The `if` property takes a Lua expression. The option only appears if the expression evaluates to `true`:

```yaml
options:
  use_key:
    text: "Unlock the door"
    if: contains(bag, "golden key")
    goto: treasure_room

  buy_sword:
    text: "Purchase the sword (10 gold)"
    if: gold >= 10
    run: gold = gold - 10; insert(bag, "sword")
    goto: shop

  flee:
    text: "Run away!"
    if: section.visits > 3 and enemy_hp > 0
    goto: forest
```

### Option Execution Order

When a player selects an option, the following occurs in order:

1. The `run` code executes (if present)
2. The `post_option()` callback is called (if defined in `init`)
3. The `notify` message is displayed (if present, supports variable interpolation)
4. Navigation to the `goto` target occurs

### Using `restart`

To restart the story from the beginning (resetting all state):

```yaml
options:
  play_again:
    text: "Start a new adventure"
    goto: restart
```

---

## Variable Interpolation

Use `{variable_name}` syntax in `text` and `notify` strings to display variable values:

```yaml
text: "You have {gold} gold coins and {stamina} stamina remaining."
notify: "You found {temp.gold} gold pieces!"
```

Supports:

- Global variables: `{gold}`, `{player_name}`
- Section variables: `{section.enemy_hp}`
- Temporary variables: `{temp.damage}`

---

## Built-in Lua Functions

These functions are available in all Lua code blocks (`init`, `run`, `run_once`, option `run`, and `if` conditions).

### Dice Rolling

| Function                       | Description                                          |
| ------------------------------ | ---------------------------------------------------- |
| `d(sides)`                     | Roll a single die with the specified number of sides |
| `dice(sides, count, modifier)` | Roll multiple dice and add a modifier                |

```lua
local attack = d(6) + d(6) + skill      -- 2d6 + skill
local damage = dice(8, 3, 2)            -- 3d8 + 2
```

### Container/Inventory Functions

| Function                      | Description                                           |
| ----------------------------- | ----------------------------------------------------- |
| `contains(container, item)`   | Returns `true` if the container has the item          |
| `insert(container, item)`     | Adds an item to the container                         |
| `remove(container, item)`     | Removes the first occurrence of item from container   |
| `remove_all(container, item)` | Removes all occurrences of item from container        |
| `count(container, item)`      | Returns the number of times item appears in container |

```lua
if contains(bag, "torch") then
  insert(bag, "lit torch")
  remove(bag, "torch")
end

if count(bag, "potion") >= 2 then
  -- Player has at least 2 potions
end
```

### Output Functions

| Function          | Description                                  |
| ----------------- | -------------------------------------------- |
| `notify(message)` | Display a notification message to the player |
| `print(...)`      | Debug output (for development)               |

### Standard Lua Libraries

The following Lua standard libraries are available:

- `math` - Math functions (`math.max`, `math.min`, `math.floor`, `math.random`, etc.)
- `string` - String manipulation
- `table` - Table utilities

---

## Variable Scopes

### Global Variables

Defined in `vars` or created in `init`. Persist for the entire game session.

```lua
gold = gold + 10
stamina = math.max(0, stamina - 5)
```

### Section Variables (`section.*`)

Persist only for the current section. Reset when entering a different section but preserved if you return.

```lua
section.searched = true
section.enemy_hp = 15
```

### Temporary Variables (`temp.*`)

Reset at the start of each section. Use for calculations and intermediate values.

```lua
temp.damage = d(6) + strength
temp.gold_found = d(10) + d(10)
```

---

## Complete Example

```yaml
title: The Dark Forest
author: Example Author
version: 1.0

vars:
  health: 20
  gold: 5
  bag: ["dagger", "bread"]

init: |
  function post_option()
    if health <= 0 then
      return "game_over"
    end
    return nil
  end

sections:
  start:
    title: "Forest Entrance"
    text: "You stand at the edge of a dark forest. The trees loom menacingly overhead."
    options:
      enter: ["Enter the forest", deep_woods]
      search:
        text: "Search the ground"
        if: not section.searched
        run: |
          section.searched = true
          temp.coins = d(4)
          gold = gold + temp.coins
        notify: "You find {temp.coins} gold coins hidden in the leaves!"
        goto: start

  deep_woods:
    title: "Deep in the Woods"
    text: "The path splits. You hear growling to the east."
    run_once: section.wolf_hp = 8
    options:
      west: ["Go west (safe path)", clearing]
      east:
        text: "Go east (toward the growling)"
        goto: wolf_encounter
      eat_bread:
        text: "Eat your bread (+5 health)"
        if: contains(bag, "bread")
        run: |
          remove(bag, "bread")
          health = math.min(20, health + 5)
        notify: "You eat the bread and feel restored."
        goto: deep_woods

  wolf_encounter:
    title: "Wolf Attack!"
    text: "A fierce wolf blocks your path! It has {section.wolf_hp} HP remaining."
    run: section.wolf_hp = section.wolf_hp or 8
    options:
      fight:
        text: "Attack the wolf"
        if: section.wolf_hp > 0
        run: |
          temp.damage = d(6)
          section.wolf_hp = section.wolf_hp - temp.damage
          if d(6) <= 2 then
            health = health - 3
          end
        notify: "You deal {temp.damage} damage!"
        goto: wolf_encounter
      victory:
        text: "Continue on"
        if: section.wolf_hp <= 0
        notify: "You defeated the wolf!"
        goto: clearing
      flee:
        text: "Run away!"
        run: health = health - 2
        notify: "You escape but take 2 damage fleeing!"
        goto: deep_woods

  clearing:
    title: "Peaceful Clearing"
    text: "You emerge into a sunlit clearing. You've made it through the forest!"
    options:
      restart: ["Play again", restart]

  game_over:
    title: "Game Over"
    text: "Your health has dropped to zero. The forest claims another victim."
    options:
      restart: ["Try again", restart]
```

---

## Tips and Best Practices

1. **Always have a `start` section** - This is where your story begins.

2. **Use `run_once` for initialization** - Set up section-specific variables on first visit.

3. **Use `temp.*` for calculations** - Avoid polluting global scope with temporary values.

4. **Test conditional options** - Make sure players can always progress (avoid softlocks).

5. **Use `section.visits`** - Create content that changes based on repeat visits.

6. **Keep option `if` conditions simple** - Complex logic should go in `run` blocks or `init` functions.

7. **Use `post_option()` for global checks** - Death conditions, time limits, etc.

8. **Use `notify` for feedback** - Let players know the effect of their choices.
