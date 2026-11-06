using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StackTower.Core;

namespace StackTower.Gameplay
{
    /// <summary>
    /// Manages the tower blocks, overlap trimming calculations, placement logic,
    /// perfect alignment detection, offcut spawning, and difficulty scaling.
    /// </summary>
    public class TowerController : MonoBehaviour
    {
        public static TowerController Instance { get; private set; }

        [Header("Prefabs & References")]
        [SerializeField] private GameObject _blockPrefab;
        [SerializeField] private GameObject _offcutPrefab;
        [SerializeField] private Transform _towerContainer;
        [SerializeField] private SpriteRenderer _dropIndicator;

        [Header("Block Dimensions")]
        [SerializeField] private float _initialBlockWidth = 3.0f;
        [SerializeField] private float _blockHeight = 0.65f;
        [SerializeField] private float _baseBlockY = -3.5f;

        [Header("Movement & Boundaries")]
        [SerializeField] private float _movementBoundsX = 3.2f;
        [SerializeField] private float _initialSpeed = 3.0f;
        [SerializeField] private float _speedIncrement = 0.08f;
        [SerializeField] private float _maxSpeed = 7.0f;

        [Header("Placement Rules")]
        [SerializeField] private float _perfectToleranceRatio = 0.035f; // Center diff within 3.5% of width
        [SerializeField] private float _minPlayableWidthRatio = 0.10f; // 10% of initial width

        [Header("Visual Palette")]
        [SerializeField] private Color[] _blockPalette = new Color[]
        {
            new Color(0.00f, 0.82f, 1.00f), // Cyan #00D2FF
            new Color(0.23f, 0.51f, 0.96f), // Blue #3B82F6
            new Color(0.61f, 0.32f, 0.88f), // Purple #9B51E0
            new Color(0.93f, 0.28f, 0.60f), // Magenta #EC4899
            new Color(0.96f, 0.25f, 0.37f), // Pink/Coral #F43F5E
            new Color(0.98f, 0.45f, 0.09f), // Orange #F97316
            new Color(0.96f, 0.62f, 0.07f), // Amber #F59E0B
            new Color(0.18f, 0.73f, 0.51f)  // Emerald #2ECC71
        };

        private readonly List<GameObject> _placedBlocks = new List<GameObject>();
        private MovingBlock _currentMovingBlock;
        private Vector2 _currentTopSize;
        private Vector3 _currentTopPos;
        private int _placementCount = 0;
        private int _consecutivePerfects = 0;
        private int _spawnDirection = 1;
        private bool _isProcessingPlacement = false;

        public event System.Action<Vector3, bool, int> OnBlockPlaced; // pos, isPerfect, combo
        public event System.Action<float> OnTowerHeightUpdated; // new top Y

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (_towerContainer == null)
            {
                var containerGo = new GameObject("TowerContainer");
                containerGo.transform.SetParent(transform);
                _towerContainer = containerGo.transform;
            }

            ResetAndStartTower();
        }

        private void Update()
        {
            UpdateDropIndicator();
        }

        public void ResetAndStartTower()
        {
            StopAllCoroutines();
            _isProcessingPlacement = false;
            _placementCount = 0;
            _consecutivePerfects = 0;
            _spawnDirection = 1;

            // Clear previous tower blocks
            foreach (var block in _placedBlocks)
            {
                if (block != null) Destroy(block);
            }
            _placedBlocks.Clear();

            if (_currentMovingBlock != null)
            {
                Destroy(_currentMovingBlock.gameObject);
                _currentMovingBlock = null;
            }

            // Spawn base stationary block
            _currentTopSize = new Vector2(_initialBlockWidth, _blockHeight);
            _currentTopPos = new Vector3(0f, _baseBlockY, 0f);

            GameObject baseBlock = CreateBlockInstance(_currentTopSize, _currentTopPos, GetColorForPlacement(0));
            _placedBlocks.Add(baseBlock);

            OnTowerHeightUpdated?.Invoke(_currentTopPos.y);

            // Spawn first moving block
            SpawnNextMovingBlock();
        }

