using UnityEngine;

namespace StackTower.Gameplay
{
    /// <summary>
    /// Manages the twilight mountain scenery, starry sky, and responsive background sizing.
    /// Follows the camera vertically with subtle parallax.
    /// </summary>
    public class BackgroundController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private Transform _sceneryRoot;

        [Header("Parallax Settings")]
        [SerializeField] private float _parallaxFactor = 0.85f; // Follows camera smoothly
        [SerializeField] private float _sceneryInitialY = 0f;

        private void Start()
        {
            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }

            if (_sceneryRoot == null)
            {
                _sceneryRoot = transform;
            }

            _sceneryInitialY = _sceneryRoot.position.y;
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null || _sceneryRoot == null) return;

            // Follow camera X completely so wide screens stay centered
            // Follow camera Y with gentle parallax
            float cameraY = _cameraTransform.position.y;
            float newY = _sceneryInitialY + (cameraY * _parallaxFactor);

            _sceneryRoot.position = new Vector3(_cameraTransform.position.x, newY, _sceneryRoot.position.z);
        }
    }
}
