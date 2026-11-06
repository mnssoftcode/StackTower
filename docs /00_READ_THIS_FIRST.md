# Stack Tower — Project AI Instructions

Read this file and every Markdown file in this folder before implementing the game. These documents define the MVP scope and should be treated as the project specification.

## Instruction to the development agent

1. Inspect the existing Unity project before changing it. Reuse its current Unity version and settings where practical.
2. Read all attached Stack Tower specification files. Do not implement from the original development prompt alone.
3. Build the playable MVP described in these files. Do not stop after producing a plan, mock-up, or placeholder UI.
4. Keep the implementation small, modular, and understandable. Use built-in Unity 2D features and avoid paid assets, online services, and unnecessary packages.
5. Follow the specified screen count, button count, portrait gameplay format, and project structure unless an existing project constraint makes that impossible. Explain any necessary deviation.
6. Do not invent extra gameplay systems or features. No shop, skins, missions, daily rewards, leaderboards, multiplayer, ads, in-app purchases, account system, analytics, or cloud saves in this MVP.
7. After implementation, run the game in the Unity Editor and fix compile errors, missing references, broken buttons, and gameplay bugs. Report what was actually tested and what still requires a device or platform build.

## Source of truth

- `01_GAME_OVERVIEW.md` — the product definition and MVP boundaries.
- `02_GAMEPLAY_RULES.md` — exact stacking, scoring, and game-over behavior.
- `03_PROJECT_STRUCTURE.md` — scenes, folders, and script responsibilities.
- `04_SCREENS_AND_UI.md` — exact screens, visible elements, and button inventory.
- `05_VISUAL_STYLE.md` — visual design rules and color palette.
- `06_ANIMATIONS_AND_FEEDBACK.md` — movement, transitions, and feedback.
- `07_PLATFORM_AND_RESPONSIVE_LAYOUT.md` — portrait mobile and cross-platform layout/input requirements.
- `08_ACCEPTANCE_CHECKLIST.md` — definition of done and test cases.

## Definition of success

A player can open the game, press **Play**, stack blocks using touch or click, see the score update, reach a game-over state, restart, return to the main menu, and retain their best score after restarting the app. The same core game layout must remain usable on the listed platforms.
