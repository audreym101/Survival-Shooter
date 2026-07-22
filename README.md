# Survival Shooter - Audrey

An AR (augmented reality) survival shooter built in Unity. Place the game world in your real environment using AR Foundation, then fight off waves of enemies before time runs out.

## Gameplay

- Point your device at a flat surface to find a placement spot, then tap to drop the game world into your space.
- Enemies spawn continuously and either shoot at or melee the player.
- Survive until the timer runs out, or die trying — either way your score, enemy kill count, and survival time are saved to a local leaderboard.
- Choose between **Easy** and **Hard** difficulty, which changes enemy spawn rate, enemy speed, enemy health, and match duration.

## Tech Stack

- **Engine:** Unity 6000.0.76f1
- **AR:** AR Foundation (`ARRaycastManager`, plane detection/placement)
- **Platform:** Android (AR-capable device required to play in AR mode; runs directly in-editor without AR for quick iteration)

## Project Structure

```
Assets/
  Scripts/        Gameplay, AR, UI, and manager scripts
  Animations/      Enemy animator controller
  Audio/          Gunshots, enemy sounds, UI sound effects
Packages/          Unity package manifest
ProjectSettings/   Unity project configuration
```

### Key Scripts

| Script | Responsibility |
|---|---|
| `GameManager` | Core game loop — start/end/restart, match timer |
| `ARPlacementManager` | AR plane detection and placing the game world in the real world |
| `EnemySpawner` / `EnemyBase` / `ShooterEnemy` / `MeleeEnemy` | Enemy spawning and behavior |
| `PlayerMovement` / `PlayerShooter` / `PlayerHealth` | Player control, shooting, and health |
| `DifficultyManager` | Easy/Hard difficulty presets |
| `ScoreManager` / `LeaderboardManager` | Score tracking and local high-score leaderboard (`PlayerPrefs`) |
| `UIManager` / `MainMenuManager` | HUD and menu screens |
| `AudioManager` | Sound effect playback |

## Getting Started

1. Open the project in **Unity 6000.0.76f1** (or newer) via Unity Hub.
2. Open the main scene from `Assets/Scenes` and press Play to test in the Editor (the game world auto-spawns without needing AR).
3. To test AR placement, build to an AR-capable Android device (**File > Build Settings > Android**).

## Building

- Target platform: Android
- Requires an ARCore-supported device to run the AR placement flow.
