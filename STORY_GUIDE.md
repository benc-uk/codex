# Codex Story Development Guide

A comprehensive guide for creating interactive stories using the Codex engine.

## Table of Contents

- [Introduction](#introduction)
- [Story File Format](#story-file-format)
- [Story Structure](#story-structure)
- [Variables and State](#variables-and-state)
- [Sections](#sections)
- [Options](#options)
- [Lua Scripting](#lua-scripting)
- [Events System](#events-system)
- [Hooks](#hooks)
- [Templates and Reusability](#templates-and-reusability)
- [Best Practices](#best-practices)
- [Complete Examples](#complete-examples)

## Introduction

Codex stories are written in YAML format with embedded Lua code for game logic. This guide will teach you everything you need to know to create engaging, interactive narratives with complex mechanics.

### What You Can Build

- Choose-your-own-adventure stories
- RPG-style adventures with stats and combat
- Puzzle games with inventory mechanics
- Interactive fiction with branching narratives
- Gamebook-style experiences (Fighting Fantasy, Lone Wolf, etc.)

### Prerequisites

- Basic understanding of YAML syntax
- Basic Lua programming knowledge (helpful but not required)
- A text editor
- Codex running locally (see main README for setup)

## Story File Format

Stories are YAML files placed in `web/public/stories/`. The filename (without `.yaml`) becomes the story URL.

**Example**: `web/public/stories/dragon-quest.yaml` → `http://localhost:5173/dragon-quest`

### Basic Template

```yaml
title: Your Story Title
system: fighting-fantasy
author: Your Name
version: 1.0

vars:
  # Global variables

init: |
  # Lua initialization code

sections:
  start:
    text: 'Story begins here...'
    options:
      # Player choices
```

## Story Structure

### Metadata

```yaml
title: The Lost Kingdom # Story title (shown in browser)
system: fighting-fantasy # UI system/theme for rendering (required)
author: Jane Doe # Author name
version: 1.0 # Version number
```

### System Field

The `system` field is a **required** top-level field that specifies which UI system/theme is used to render the story. Each system provides its own HTML template and CSS styling appropriate for different genres.

Systems are located in `web/sys/`, where each subdirectory contains the HTML and CSS for that system. You can create custom systems by adding a new folder with your own `index.html` and `style.css`.

**Built-in systems**:

| System             | Description                                          |
| ------------------ | ---------------------------------------------------- |
| `fighting-fantasy` | Classic gamebook style with skill/stamina/luck stats |
| `sci-fi`           | Science fiction themed interface                     |

**Example**:

```yaml
title: Space Station Adventure
system: sci-fi
author: Your Name
```

The system determines the visual presentation and may include genre-specific UI elements for displaying stats, inventory, and other game state.

### Global Variables (`vars`)

Define initial values for variables that persist throughout the story:

```yaml
vars:
  # Character stats
  skill: 8
  stamina: 20
  luck: 12

  # Derived values
  skill_init: 8
  stamina_init: 20
  luck_init: 12

  # Inventory
  gold: 10
  equip: ['sword', 'leather_armor']
  consumables: ['healing_potion', 'healing_potion']

  # Story flags
  met_wizard: false
  dragon_defeated: false

  # Player info
  player_name: 'Hero'
```

### Initialization Code (`init`)

Lua code that runs once when the story loads. Use for helper functions:

```yaml
init: |
  -- Combat system
  function fight(enemy_name, enemy_skill, enemy_stamina)
    local player_roll = d(6) + d(6) + skill
    local enemy_roll = d(6) + d(6) + enemy_skill
    
    if player_roll > enemy_roll then
      enemy_stamina = enemy_stamina - 2
      return enemy_stamina, "You hit the " .. enemy_name .. "!"
    elseif enemy_roll > player_roll then
      stamina = stamina - 2
      return enemy_stamina, "The " .. enemy_name .. " hits you!"
    else
      return enemy_stamina, "Both attacks miss!"
    end
  end

  -- Luck test
  function test_luck()
    local roll = d(6) + d(6)
    local success = roll <= luck
    luck = luck - 1  -- Using luck reduces it
    return success
  end
```

## Variables and State

### Variable Scopes

Codex has three variable scopes:

1. **Global variables** (`vars:`): Persist throughout the story
2. **Section variables** (`s.*`): Specific to each section, persist across visits
3. **Temporary variables** (`temp.*`): Reset when entering a section

```yaml
vars:
  gold: 50 # Global: persists everywhere

sections:
  shop:
    vars:
      shopkeeper_mood: 'friendly' # Section-specific
    run: |
      temp.greeting = "Welcome!"    # Temporary
    text: |
      The shopkeeper says: {temp.greeting}
      You have {gold} gold.
```

### Variable Interpolation

Embed Lua expressions in text using `{expression}`:

```yaml
text: |
  You have {gold} gold coins.
  Your health: {stamina}/{stamina_init}
  The enemy has {s.enemy_hp} HP remaining.
```

You can use any Lua expression:

```yaml
text: |
  You need {100 - gold} more gold.
  Status: {stamina > 10 and "Healthy" or "Wounded"}
  Inventory count: {#equip}
```

## Sections

Sections are the building blocks of your story. Each section represents a location, scene, or game state.

### Basic Section

```yaml
sections:
  forest_path:
    title: 'The Forest Path' # Optional display title
    text: 'You stand on a winding forest path.'
    options:
      north: ['Go north', mountain]
      south: ['Go south', village]
```

### Section with Variables

```yaml
sections:
  dragon_lair:
    vars:
      dragon_hp: 30
      dragon_skill: 10
      rounds: 0
    text: |
      You face the dragon!
      Dragon HP: {s.dragon_hp}
      Combat rounds: {s.rounds}
```

### Section Run Code

Code that executes when entering a section:

```yaml
sections:
  treasure_room:
    run: |
      if not s.looted then
        gold = gold + 100
        s.looted = true
      end
    text: |
      {s.looted and "The room has been plundered." or "You find treasure!"}
```

### Visit Tracking

Sections automatically track visit counts via `s.visits`:

```yaml
sections:
  mysterious_door:
    text: |
      {s.visits == 1 and "You see a strange door." or "You return to the door."}
    options:
      enter:
        text: 'Enter'
        flags: [first] # Only on first visit
        goto: secret_room
      leave:
        text: 'Leave'
        flags: [not_first] # Only after first visit
        goto: hallway
```

## Options

Options are the choices players make to progress through the story.

### Short Form

Quick syntax for simple choices:

```yaml
options:
  north: ['Go north', forest]
  south: ['Go south', castle]
  # [display text, target section]
```

### Long Form

Full control over option behavior:

```yaml
options:
  attack_goblin:
    text: 'Attack the goblin'
    if: s.goblin_hp > 0 # Show only if condition true
    run: | # Execute when selected
      s.goblin_hp = s.goblin_hp - 2
      stamina = stamina - 1
    goto: self # Stay on same section
    notify: 'You strike the goblin!'
    flags: [once] # Hide after use
```

### Option Properties

- **`text`**: Display text (required)
- **`goto`**: Target section ID (default: current section)
  - Use `self` to stay on same section
- **`if`**: Lua condition (show only if true)
- **`run`**: Lua code to execute when chosen
- **`notify`**: Message to show in popup after execution
- **`confirm`**: Confirmation message (not yet implemented)
- **`hidden`**: Boolean, hide this option
- **`flags`**: Array of special behaviors

### Option Flags

- **`once`**: Option disappears after use
- **`first`**: Only shown on first visit to section
- **`not_first`**: Only shown on subsequent visits

```yaml
options:
  secret_lever:
    text: 'Pull the hidden lever'
    flags: [once, first]
    run: |
      insert(equip, 'magic_key')
    notify: 'You found a magic key!'
    goto: self
```

### Conditional Options

Show options based on game state:

```yaml
options:
  use_key:
    text: 'Unlock the door with the silver key'
    if: contains(equip, 'silver_key')
    goto: treasure_room

  bribe_guard:
    text: 'Bribe the guard (50 gold)'
    if: gold >= 50
    run: |
      gold = gold - 50
    goto: treasure_room

  fight_guard:
    text: 'Fight the guard'
    if: skill >= 8
    goto: guard_combat
```

## Lua Scripting

Codex embeds a full Lua 5.1 interpreter. Use Lua for game logic, calculations, and complex behaviors.

### Built-in Helper Functions

Codex provides these convenience functions:

#### Dice Rolling

```lua
d(6)                    -- Roll 1d6 (returns 1-6)
d(20)                   -- Roll 1d20
dice(3, 6, 2)          -- Roll 3d6+2 (count, sides, modifier)
```

#### Array/Table Operations

```lua
contains(equip, 'sword')           -- Check if item in array
insert(equip, 'shield')            -- Add item to array
remove(consumables, 'potion')      -- Remove first instance
remove_all(consumables, 'potion')  -- Remove all instances
count(consumables, 'potion')       -- Count occurrences
```

### Common Lua Patterns

#### Random Outcomes

```lua
run: |
  local roll = d(6)
  if roll <= 2 then
    stamina = stamina - 3
    temp.result = "You fall and hurt yourself!"
  elseif roll <= 4 then
    temp.result = "You barely make it across."
  else
    gold = gold + 10
    temp.result = "You find gold on the other side!"
  end
```

#### Inventory Management

```lua
run: |
  if contains(equip, 'torch') then
    remove(equip, 'torch')
    insert(equip, 'burnt_torch')
    temp.msg = "Your torch burns out."
  end
```

#### Stat Checks

```lua
run: |
  local roll = d(6) + d(6)
  if roll <= skill then
    -- Success
    temp.success = true
  else
    -- Failure
    temp.success = false
    stamina = stamina - 2
  end
```

#### Combat System Example

```lua
init: |
  function combat_round(enemy_name, enemy_skill, enemy_stamina)
    local player_attack = d(6) + d(6) + skill
    local enemy_attack = d(6) + d(6) + enemy_skill

    local msg = "You rolled " .. player_attack .. "\n"
    msg = msg .. enemy_name .. " rolled " .. enemy_attack .. "\n\n"

    if player_attack > enemy_attack then
      enemy_stamina = enemy_stamina - 2
      msg = msg .. "You hit! Enemy HP: " .. enemy_stamina
    elseif enemy_attack > player_attack then
      stamina = stamina - 2
      msg = msg .. "Enemy hits! Your HP: " .. stamina
    else
      msg = msg .. "Both miss!"
    end

    return enemy_stamina, msg
  end
```

### Lua Standard Library

Available Lua modules:

- `math.*` (math.random, math.floor, math.max, etc.)
- `string.*` (string.find, string.sub, string.upper, etc.)
- `table.*` (table.insert, table.remove, etc.)

**Note**: File I/O and OS operations are not available for security.

## Events System

Events are custom handlers you can trigger from anywhere in your story. Useful for item usage, special actions, etc.

### Defining Events

```yaml
events:
  use_item:
    params: [item_id]
    run: |
      if item_id == "healing_potion" then
        stamina = math.min(stamina + 5, stamina_init)
        remove(consumables, item_id)
        return "You feel refreshed! +5 stamina"
      elseif item_id == "strength_elixir" then
        skill = skill + 2
        skill_init = skill_init + 2
        remove(consumables, item_id)
        return "Your strength increases! +2 skill"
      else
        return "You can't use that right now."
      end

  rest:
    params: []
    run: |
      if meals > 0 then
        stamina = math.min(stamina + 4, stamina_init)
        meals = meals - 1
        return "You eat a meal and rest. +4 stamina"
      else
        return "You have no meals left."
      end
```

### Triggering Events

Events are triggered automatically by the UI (e.g., clicking items in character sheet). You can also trigger them from options:

```lua
run: |
  local message = event_rest()  -- Call event handler
  temp.result = message
```

## Hooks

Hooks are callbacks that run at specific times during gameplay.

### Available Hooks

```yaml
hooks:
  post_option:
    run: |
      -- Runs after every option selection
      -- Good for hunger systems, time tracking, etc.
      if stamina <= 0 then
        goto_section = "death"
      end
```

### Using Hooks

```yaml
hooks:
  post_option:
    run: |
      -- Hunger system
      if stamina > 0 then
        stamina = stamina - 1
      end

      -- Time tracking
      hours = hours + 1
      if hours >= 24 then
        days = days + 1
        hours = 0
      end

      -- Death check
      if stamina <= 0 then
        goto_section = "game_over"
      end
```

## Templates and Reusability

YAML anchors let you reuse option sets across multiple sections.

### Defining Templates

```yaml
templates:
  # Combat template
  - &combat
    attack:
      text: 'Attack'
      if: s.enemy_hp > 0
      run: |
        local damage = d(6)
        s.enemy_hp = s.enemy_hp - damage
        stamina = stamina - 1
      goto: self

    defend:
      text: 'Defend'
      if: s.enemy_hp > 0
      run: |
        stamina = stamina + 1
      goto: self

    flee:
      text: 'Flee'
      if: s.enemy_hp > 0
      goto: previous_room

  # Standard navigation
  - &directions
    north: ['North', north_room]
    south: ['South', south_room]
    east: ['East', east_room]
    west: ['West', west_room]
```

### Using Templates

```yaml
sections:
  goblin_fight:
    vars:
      enemy_hp: 8
    text: "A goblin attacks! (HP: {s.enemy_hp})"
    options:
      <<: *combat              # Include combat template
      # Can add section-specific options too
      victory:
        text: "Continue"
        if: s.enemy_hp <= 0
        goto: next_room

  orc_fight:
    vars:
      enemy_hp: 15
    text: "An orc blocks your path! (HP: {s.enemy_hp})"
    options:
      <<: *combat              # Reuse same combat system
```

## Best Practices

### Story Structure

1. **Start simple**: Begin with a linear story, add branching later
2. **Name sections clearly**: Use descriptive IDs like `forest_entrance`, not `room12`
3. **Test frequently**: Play through your story often during development
4. **Save backups**: Use version control (git) for your story files

### Variable Management

1. **Initialize all globals**: Define every variable in `vars:` section
2. **Use meaningful names**: `dragon_hp` not `dhp`
3. **Track initial values**: Store `stamina_init` for max HP calculations
4. **Comment your code**: Explain complex Lua logic

### Performance

1. **Avoid deep nesting**: Keep Lua functions simple
2. **Don't store functions in variables**: Use the `init:` section for functions
3. **Limit table iterations**: Avoid excessive looping in hot paths

### Player Experience

1. **Provide clear choices**: Option text should be unambiguous
2. **Show consequences**: Use `notify` to give feedback
3. **Balance difficulty**: Test your combat/puzzle systems thoroughly
4. **Add variety**: Use conditional options to show different content on replays

### Debugging

1. **Check browser console**: Lua errors appear here
2. **Use temp variables**: Store debug info in `temp.debug`
3. **Test edge cases**: What if player has 0 gold? 0 stamina?
4. **Validate conditions**: Make sure `if` conditions handle all states

```yaml
# Good: Handles missing item gracefully
if: contains(equip, 'key')

# Bad: Crashes if equip is nil
if: equip.key == true
```

## Complete Examples

### Simple Adventure

```yaml
title: The Haunted Manor
author: Story Author
version: 1.0

vars:
  courage: 10
  has_lantern: false
  explored_basement: false

sections:
  start:
    text: |
      You stand before a dark, abandoned manor. The door creaks open.
    options:
      enter: ['Enter the manor', entrance_hall]
      leave: ['Leave this cursed place', ending_coward]

  entrance_hall:
    text: |
      The entrance hall is dimly lit. Dust covers everything.
      Doors lead north and east. Stairs descend to the basement.
    options:
      north: ['Go north', library]
      east: ['Go east', dining_room]
      down: ['Go downstairs', basement]

  library:
    run: |
      if not s.searched then
        has_lantern = true
        s.searched = true
      end
    text: |
      {s.searched and "The library has been searched." or "You find a lantern!"}
    options:
      back: ['Return to entrance', entrance_hall]

  basement:
    text: |
      {has_lantern and "Your lantern lights the way." or "It's too dark to see!"}
    options:
      explore:
        text: 'Explore'
        if: has_lantern
        run: |
          explored_basement = true
        goto: basement_treasure
      back: ['Go back upstairs', entrance_hall]

  basement_treasure:
    text: "You found the treasure! You've won!"
    options:
      restart: ['Play again', start]
```

### Combat System

```yaml
title: Arena Combat
author: Story Author
version: 1.0

vars:
  skill: 8
  stamina: 20
  skill_init: 8
  stamina_init: 20

init: |
  function fight(enemy_skill, enemy_hp)
    local p_roll = d(6) + d(6) + skill
    local e_roll = d(6) + d(6) + enemy_skill
    
    fight_msg = "You: " .. p_roll .. " vs Enemy: " .. e_roll .. "\n"
    
    if p_roll > e_roll then
      enemy_hp = enemy_hp - 2
      fight_msg = fight_msg .. "You hit! Enemy HP: " .. enemy_hp
    elseif e_roll > p_roll then
      stamina = stamina - 2
      fight_msg = fight_msg .. "Enemy hits! Your HP: " .. stamina
    else
      fight_msg = fight_msg .. "Both miss!"
    end
    
    return enemy_hp
  end

templates:
  - &combat
    attack:
      text: '⚔️ Attack'
      if: s.enemy_hp > 0 and stamina > 0
      run: |
        s.enemy_hp = fight(s.enemy_skill, s.enemy_hp)
      notify: '{fight_msg}'
      goto: self

    victory:
      text: 'Victory!'
      if: s.enemy_hp <= 0
      goto: victory

    defeat:
      text: '...'
      if: stamina <= 0
      goto: defeat

sections:
  start:
    text: 'Enter the arena?'
    options:
      fight: ['Fight!', warrior_fight]

  warrior_fight:
    vars:
      enemy_hp: 12
      enemy_skill: 7
    text: |
      A warrior challenges you!
      Enemy HP: {s.enemy_hp} | Your HP: {stamina}
    options:
      <<: *combat

  victory:
    text: 'You are victorious!'
    options:
      again: ['Fight again', start]

  defeat:
    text: 'You have been defeated...'
    options:
      restart: ['Try again', start]
```

### Inventory Puzzle

```yaml
title: The Locked Door
author: Story Author
version: 1.0

vars:
  equip: []
  gold: 0

sections:
  start:
    text: 'You need to find a way through the locked door.'
    options:
      search: ['Search the room', search]
      door: ['Examine the door', door]

  search:
    run: |
      if not s.searched then
        if contains(equip, 'rusty_key') then
          insert(equip, 'golden_key')
          remove(equip, 'rusty_key')
          s.found_polish = true
        else
          insert(equip, 'rusty_key')
        end
        s.searched = true
      end
    text: |
      {s.found_polish and "You polish the rusty key. It's golden!" or "You find a rusty key."}
    options:
      back: ['Back', start]

  door:
    text: 'A heavy door with a golden keyhole.'
    options:
      unlock:
        text: 'Use golden key'
        if: contains(equip, 'golden_key')
        run: |
          remove(equip, 'golden_key')
        goto: beyond
      back: ['Back', start]

  beyond:
    text: "The door opens! You've escaped!"
    options: {}
```

---

## Need Help?

- Check the browser console for Lua errors
- Review the example stories in `web/public/stories/`
- See the main README for technical architecture details
- Report bugs or ask questions on GitHub

Happy storytelling!
