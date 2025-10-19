# 🐔 Chicken Invaders - Unity Recreation

A modern recreation of the classic Chicken Invaders arcade shooter, built in Unity as a school project. This project showcases various game development patterns, performance optimizations, and polished gameplay mechanics.

## 🎮 Gameplay Features

### Core Mechanics

-   **Wave-Based Combat**: Six unique enemy wave patterns that cycle throughout the game
    -   Formation waves with synchronized movement
    -   Arc patterns with sine wave motion
    -   Diving enemies that target the player
    -   Column-based descents
    -   Pincer attacks from both sides
    -   Epic boss battles every 6th wave
-   **Progressive Difficulty**: Enemies scale in health and complexity based on wave tier
-   **Boss Battles**: Large boss enemies with increased HP, multiple attack patterns, and generous rewards
-   **Power-Up System**:
    -   Weapon upgrades (8 levels)
    -   Shield pickups for temporary invulnerability
    -   Bomb system for screen-clearing attacks
    -   Food drops from enemies for scoring

### Player Experience

-   **Dual Control Schemes**: Mouse (with smoothing) or keyboard controls
-   **Light Speed Transitions**: Cinematic sequences after boss battles with parallax acceleration
-   **Score System**: High score tracking with automatic bonus lives and bombs
-   **Persistent Leaderboard**: Top 5 scores saved locally with player names
-   **Dynamic Audio**: Context-aware music system with smooth transitions between themes

## 🏗️ Architecture & Design Patterns

### Core Patterns

-   **Singleton Pattern**: Used for all manager classes (GameManager, AudioManager, PoolManager, etc.)
-   **Object Pooling**: High-performance pooling system for frequently instantiated objects:
    -   Enemies, bullets, bombs, food, UI icons
    -   Prevents garbage collection spikes and maintains smooth frame rates
    -   Dynamic pool expansion when needed
-   **State Machine**: Clean game state management (Menu, Playing, Paused, GameOver)
-   **Interface-Driven Design**:
    -   `IDamageable` for all objects that can take damage
    -   `IPoolable` for objects that work with the pooling system

### Code Organization

-   **Separation of Concerns**: Each script handles a single responsibility
    -   `EnemyController`: Health and damage
    -   `EnemyMover`: Movement patterns
    -   `EnemyShootingSimple`: Attack behavior
-   **Manager Pattern**: Centralized systems for audio, UI, waves, pickups, and pools
-   **Event-Driven Architecture**: C# events for game state changes and decoupled communication

### Performance Optimizations

-   **Object Pooling**: Eliminates expensive instantiate/destroy calls
-   **Cached Component References**: Animator parameter hashing to avoid string lookups
-   **Efficient Enemy Cleanup**: Automatic return to pool when off-screen
-   **Dynamic Pool Sizing**: Pools expand automatically under high load

## 🎵 Audio System

The game features a sophisticated audio management system:

-   **Dynamic Music Transitions**: Smooth fading between different musical themes
-   **Context-Aware Playback**: Music changes based on game state (menu, boss battle, light speed)
-   **Resume Functionality**: Theme music resumes from the exact timestamp after boss battles
-   **Volume Controls**: Independent master, music, and SFX volume settings
-   **Pause-Aware**: Audio properly pauses and resumes with game state

## 🎨 Visual Effects

-   **Parallax Scrolling**: Multi-layer background with depth perception
-   **Light Speed Effect**: Background acceleration during transition sequences
-   **Animations**: Character movement, explosions, and attack animations
-   **HUD System**: Dynamic UI with pooled icons for lives and bombs

## 🛠️ Technical Implementation

### Key Systems

-   **WaveDirector**: Orchestrates enemy spawn patterns and difficulty progression
-   **PickupManager**: Handles loot drops with configurable probabilities
-   **SettingsManager**: Persistent player preferences using PlayerPrefs
-   **LeaderboardManager**: JSON-based score persistence with date tracking
-   **ParallaxManager**: Dynamic background speed control for cinematic effects

### Unity Features Used

-   Universal Render Pipeline (URP)
-   New Input System
-   Animator State Machines
-   Coroutines for timing and sequences
-   PlayerPrefs for data persistence
-   TextMeshPro for UI text

## 📚 Learning Outcomes

This school project demonstrates proficiency in:

-   Unity game development fundamentals
-   C# programming best practices
-   Design pattern implementation
-   Performance optimization techniques
-   Audio/visual polish and game feel
-   Clean, maintainable code architecture

## 🎓 Project Status

This is a **school project** created for educational purposes. While the game is fully playable and polished, it is not intended for commercial release.

## 🎯 Getting Started

1. Open the project in Unity 6 (2023.3+)
2. Load the main scene from `Assets/Scenes/`
3. Press Play to start the game
4. Use mouse or WASD/Arrow keys to move
5. Click or Space to shoot
6. Press B to use bombs (when available)

---

_Developed with Unity 6000.0.54f1_
