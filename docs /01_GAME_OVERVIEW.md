# Stack Tower — Game Overview

## One-sentence concept

A simple one-tap arcade game in which the player drops horizontally moving blocks onto a tower, trying to align each block and build the tallest stack.

## Product goals

- Make the core action understandable within a few seconds.
- Make one complete play session feel smooth and satisfying.
- Keep the game suitable for portrait mobile play while remaining playable on desktop and web.
- Deliver a polished, small MVP that can be tested and published after platform-specific checks.

## Core gameplay loop

1. The player starts a game.
2. A block moves horizontally above the current tower.
3. The player taps or clicks to drop it.
4. The part that overlaps the block below remains; any overhanging part is cut off.
5. The score increases and the next block appears.
6. The game ends when the block misses the tower or the remaining width becomes too small.
7. The player can restart or return to the main menu.

## MVP content

- 2 Unity scenes: `MainMenu` and `Gameplay`.
- 3 UI states: Main Menu, Gameplay, and Game Over overlay.
- 3 buttons total: Play, Restart, and Main Menu.
- One endless-style game mode that ends when the player misses.
- Current score and locally saved best score.
- Touch input on mobile; mouse input on desktop and web.
- A fixed portrait 9:16 gameplay composition on every platform.

## Explicitly out of scope

Do not add extra modes, levels, characters, enemies, power-ups, unlockable skins, an inventory, a shop, currencies, missions, daily rewards, online leaderboards, multiplayer, accounts, cloud saves, ads, in-app purchases, analytics, social sharing, or a settings screen.

## Quality priorities

1. Correct and reliable stacking behavior.
2. Responsive input with no accidental double drops.
3. Clear score and game-over feedback.
4. Smooth block and camera movement.
5. Consistent layout across aspect ratios.
6. Clean, minimal presentation.

A small, finished game is preferred over a larger unfinished game.
