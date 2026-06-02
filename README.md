# OgreRunn

OgreRunn is an Android-focused 2D top-down maze survival game. The player starts near the bottom-right of a maze, the ogre starts near the top-left, and the goal is to reach the gate marked `G` before being caught.

The original V1 implementation lives under `unity/Runn`. The `version-2` branch starts a Godot rebuild under `godot/OgreRunn` while keeping the same V1 gameplay rules as the source of truth.

## V2 Godot Rebuild

- Godot project path: `godot/OgreRunn`.
- Target engine: Godot 4.x.
- First milestone: rebuild the V1 core loop in Godot before adding new mechanics.
- Current prototype includes text-level loading, grid pathfinding, player movement, ogre AI states, bench hiding, map reveal, level completion, and game-over handling.

## V1 Scope

- 5 built-in maze levels.
- Tile markers: `#` wall, `.` path, `P` player spawn, `O` ogre spawn, `B` bench, `G` gate.
- Exactly 2 benches per level.
- Player speed constant: `1.00` design ratio.
- Ogre speed constant: `0.80` of player speed.
- Normal gameplay uses top-down camera visibility around the player.
- Map reveal shows the full maze for 5 seconds.
- Map reveal has 2 uses per level.
- Player and ogre freeze during map reveal.
- Ogre uses grid pathfinding and V1 states: `Patrol`, `Chase`, `Search`, `Lost`.
- Player is hidden while inside a bench hide zone.
- Hidden player is not detected by the ogre.
- Player wins by reaching `G`.
- Player loses when caught by the ogre.

## Repository Layout

- `godot/OgreRunn/` - V2 Godot rebuild.
- `prototype-web/` - frozen web prototype reference.
- `unity/Runn/` - V1 Android game implementation.
- `docs/` - planning and release notes.

## Godot Setup

Open `godot/OgreRunn` in Godot 4.x and run `scenes/main.tscn`.

## Unity Setup

Open `unity/Runn` in Unity Hub with Unity `2022.3.x` and install Android Build Support.

To create/open the boot scene, use the editor helper under `Assets/Editor/BootSceneCreator.cs`, then open `Assets/Scenes/Boot.unity`.

## Controls

- `W` / Arrow Up: move up
- `A` / Arrow Left: move left
- `S` / Arrow Down: move down
- `D` / Arrow Right: move right
- `M`: activate map reveal
- `P`: pause/resume
- `R`: restart level
- Android: left joystick/d-pad movement, map button, pause button

## Tests

Edit-mode tests live in `unity/Runn/Assets/Tests/EditMode`.

They cover:

- V1 constants
- Level parsing
- Grid pathfinding

## Not In V1

These are intentionally excluded until the core V1 loop is stable:

- Multiplayer
- Ads or monetization
- Key/inventory systems
- Weapons/combat
- Cloud saves
- Character skins
- Advanced stealth sound mechanics
