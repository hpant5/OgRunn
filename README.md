# OgRunn

First playable prototype for a first-person maze escape game. The player is chased by an ogre, can use limited map views, and can hide under benches if the ogre does not see the hide.

## Run locally

```powershell
cd W:\MobileApps\Runn
python -m http.server 4173 --bind 127.0.0.1
```

Open `http://127.0.0.1:4173/`.

## Controls

- `WASD`: move and strafe
- Arrow keys: move and turn
- Mouse drag: look around
- `M`: map view
- `H`, `E`, or `Space`: hide or leave hiding near a bench

## Prototype scope

- 5 starter levels
- Ogre speed is half the player speed
- Ogre visibility is 75% of player visibility
- 2 map views per level
- 2 benches per level
- Simple grid pathfinding for ogre chase and search behavior