        public void DropMovingBlock()
        {
            if (_isProcessingPlacement) return;
            if (_currentMovingBlock == null || !_currentMovingBlock.IsMoving) return;
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

            _isProcessingPlacement = true;
            Vector3 dropPos = _currentMovingBlock.StopMovement();

            // Perform trimming math
            ProcessPlacement(dropPos);
        }

        private void ProcessPlacement(Vector3 dropPos)
        {
            float targetWidth = _currentTopSize.x;
            float targetCenterX = _currentTopPos.x;
            float targetLeft = targetCenterX - (targetWidth / 2f);
            float targetRight = targetCenterX + (targetWidth / 2f);

            float movingWidth = _currentTopSize.x;
            float movingCenterX = dropPos.x;
            float movingLeft = movingCenterX - (movingWidth / 2f);
            float movingRight = movingCenterX + (movingWidth / 2f);

            // 1. Calculate Overlap
            float overlapLeft = Mathf.Max(targetLeft, movingLeft);
            float overlapRight = Mathf.Min(targetRight, movingRight);
            float overlapWidth = overlapRight - overlapLeft;

            // Complete Miss!
            if (overlapWidth <= 0f)
            {
                HandleCompleteMiss(dropPos, movingWidth);
                return;
            }

            // Check for Perfect Placement
            float centerDiff = Mathf.Abs(movingCenterX - targetCenterX);
            float perfectTolerance = targetWidth * _perfectToleranceRatio;
            bool isPerfect = centerDiff <= perfectTolerance;

            float minPlayableWidth = _initialBlockWidth * _minPlayableWidthRatio;

            if (isPerfect)
            {
                // Snap to exact center, keep width, no offcut
                _consecutivePerfects++;
                _currentTopPos = new Vector3(targetCenterX, dropPos.y, 0f);
                _currentMovingBlock.SetDimensions(_currentTopSize, _currentTopPos);

                // Play settle animation and score
                FinalizeSuccessfulPlacement(true, 0f, 0f);
            }
            else
            {
                _consecutivePerfects = 0;

                // Check too small
                if (overlapWidth < minPlayableWidth)
                {
                    HandleTooSmall(overlapWidth, overlapLeft, overlapRight, dropPos.y);
                    return;
                }

                // Trim and spawn offcut
                float newCenterX = (overlapLeft + overlapRight) / 2f;
                _currentTopSize = new Vector2(overlapWidth, _blockHeight);
                _currentTopPos = new Vector3(newCenterX, dropPos.y, 0f);

                _currentMovingBlock.SetDimensions(_currentTopSize, _currentTopPos);

                // Calculate offcut piece
                float offcutWidth;
                float offcutCenterX;
                float forceX;

                if (movingCenterX > targetCenterX)
                {
                    offcutWidth = movingRight - targetRight;
                    offcutCenterX = targetRight + (offcutWidth / 2f);
                    forceX = 2.0f;
                }
                else
                {
                    offcutWidth = targetLeft - movingLeft;
                    offcutCenterX = targetLeft - (offcutWidth / 2f);
                    forceX = -2.0f;
                }

                SpawnOffcut(new Vector2(offcutWidth, _blockHeight), new Vector3(offcutCenterX, dropPos.y, 0f), forceX);

                FinalizeSuccessfulPlacement(false, offcutCenterX, offcutWidth);
            }
        }

        private void FinalizeSuccessfulPlacement(bool isPerfect, float offcutCenterX, float offcutWidth)
        {
            _placementCount++;
            int points = isPerfect ? 2 : 1;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(points, isPerfect);
            }

            OnBlockPlaced?.Invoke(_currentTopPos, isPerfect, _consecutivePerfects);
            OnTowerHeightUpdated?.Invoke(_currentTopPos.y);

