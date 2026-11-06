# Stack Tower — Gameplay Rules

This file defines the required MVP mechanics. Keep the rules deterministic and easy to tune.

## Playfield setup

- Use a 2D orthographic camera.
- Place one stationary base block near the bottom of the portrait playfield.
- Spawn one moving block above the current top block.
- Keep the moving block fully visible and within the horizontal play bounds.
- Main tower blocks are normal 2D objects; do not add unnecessary Rigidbody2D components to every block.

## Moving block

- The block moves horizontally at a constant speed.
- It reverses direction when it reaches the allowed left or right movement boundary.
- The player can drop it with one primary action: a screen tap or mouse click.
- A successful placement creates exactly one next moving block.
- Increase movement speed gradually as the score grows, with a hard maximum so the game remains playable.
- Keep the speed values configurable in one place. Suggested starting speed: approximately 3 world units/second; suggested increase: approximately 0.08 per successful placement; suggested cap: approximately 7 world units/second. Tune these values through playtesting.

## Placement and trimming calculation

When the player drops a block, compare its horizontal bounds with the block directly below it:

1. Calculate the left and right edges of both blocks.
2. The overlap is the distance between the larger left edge and the smaller right edge.
3. If the overlap is zero or negative, the block missed: enter Game Over.
4. Otherwise, resize and reposition the placed block so it exactly covers the overlap.
5. If the moving block extended past either edge of the block below, create a small offcut for that overhanging section and animate it falling away.
6. Use the trimmed block as the base for the next placement.

Do not approximate the overlap using visual scale alone. The visible block width, collider width (if used), and calculated width must agree.

## Perfect placement

- Define a configurable perfect-alignment tolerance.
- Suggested tolerance: the absolute center difference is no more than 2% of the width of the block below.
- When within tolerance, snap the moving block to the exact center of the block below and do not create an offcut.
- Show a brief `PERFECT` feedback label or subtle visual effect.

## Scoring

- Add 1 point for every successful placement.
- Add 1 bonus point for a perfect placement, making a perfect placement worth 2 points total.
- The base block does not count as a point.
- Display the current score during gameplay.
- Update the best score whenever the current score exceeds it.
- Save the best score locally using Unity `PlayerPrefs` (or the project's existing simple local-save solution). Suggested key: `StackTower_BestScore_v1`.
- No online score service or cloud save is required.

## Too-small block rule

If the retained block width becomes smaller than a configurable minimum playable width, end the game after the placement. Suggested threshold: 10% of the starting block width. This prevents the game from continuing with an almost invisible block.

## Game-over conditions

Enter Game Over when either condition occurs:

- The dropped block has no horizontal overlap with the block below.
- The retained block is below the minimum playable width.

On Game Over:

- Stop block movement and reject further gameplay taps.
- Finalize and save the best score if needed.
- Show the Game Over overlay in the existing Gameplay scene.
- Provide Restart and Main Menu actions.

## Restart behavior

Restart must reset all run-specific state: score, block positions, block widths, movement speed, tower height, offcuts, camera position, and game-over state. The saved best score must remain unchanged unless the new run beats it.

## Implementation boundaries

- Keep gameplay independent from UI layout and platform-specific screen dimensions.
- Centralize tunable values such as movement speed, speed increase, perfect tolerance, and minimum width.
- Prevent a single press or touch from triggering more than one drop.
- Ignore gameplay input while a UI button is being pressed or while Game Over is active.
