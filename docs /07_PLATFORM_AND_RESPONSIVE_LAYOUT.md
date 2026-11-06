# Stack Tower — Platform and Responsive Layout

## Required targets

Prepare the project for these targets:

- Android
- iOS
- Windows
- macOS
- Linux
- Web

The MVP must use one shared gameplay implementation. Do not create separate gameplay versions for each platform.

## Orientation and aspect ratio

### Mobile

- The game is designed to be played vertically in portrait orientation.
- Set Android and iOS orientation to **Portrait** for the MVP.
- Use a logical portrait composition with a target aspect ratio of **9:16**.
- Do not rotate the gameplay to landscape on phones or tablets.
- Respect safe areas so UI is not hidden by notches, camera cutouts, rounded corners, or system gesture areas.

### Desktop and Web

- Keep the game board itself in a centered portrait 9:16 play area.
- On wide screens, show the portrait play area in the center and allow the surrounding space to remain a simple background. Do not stretch the playfield horizontally.
- On a narrow browser or desktop window, scale the portrait play area down to fit without cropping the tower, score, or buttons.
- The game can run in a normal desktop window; it does not need to force the entire monitor into portrait orientation.
- Keep mouse controls available on Windows, macOS, Linux, and Web.

## Canvas and camera

- Use a Screen Space - Overlay Canvas for UI.
- Configure the Canvas Scaler to `Scale With Screen Size` with a portrait reference resolution such as **1080 × 1920**.
- Use a sensible width/height match setting (approximately 0.5) and test on tall and wide aspect ratios.
- Place score and menu UI within the safe area.
- Use an orthographic camera for the 2D playfield.
- Adapt the visible world area to the actual display ratio without changing the 9:16 logical composition.
- Do not allow blocks to spawn or move outside the visible horizontal gameplay bounds.

## Input

- Mobile: one-finger tap in the gameplay area drops the moving block.
- Desktop/Web: primary mouse click in the gameplay area drops the moving block.
- Centralize primary input in one input component or service.
- Prevent the same tap/click from being processed twice.
- Ignore input while a UI button is being pressed, during scene transitions, and after Game Over.
- Do not require a keyboard to play. Keyboard support is optional and not required for the MVP.

## Performance and implementation

- Prefer simple sprites, transforms, and lightweight UI.
- Avoid unnecessary per-frame allocations and repeated scene-wide object searches.
- Keep offcuts short-lived and clean them up.
- Avoid heavy post-processing and large textures.
- Test the game at more than one aspect ratio and at a lower-performance mobile resolution if device testing is available.

## Build preparation

- Configure the project for the six listed target families using the Unity version and platform modules installed on the development machine.
- Add the two game scenes to the build scene list.
- Do not claim that a platform build has been tested unless an actual build has been created and launched.
- iOS packaging/signing and some platform-specific build steps may require the appropriate Apple development environment and credentials. Report such requirements rather than pretending they are complete.
- Web builds must be checked in a real supported browser after export; Editor Play Mode alone does not verify Web behavior.

## Cross-platform acceptance rule

The core game rules, scoring, saved best score behavior, and screen flow must remain the same on every target. Only input handling, window sizing, and platform build settings may differ.
