# Runn

A horror stealth-chase maze game for Android. The player navigates a maze, finds a key, and escapes through the exit door while a slow but relentless ogre patrols, hunts by sight and sound, and sprints when it spots you. Hide under benches when you have no other option, and use a limited "Map Ping" ability to peek at the level layout.

The end goal is to ship to the Google Play Store with rewarded + interstitial ads.

## Repository layout

- [`prototype-web/`](prototype-web/) - the original web (HTML + canvas raycasting) prototype. Kept as a **frozen reference** for design tuning, level layouts, and AI behavior. Not the shipping build.
- [`unity/Runn/`](unity/Runn/) - the **shipping build**. Unity 2022.3 LTS, third-person 3D, Android target.

## Finalized design

### Core loop (one level, ~90-180s)

- Spawn in a hand-authored maze. Find the key, then reach the exit door.
- Ogre states: `Patrol` -> `Investigate noise` -> `Chase on sight` -> `Search last-known position` -> back to `Patrol`.
- Two **Map Pings** per level. Player freezes for 4 seconds, ogre keeps moving. Reveals exit, benches, and key. Ogre is shown with a 1 second position delay (imperfect info on purpose).
- Two benches per level. Hiding has a hold-breath timer (8 seconds). If the ogre saw you enter, it walks straight to the bench and ends the run with the line "I SAW YOU HIDING".

### Tuning (carried forward from the web prototype)

- Player speed `1.0`, ogre base speed `0.5`.
- Ogre sprint burst `1.15` for `2.5s`, `8s` cooldown, triggered by confirmed line-of-sight.
- Player FOV 60 degrees, ogre FOV 75 degrees, ogre vision distance is 0.75x the player's. Walls fully occlude vision.
- Hearing: sprinting and fast turns emit noise events the ogre investigates.

### Tone and monetization

- Horror tone. Audio carries most of the fear (heartbeat ramp on proximity, ogre footsteps with distance falloff).
- Rewarded ads: "Reveal exit for 3s", "+1 Map Ping" pre-run, "Revive once" on death.
- Interstitial ads: only between levels and after death, never mid-run.

### Progression

- Levels 1-3: teach controls, hiding, map ping.
- Levels 4-6: locked exit, 1 key.
- Levels 7-10: 2 keys, ogre variants ("keen hearing", "fast sprinter", "bench-checker").

## Controls (Android)

- Left thumb: virtual joystick (move).
- Right thumb: drag to look.
- HUD buttons: `Map Ping`, `Hide / Leave hiding`.

## Build instructions

### Web prototype (reference)

```powershell
cd W:\MobileApps\Runn\prototype-web
python -m http.server 4173 --bind 127.0.0.1
```

Open `http://127.0.0.1:4173/`.

### Unity Android build

Prerequisites:

- Unity Hub + Unity 2022.3 LTS with **Android Build Support** module (includes OpenJDK, Android SDK, NDK).
- Android device with USB debugging on, or an Android emulator.

Open and build:

1. In Unity Hub, click `Add` and select [`unity/Runn`](unity/Runn/).
2. Open the project. Open `Assets/Scenes/Boot.unity`.
3. `File -> Build Settings`. Verify `Android` is the target platform with `Switch Platform`.
4. `Player Settings`:
   - `Scripting Backend`: IL2CPP.
   - `Target Architectures`: ARM64 only (uncheck ARMv7 for Play Store).
   - `Minimum API Level`: 24 (Android 7.0).
   - `Target API Level`: Automatic (highest installed).
5. Connect device, click `Build And Run` (or `Build` to produce an `.apk` / `.aab`).

For a signed Play Store build:

1. `Player Settings -> Publishing Settings -> Keystore Manager` to create a keystore.
2. In `Build Settings`, check `Build App Bundle (Google Play)` and click `Build`.

## Project status

See [`.cursor/plans/`](.cursor/plans/) for the active build plan and incremental slice checklist.
