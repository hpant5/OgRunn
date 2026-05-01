# Runn - Web Prototype (frozen reference)

This is the original first-person, raycasting web prototype for Runn. It is kept here as a **frozen design reference** for the Unity build. New gameplay work happens in `unity/Runn/`.

## Run locally

```powershell
cd W:\MobileApps\Runn\prototype-web
python -m http.server 4173 --bind 127.0.0.1
```

Open `http://127.0.0.1:4173/`.

## Controls

- `WASD`: move and strafe
- Arrow keys: move and turn
- Mouse drag: look around
- `M`: map view
- `H`, `E`, or `Space`: hide or leave hiding near a bench

## What this prototype proves

- 5 hand-authored ASCII grid levels with consistent tile size.
- Ogre LOS chase with simple grid pathfinding and a search state when the player is lost.
- Hide-under-bench mechanic that only succeeds if the ogre did not see the entry.
- Limited "map view" peeks (2 per level).

These mechanics are ported into Unity in [`../unity/Runn/`](../unity/Runn/).
