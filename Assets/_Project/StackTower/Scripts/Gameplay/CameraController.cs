using UnityEngine;
using StackTower.Gameplay;

namespace StackTower.Gameplay
{
    /// <summary>
    /// Smoothly moves the orthographic camera upward as the tower height increases.
    /// Adapts camera orthographic size to guarantee the 9:16 playfield is always visible.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private float _verticalOffset = -1.2f; // Keeps tower top near lower-middle area
        [SerializeField] private float _smoothSpeed = 5.0f;
        [SerializeField] private float _minCameraY = 0.0f;

        [Header("Responsive Settings")]
        [SerializeField] private float _baseOrthoSize = 5.0f;
        [SerializeField] private float _minHalfWidth = 3.8f; // Ensure bounds of movement are always inside view

        private Camera _camera;
        private float _targetY = 0f;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _targetY = _minCameraY;
            AdjustOrthographicSize();
        }

        private void Start()
        {
            if (TowerController.Instance != null)
            {
                TowerController.Instance.OnTowerHeightUpdated += HandleTowerHeightUpdated;
            }
            AdjustOrthographicSize();
        }

        private void Update()
        {
            AdjustOrthographicSize();

            Vector3 currentPos = transform.position;
            float newY = Mathf.Lerp(currentPos.y, _targetY, Time.deltaTime * _smoothSpeed);
            transform.position = new Vector3(0f, newY, currentPos.z);
        }

        private void HandleTowerHeightUpdated(float towerTopY)
        {
            // Camera target rises so top of tower is positioned comfortably
            float desiredY = towerTopY - _verticalOffset;
            _targetY = Mathf.Max(_minCameraY, desiredY);
        }

        private void AdjustOrthographicSize()
        {
            if (_camera == null || !_camera.orthographic) return;

            float currentAspect = _camera.aspect;
            float visibleHalfWidth = _baseOrthoSize * currentAspect;

            if (visibleHalfWidth < _minHalfWidth)
            {
                // Screen is narrower than standard, expand ortho size so width is preserved
                _camera.orthographicSize = _minHalfWidth / currentAspect;
            }
            else
            {
                _camera.orthographicSize = _baseOrthoSize;
            }
        }

        private void OnDestroy()
        {
            if (TowerController.Instance != null)
            {
                TowerController.Instance.OnTowerHeightUpdated -= HandleTowerHeightUpdated;
            }
        }
    }
}
