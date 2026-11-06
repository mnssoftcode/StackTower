# Stack Tower — MVP Acceptance Checklist

The MVP is complete only when the relevant items below have been implemented and verified. Mark platform-specific items as **Not tested** when the required device, build module, browser, or signing setup is unavailable.

## Project and scenes

- [ ] The project opens in its existing Unity version without new compile errors.
- [ ] `MainMenu.unity` exists and is included in the build scene list.
- [ ] `Gameplay.unity` exists and is included in the build scene list.
- [ ] Game Over is an overlay in the Gameplay scene, not a third scene.
- [ ] Scripts, prefabs, and assets are organized under the project folder described in the structure guide.

## Main menu

- [ ] The game title is visible.
- [ ] The saved best score is displayed.
- [ ] The Play button loads Gameplay and starts a fresh run.
- [ ] The layout stays readable in the portrait play area.

## Gameplay

- [ ] A stationary base block appears at the start of a new run.
- [ ] A moving block travels horizontally above the tower.
- [ ] The block reverses direction at its movement boundaries.
- [ ] One tap or click drops exactly one block.
- [ ] The overlap calculation trims the correct overhanging section.
- [ ] A perfect placement snaps to center and awards the bonus point.
- [ ] The next block uses the retained block's width.
- [ ] The score updates correctly.
- [ ] Movement speed increases gradually and respects its maximum.
- [ ] The camera follows the tower without moving screen-space UI.
- [ ] Offcuts fall away and do not disturb the tower.
- [ ] Input does not double-trigger.

## Game Over and saving

- [ ] A complete miss ends the run.
- [ ] A block below the minimum playable width ends the run.
- [ ] Game Over stops movement and ignores additional gameplay input.
- [ ] Final score and best score are visible.
- [ ] A new best score is clearly indicated.
- [ ] Best score persists after leaving and reopening the game.
- [ ] Restart resets the run but preserves the saved best score.
- [ ] Main Menu returns to the main menu scene.

## UI and visual quality

- [ ] There are exactly 3 buttons: Play, Restart, and Main Menu.
- [ ] Gameplay uses tap/click anywhere in the play area; no drop button is added.
- [ ] The UI contains only the elements defined in the screen specification.
- [ ] Text has sufficient contrast and is not obscured by safe areas.
- [ ] Blocks and background follow the minimal premium visual guide.
- [ ] Placement, perfect, camera, and Game Over feedback are smooth and brief.

## Platform and orientation

- [ ] Android is configured for portrait and tested on a device or emulator.
- [ ] iOS is configured for portrait; an actual iOS build is tested when the required Apple build environment is available.
- [ ] Windows build launches and accepts mouse input.
- [ ] macOS build launches and accepts mouse input.
- [ ] Linux build launches and accepts mouse input.
- [ ] Web build launches in a supported browser and accepts mouse input.
- [ ] On wide desktop/web screens, the play area remains centered and portrait rather than stretching.
- [ ] On narrow screens, the game scales to fit without cropping essential content.

## Final report from the development agent

At completion, report:

1. What was implemented.
2. The final scene and script list.
3. What was tested in Unity Editor.
4. Which platform builds were actually created and tested.
5. Any remaining blocker that requires a device, platform module, browser, signing credential, or manual setup.

Do not mark a platform as tested based only on selecting it in Build Settings.
