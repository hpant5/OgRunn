# OgreRunn V1 Game Design

## Core Loop

Start level, navigate the maze, avoid the ogre, hide under benches when needed, use at most two map reveals, reach the gate `G`, then advance to the next level.

## Game States

- `MainMenu`
- `LevelStart`
- `Playing`
- `MapReveal`
- `Paused`
- `LevelComplete`
- `GameOver`

## Level Rules

- `P` starts on the bottom-right side.
- `O` starts on the top-left side.
- `G` is the only win condition.
- Each level has exactly two benches.
- Levels should get more complex without making early levels unfair.

## Hiding

The player is hidden while their center point is inside a bench hide zone. Hidden players cannot be detected by the ogre. Leaving the bench zone makes the player visible again.

## Map Reveal

Each level grants two map reveals. A reveal lasts five seconds, shows the full level, and freezes both the player and ogre.
