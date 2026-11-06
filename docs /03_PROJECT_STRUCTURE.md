# Stack Tower — Unity Project Structure

Use the existing Unity project if one has already been created. Keep all game-specific content inside `Assets/_Project/StackTower/` so it is easy to find and maintain.

## Required folder layout

```text
Assets/
└── _Project/
    └── StackTower/
        ├── Scenes/
        │   ├── MainMenu.unity
        │   └── Gameplay.unity
        ├── Scripts/
        │   ├── Core/
        │   │   ├── GameManager.cs
        │   │   └── GameState.cs
        │   ├── Gameplay/
        │   │   ├── TowerController.cs
        │   │   ├── MovingBlock.cs
        │   │   └── BlockPiece.cs
        │   ├── UI/
        │   │   ├── MainMenuUI.cs
        │   │   └── GameplayUI.cs
        │   └── Platform/
        │       └── PrimaryInput.cs
        ├── Prefabs/
        │   ├── Block.prefab
        │   └── Offcut.prefab
        ├── Materials/
        ├── Art/
        └── Audio/
```

The agent may adjust file names slightly to fit the existing project, but must preserve the separation of responsibilities and avoid creating unnecessary scripts.

## Scene responsibilities

### `MainMenu.unity`

- Contains the main menu Canvas and its Play button.
- Shows the game title and saved best score.
- Loads `Gameplay.unity` when Play is pressed.

### `Gameplay.unity`

- Contains the orthographic camera, base block, gameplay controller, moving block, score UI, and Game Over overlay.
- Game Over is a UI panel in this scene; it is **not** a third scene.
- Restart resets the current gameplay scene or reinitializes the gameplay state reliably.
- Main Menu returns to `MainMenu.unity`.

## Script responsibilities

### `GameManager.cs`

Owns the high-level run state and coordinates starting, playing, and game over. Avoid putting all block math, animation, and UI code into this one class.

### `GameState.cs`

A small enum or equivalent state definition, such as `MainMenu`, `Playing`, and `GameOver`. Do not create a large state framework for this MVP.

### `TowerController.cs`

Owns the tower's block placement, overlap calculation, trimming, scoring event, and reset of tower objects.

### `MovingBlock.cs`

Owns horizontal movement and exposes a clear drop/stop operation. It should not independently update the score or manipulate menu UI.

### `BlockPiece.cs`

Represents a falling overhang/offcut. It may use a Rigidbody2D with gravity and be destroyed after leaving the visible play area or after a short lifetime.

### `MainMenuUI.cs`

Connects the Play button and displays the best score on the main menu.

### `GameplayUI.cs`

Displays the current score, shows/hides the Game Over overlay, displays the final and best scores, and connects Restart and Main Menu buttons.

### `PrimaryInput.cs`

Provides one consistent primary action to the gameplay controller. Support touch on mobile and mouse on desktop/web. Centralize input so the same physical action cannot be processed twice.

## Project setup rules

- Add both scenes to the project's build scene list.
- Use the existing Unity version and render pipeline unless a project constraint requires a change.
- Use Unity built-in 2D objects, simple sprites, and materials for the blocks and background.
- Use TextMeshPro if it is already available or can be enabled through the normal Unity package setup; otherwise use the existing UI text solution. Do not let font/package setup block the playable MVP.
- Keep serialized references visible and clearly named in the Inspector.
- Avoid hard-coded scene object searches every frame. Prefer serialized references or one-time setup.
- Do not create duplicate managers when restarting or changing scenes.
