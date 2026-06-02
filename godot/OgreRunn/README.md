# River Rafting OG

Godot 4.x survival rafting prototype.

## Run

Open this folder in Godot 4.x and run `scenes/main.tscn`.

## Current Game Loop

You wake up in a hilly jungle beside a river. Collect wood and rope from the shore, build a raft at the river edge, ride downstream, repair the raft as it loses health, fish for food, and survive long enough to clear the river.

## Controls

- `WASD` or arrow keys: move / steer raft
- `E`: collect, build raft, board raft, leave raft
- `Q`: repair raft with 1 wood and 1 rope
- `F`: fish from river edge or raft
- `C`: eat fish to restore hunger
- `R`: restart

## Survival Rules

- Hunger starts at 10.
- Staying in water for more than 10 seconds in one stretch causes an alligator attack and costs 1 hunger.
- The raft protects you from water attacks, but loses health while moving downstream.
- Shore hazards can attack when you are away from the water.
- Fish can restore hunger.
- Reach the downstream finish zone to clear the level.
