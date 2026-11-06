using System.Collections;
using UnityEngine;

namespace StackTower.Gameplay
{
    /// <summary>
    /// Manages the horizontal movement, direction reversal, and stopping of an active block.
    /// Uses 9-sliced SpriteRenderer sizing to keep rounded corners crisp at any width.
    /// </summary>
    public class MovingBlock : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteRenderer _highlightRenderer;

        [Header("Movement Settings")]
        [SerializeField] private float _boundsX = 3.2f;

        private float _speed = 3.5f;
        private int _direction = 1;
        private bool _isMoving = false;
        private Vector2 _currentDimensions = Vector2.one;

        public bool IsMoving => _isMoving;
        public Vector2 CurrentDimensions => _currentDimensions;
        public SpriteRenderer SpriteRenderer => _spriteRenderer;

        private void Awake()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(
            float width,
            float height,
            float yPos,
            float speed,
            int direction,
            Color blockColor,
            float boundsX)
        {
            _currentDimensions = new Vector2(width, height);
            _speed = speed;
            _direction = direction >= 0 ? 1 : -1;
            _boundsX = boundsX;

            float startX = _direction > 0 ? -_boundsX : _boundsX;
            transform.position = new Vector3(startX, yPos, 0f);
            SetDimensions(_currentDimensions, transform.position);

            SetColor(blockColor);
            _isMoving = true;
        }

        private void Update()
        {
            if (!_isMoving) return;

            float newX = transform.position.x + (_direction * _speed * Time.deltaTime);

            if (newX >= _boundsX)
            {
                newX = _boundsX;
                _direction = -1;
            }
            else if (newX <= -_boundsX)
            {
                newX = -_boundsX;
                _direction = 1;
            }

            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }

        public Vector3 StopMovement()
        {
            _isMoving = false;
            return transform.position;
        }

        public void SetDimensions(Vector2 size, Vector3 centerPosition)
        {
            _currentDimensions = size;
            transform.position = centerPosition;
            transform.localScale = Vector3.one;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.drawMode = SpriteDrawMode.Sliced;
                _spriteRenderer.size = size;
            }

            if (TryGetComponent<BoxCollider2D>(out var col))
            {
                col.size = size;
            }
        }

        public void SetColor(Color color)
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = color;
            }

            if (_highlightRenderer != null)
            {
                _highlightRenderer.color = new Color(1f, 1f, 1f, 0.35f);
            }
        }

        public void PlaySettleAnimation(System.Action onComplete = null)
        {
            StartCoroutine(SettleCoroutine(onComplete));
        }

        private IEnumerator SettleCoroutine(System.Action onComplete)
        {
            Vector3 baseScale = Vector3.one;
            Vector3 squishScale = new Vector3(1.02f, 0.92f, 1f);

            float duration = 0.1f;
            float elapsed = 0f;

            // Squish down slightly
            while (elapsed < duration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration * 0.5f);
                transform.localScale = Vector3.Lerp(baseScale, squishScale, t);
                yield return null;
            }

            elapsed = 0f;
            // Pop back to base
            while (elapsed < duration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration * 0.5f);
                transform.localScale = Vector3.Lerp(squishScale, baseScale, t);
                yield return null;
            }

            transform.localScale = baseScale;
            onComplete?.Invoke();
        }
    }
}
