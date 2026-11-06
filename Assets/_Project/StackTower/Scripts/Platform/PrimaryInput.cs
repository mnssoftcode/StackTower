using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using StackTower.Core;
using StackTower.Gameplay;

namespace StackTower.Platform
{
    /// <summary>
    /// Centralized input handler for tap and click actions.
    /// Filters out UI interactions and ensures a single drop action per frame.
    /// </summary>
    public class PrimaryInput : MonoBehaviour
    {
        [SerializeField] private bool _enableKeyboardShortcuts = true;

        private int _lastTriggerFrame = -1;

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                return;

            if (Time.frameCount == _lastTriggerFrame)
                return;

            if (WasPrimaryActionPressedThisFrame())
            {
                if (IsPointerOverUI())
                    return;

                _lastTriggerFrame = Time.frameCount;

                if (TowerController.Instance != null)
                {
                    TowerController.Instance.DropMovingBlock();
                }
            }
        }

        private bool WasPrimaryActionPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            // New Input System
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                return true;
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                return true;
            }

            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            {
                return true;
            }

            if (_enableKeyboardShortcuts && Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
                {
                    return true;
                }
            }
#else
            // Legacy Input Manager fallback
            if (Input.GetMouseButtonDown(0))
            {
                return true;
            }

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                return true;
            }

            if (_enableKeyboardShortcuts && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
            {
                return true;
            }
#endif

            return false;
        }

        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null) return false;

#if ENABLE_INPUT_SYSTEM
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                int touchId = Touchscreen.current.primaryTouch.touchId.ReadValue();
                if (EventSystem.current.IsPointerOverGameObject(touchId))
                    return true;
            }

            return EventSystem.current.IsPointerOverGameObject();
#else
            if (Input.touchCount > 0)
            {
                return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            }

            return EventSystem.current.IsPointerOverGameObject();
#endif
        }
    }
}