            // Add moving block to placed blocks
            if (_currentMovingBlock != null)
            {
                _placedBlocks.Add(_currentMovingBlock.gameObject);
                _currentMovingBlock.PlaySettleAnimation(() =>
                {
                    _isProcessingPlacement = false;
                    if (GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Playing)
                    {
                        SpawnNextMovingBlock();
                    }
                });
            }
            else
            {
                _isProcessingPlacement = false;
                SpawnNextMovingBlock();
            }
        }

        private void SpawnNextMovingBlock()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                return;

            float nextY = _currentTopPos.y + _blockHeight;
            float currentSpeed = Mathf.Min(_maxSpeed, _initialSpeed + (_placementCount * _speedIncrement));

            // Alternate starting direction
            _spawnDirection = -_spawnDirection;

            Color blockColor = GetColorForPlacement(_placementCount + 1);

            GameObject blockGo = Instantiate(_blockPrefab, _towerContainer);
            _currentMovingBlock = blockGo.GetComponent<MovingBlock>();

            if (_currentMovingBlock == null)
            {
                _currentMovingBlock = blockGo.AddComponent<MovingBlock>();
            }

            _currentMovingBlock.Initialize(
                _currentTopSize.x,
                _blockHeight,
                nextY,
                currentSpeed,
                _spawnDirection,
                blockColor,
                _movementBoundsX
            );
        }

        private void HandleCompleteMiss(Vector3 dropPos, float width)
        {
            // Drop entire block as falling piece
            if (_currentMovingBlock != null)
            {
                Destroy(_currentMovingBlock.gameObject);
                _currentMovingBlock = null;
            }

            float forceX = dropPos.x > _currentTopPos.x ? 2.5f : -2.5f;
            SpawnOffcut(new Vector2(width, _blockHeight), dropPos, forceX);

            TriggerGameOver();
        }

        private void HandleTooSmall(float overlapWidth, float overlapLeft, float overlapRight, float dropY)
        {
            // Show the tiny sliver and game over
            float newCenterX = (overlapLeft + overlapRight) / 2f;
            _currentMovingBlock.SetDimensions(new Vector2(overlapWidth, _blockHeight), new Vector3(newCenterX, dropY, 0f));
            _placedBlocks.Add(_currentMovingBlock.gameObject);

            TriggerGameOver();
        }

        private void TriggerGameOver()
        {
            _isProcessingPlacement = true;
            if (_dropIndicator != null)
            {
                _dropIndicator.gameObject.SetActive(false);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }

        private void SpawnOffcut(Vector2 size, Vector3 position, float forceX)
        {
            if (_offcutPrefab == null) return;

            GameObject offcutGo = Instantiate(_offcutPrefab, position, Quaternion.identity);
            BlockPiece piece = offcutGo.GetComponent<BlockPiece>();
            if (piece == null)
            {
                piece = offcutGo.AddComponent<BlockPiece>();
            }

            Color color = GetColorForPlacement(_placementCount + 1);
            float torque = forceX > 0 ? -120f : 120f;
            piece.Initialize(size, color, forceX, torque);
        }

        private GameObject CreateBlockInstance(Vector2 size, Vector3 pos, Color color)
        {
            GameObject blockGo = Instantiate(_blockPrefab, pos, Quaternion.identity, _towerContainer);
            MovingBlock mb = blockGo.GetComponent<MovingBlock>();
            if (mb != null)
            {
                mb.SetDimensions(size, pos);
                mb.SetColor(color);
                mb.StopMovement();
            }
            return blockGo;
        }

        public Color GetColorForPlacement(int index)
        {
            if (_blockPalette == null || _blockPalette.Length == 0)
                return Color.white;

            int paletteIdx = index % _blockPalette.Length;
            return _blockPalette[paletteIdx];
        }

        private void UpdateDropIndicator()
        {
            if (_dropIndicator == null) return;

            if (_currentMovingBlock != null && _currentMovingBlock.IsMoving &&
                GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                _dropIndicator.gameObject.SetActive(true);
                // Position indicator directly over the top of the tower target
                _dropIndicator.transform.position = new Vector3(_currentTopPos.x, _currentTopPos.y + _blockHeight, 0f);
                _dropIndicator.drawMode = SpriteDrawMode.Sliced;
                _dropIndicator.size = _currentTopSize;
                _dropIndicator.transform.localScale = Vector3.one;
            }
            else
            {
                _dropIndicator.gameObject.SetActive(false);
            }
        }
    }
}
