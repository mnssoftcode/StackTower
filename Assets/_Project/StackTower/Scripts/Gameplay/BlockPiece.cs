using UnityEngine;

namespace StackTower.Gameplay
{
    /// <summary>
    /// Represents an offcut piece trimmed from an overhanging block.
    /// Falls under gravity and self-destructs after a short lifetime.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class BlockPiece : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private float _lifetime = 2.5f;

        private void Awake()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_rigidbody2D == null)
                _rigidbody2D = GetComponent<Rigidbody2D>();

            // Ensure offcut does not collide with tower blocks
            if (TryGetComponent<Collider2D>(out var col))
            {
                col.isTrigger = true;
            }

            Destroy(gameObject, _lifetime);
        }

        public void Initialize(Vector2 size, Color color, float horizontalForce = 0f, float torque = 0f)
        {
            transform.localScale = Vector3.one;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.drawMode = SpriteDrawMode.Sliced;
                _spriteRenderer.size = size;
                _spriteRenderer.color = color;
            }

            if (TryGetComponent<BoxCollider2D>(out var col))
            {
                col.size = size;
                col.isTrigger = true;
            }

            if (_rigidbody2D != null)
            {
                _rigidbody2D.gravityScale = 3.5f;
                _rigidbody2D.linearVelocity = new Vector2(horizontalForce, -0.5f);
                _rigidbody2D.angularVelocity = torque;
            }
        }
    }
}
