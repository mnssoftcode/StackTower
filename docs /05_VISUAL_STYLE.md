# Stack Tower — Visual Style Guide

## Design direction

Create a minimal, premium 2D arcade game. The presentation should feel calm, precise, modern, and polished—not childish, noisy, or visually crowded.

## Overall look

- Flat 2D shapes with clean edges.
- Light, warm neutral background.
- Solid-color rectangular blocks with subtle depth from a restrained shadow or slightly darker side edge.
- Strong contrast between blocks, text, and background.
- Consistent spacing and alignment.
- No complex illustrations, characters, 3D models, or detailed environments.
Main Menu: Game title, Play button and best score.



Gameplay: Moving blocks, tower, current score and pause.



Perfect Stack: Special visual feedback for perfect placement.



Game Over: Final score, restart and main menu.

## Suggested color palette

Use these as starting values; small adjustments are allowed if contrast and consistency improve.

| Purpose | Color | Hex |
|---|---|---|
| Main background | Warm ivory | `#F5F4F0` |
| Main text | Deep charcoal | `#20232B` |
| Secondary text | Muted slate | `#747984` |
| Block 1 | Deep indigo | `#5865A8` |
| Block 2 | Muted teal | `#4F9B91` |
| Block 3 | Soft coral | `#D98170` |
| Block 4 | Warm ochre | `#D5A64B` |
| Button/accent | Deep indigo | `#46538F` |
| Overlay | Charcoal with transparency | `#20232B` at roughly 45–55% opacity |

Cycle through the block palette in a predictable order. Do not use random colors on every frame or placement.

## Typography

- Use one clean sans-serif family throughout.
- Use a bold or semibold weight for the game title and score.
- Use regular/medium weight for instructions and secondary labels.
- Keep the score large enough to read quickly on a phone.
- Avoid decorative fonts, all-caps paragraphs, and excessive text.
- Use TextMeshPro when available; otherwise use the project's existing UI text solution.

## Shape and layout rules

- Blocks are simple rectangles with square or very slightly softened corners. Do not make the stacking geometry visually misleading with excessive rounded corners.
- Buttons use a consistent shape and style.
- Keep the game board visually dominant.
- Keep UI away from device notches, rounded corners, and system gesture areas.
- Use only subtle shadows; do not add heavy glow, bevel, or 3D extrusion effects.

## Asset rules

- Build the base blocks, moving blocks, offcuts, simple background, and UI with Unity primitives or simple generated sprites.
- No paid asset packs are needed.
- Do not use third-party art that introduces a different style.
- No external character art or background scenery is required.

## Visual restraint

Do not add decorative icons, currency symbols, badges, menus, particles, or background objects unless they directly improve feedback for the core stacking action. The finished result should remain readable and uncluttered on a small phone screen.
