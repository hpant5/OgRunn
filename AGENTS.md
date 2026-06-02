# Codex Notes

Follow `Ogrerunn V1 Readme.pdf` for gameplay behavior.

- Keep V1 simple and stable.
- For the Godot rebuild, keep runtime code under `godot/OgreRunn`.
- Keep Godot constants centralized in `godot/OgreRunn/scripts/constants.gd`.
- Keep Godot level data in text files under `godot/OgreRunn/levels`.
- Keep Unity constants centralized in `unity/Runn/Assets/Scripts/Systems/GameConstants.cs`.
- Keep Unity level data in text files under `unity/Runn/Assets/Levels/Resources`.
- Each V1 level must contain exactly one `P`, one `O`, one `G`, and two `B` markers.
- Do not add ads, keys, combat, multiplayer, or monetization to V1.
- Add tests for non-visual game logic when changing parser, pathfinding, hiding, or state behavior.
