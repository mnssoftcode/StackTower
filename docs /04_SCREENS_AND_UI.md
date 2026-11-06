# Stack Tower — Screens and UI Specification

The MVP has **2 Unity scenes** and **3 user-facing UI states**. The Game Over state is an overlay inside the Gameplay scene.

## Screen/state 1: Main Menu

**Purpose:** Let the player start immediately and see their best score.

Visible elements:

- Game title: `STACK TOWER`.
- One short instruction line: `Tap to stack. Build as high as you can.`
- Best score label and saved value.
- One primary button: `PLAY`.

Layout:

- Center the content in the portrait play area.
- Make the Play button the most visually prominent element.
- Keep generous spacing and avoid extra menu links.

Button inventory for this state:

| Button | Action |
|---|---|
| Play | Load the Gameplay scene and start a fresh run. |

## Screen/state 2: Gameplay

**Purpose:** Keep attention on the moving block and tower.

Visible elements:

- Current score near the top center.
- Best score can appear as a smaller secondary label, but it is optional during play.
- Tower and moving block in the central play area.
- Optional one-time hint before the first drop: `TAP TO DROP`.
- Optional brief `PERFECT` feedback after a perfect placement.

Input:

- Tap anywhere in the gameplay play area to drop the moving block.
- Click anywhere in the gameplay play area on desktop/web.
- Do not add an on-screen drop button; the whole play area is the action surface.
- Do not add a pause button in this MVP.

Button inventory for this state: **0 buttons**.

## Screen/state 3: Game Over overlay

**Purpose:** Clearly communicate the end of the run and provide the two next actions.

Visible elements:

- A simple dimmed or softened gameplay background behind the overlay.
- Heading: `GAME OVER`.
- Final score.
- Best score.
- If a new best was achieved, show a brief `NEW BEST` label.
- Two buttons: `RESTART` and `MAIN MENU`.

Button inventory for this state:

| Button | Action |
|---|---|
| Restart | Start a completely fresh run in the Gameplay scene. |
| Main Menu | Load the Main Menu scene. |

## Exact MVP totals

- Unity scenes: **2**.
- User-facing UI states: **3**.
- Buttons: **3 total** — Play, Restart, Main Menu.
- Gameplay action: tap/click anywhere in the play area; it is not a button.
- Modal/overlay: **1** — Game Over.

## UI behavior requirements

- Every button must work; no decorative or placeholder buttons.
- Button hit areas must be comfortable for touch.
- Prevent gameplay input from firing when the player presses a UI button.
- Hide the Game Over overlay while playing; show it only after a game-over condition.
- Disable or ignore repeated taps after Game Over.
- Keep score text readable against the background and visible within safe areas.
- Do not add Settings, Shop, Level Select, Profile, Login, or other screens.
