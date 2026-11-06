# Stack Tower — Animation and Feedback Specification

Animations should make the game feel responsive without slowing down the one-tap loop. Keep them short, smooth, and consistent.

## Required motion

### 1. Moving block

- Move horizontally at a steady, readable speed.
- Reverse direction cleanly at the playfield boundaries.
- Avoid jitter, sudden position jumps, or movement outside the visible play area.

### 2. Successful placement

- On tap/click, stop horizontal movement immediately.
- Calculate the overlap and trim the block.
- Settle the retained block with a very small, quick scale or position ease (approximately 0.08–0.15 seconds).
- Do not make the player wait through a long drop animation before the next block appears.

### 3. Overhanging offcut

- Spawn only the part of the moving block that extends beyond the block below.
- Animate it falling down and away from the tower.
- A simple Rigidbody2D with gravity is acceptable for the offcut only.
- Remove the offcut after it leaves the camera view or after a short lifetime.
- Offcuts must not collide with or disturb the main tower.

### 4. Next block

- Spawn the next moving block above the new tower top.
- Keep its width equal to the retained block width.
- Start movement only after the previous placement has been processed.

### 5. Camera follow

- As the tower grows, move the orthographic camera upward smoothly so the active moving block and top of the tower remain visible.
- Use a short ease rather than a sharp snap.
- Keep UI in screen space so it does not move with the world camera.
- Do not zoom in or out dramatically during a normal run.

### 6. Game Over transition

- Stop the moving block and prevent additional drops.
- Show the Game Over overlay with a short fade or scale-in transition.
- Keep the transition brief and do not delay the Restart action unnecessarily.

## Feedback

- Score updates immediately after a successful placement.
- A perfect placement displays a brief `PERFECT` label and may use a subtle scale pulse or a few small particles.
- A new best score displays `NEW BEST` on the Game Over overlay.
- Button presses have a subtle pressed-state response.

## Timing guidance

These are targets, not rigid animation-system requirements:

| Feedback | Suggested duration |
|---|---:|
| Placement settle | 0.08–0.15 s |
| Perfect label | 0.45–0.75 s |
| Camera follow | 0.15–0.30 s |
| Game Over overlay | 0.15–0.25 s |
| Button press response | 0.08–0.12 s |

## Audio

Audio is optional for the first playable pass. If implemented, use only a few quiet, original/simple sounds: placement, perfect placement, and game over. Keep all sounds short, provide a simple way to mute only if it can be added without introducing another screen, and do not let missing audio block the MVP.

## Animation boundaries

- No character animation, cutscenes, elaborate particle systems, screen shake, complex physics simulation, or long transitions.
- Do not animate the score in a way that makes it hard to read.
- Do not allow animations to block input longer than necessary.
- Ensure the game remains playable when reduced-motion or low-performance conditions require simpler effects.
