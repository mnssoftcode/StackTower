using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StackTower.Core;
using StackTower.Gameplay;

namespace StackTower.UI
{
    /// <summary>
    /// Coordinates HUD elements (current score, best score, combo feedback, hint)
    /// and the Game Over modal overlay with Restart and Main Menu buttons.
    /// </summary>
    public class GameplayUI : MonoBehaviour
    {
        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI _currentScoreText;
        [SerializeField] private TextMeshProUGUI _hudBestScoreText;
        [SerializeField] private TextMeshProUGUI _tapHintText;
        [SerializeField] private TextMeshProUGUI _perfectFeedbackText;
        [SerializeField] private TextMeshProUGUI _scoreDeltaText;

        [Header("Game Over Overlay")]
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private CanvasGroup _gameOverCanvasGroup;
        [SerializeField] private TextMeshProUGUI _finalScoreText;
        [SerializeField] private TextMeshProUGUI _gameOverBestScoreText;
        [SerializeField] private GameObject _newBestBadge;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _mainMenuButton;

        private Coroutine _perfectFeedbackCoroutine;
        private Coroutine _deltaScoreCoroutine;
        private bool _hasDroppedFirstBlock = false;

        private void Awake()
        {
            if (_gameOverPanel != null)
                _gameOverPanel.SetActive(false);

            if (_perfectFeedbackText != null)
                _perfectFeedbackText.gameObject.SetActive(false);

            if (_scoreDeltaText != null)
                _scoreDeltaText.gameObject.SetActive(false);

            if (_newBestBadge != null)
                _newBestBadge.SetActive(false);
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += HandleScoreChanged;
                GameManager.Instance.OnGameOver += HandleGameOver;

                UpdateScoreDisplay(GameManager.Instance.CurrentScore);
                UpdateBestScoreDisplay(GameManager.Instance.BestScore);
            }

            if (TowerController.Instance != null)
            {
                TowerController.Instance.OnBlockPlaced += HandleBlockPlaced;
            }

            if (_restartButton != null)
                _restartButton.onClick.AddListener(OnRestartClicked);

            if (_mainMenuButton != null)
                _mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            if (_tapHintText != null)
            {
                _tapHintText.text = "TAP TO DROP";
                _tapHintText.gameObject.SetActive(true);
            }
        }

        private void HandleScoreChanged(int currentScore, int delta, bool isPerfect)
        {
            UpdateScoreDisplay(currentScore);

            if (!_hasDroppedFirstBlock && delta > 0)
            {
                _hasDroppedFirstBlock = true;
                if (_tapHintText != null)
                {
                    _tapHintText.gameObject.SetActive(false);
                }
            }

            if (delta > 0)
            {
                ShowScoreDelta(delta, isPerfect);
            }
        }

        private void HandleBlockPlaced(Vector3 pos, bool isPerfect, int combo)
        {
            if (!_hasDroppedFirstBlock)
            {
                _hasDroppedFirstBlock = true;
                if (_tapHintText != null)
                {
                    _tapHintText.gameObject.SetActive(false);
                }
            }

            if (isPerfect)
            {
                ShowPerfectFeedback(combo);
            }
        }

        private void ShowPerfectFeedback(int combo)
        {
            if (_perfectFeedbackText == null) return;

            if (_perfectFeedbackCoroutine != null)
                StopCoroutine(_perfectFeedbackCoroutine);

            _perfectFeedbackCoroutine = StartCoroutine(PerfectFeedbackRoutine(combo));
        }

        private IEnumerator PerfectFeedbackRoutine(int combo)
        {
            _perfectFeedbackText.gameObject.SetActive(true);
            _perfectFeedbackText.text = combo > 1 ? $"PERFECT! x{combo}" : "PERFECT!";

            Transform t = _perfectFeedbackText.transform;
            t.localScale = Vector3.one * 0.5f;

            float duration = 0.55f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                // Pop scale
                float scale = (progress < 0.3f)
                    ? Mathf.Lerp(0.5f, 1.25f, progress / 0.3f)
                    : Mathf.Lerp(1.25f, 1.0f, (progress - 0.3f) / 0.7f);

                t.localScale = Vector3.one * scale;

                yield return null;
            }

            _perfectFeedbackText.gameObject.SetActive(false);
        }

        private void ShowScoreDelta(int delta, bool isPerfect)
        {
            if (_scoreDeltaText == null) return;

            if (_deltaScoreCoroutine != null)
                StopCoroutine(_deltaScoreCoroutine);

            _deltaScoreCoroutine = StartCoroutine(ScoreDeltaRoutine(delta, isPerfect));
        }

        private IEnumerator ScoreDeltaRoutine(int delta, bool isPerfect)
        {
            _scoreDeltaText.gameObject.SetActive(true);
            _scoreDeltaText.text = $"+{delta}";
            _scoreDeltaText.color = isPerfect ? new Color(0.2f, 1f, 0.6f, 1f) : new Color(1f, 0.9f, 0.3f, 1f);

            Vector3 startPos = _scoreDeltaText.transform.localPosition;
            Vector3 endPos = startPos + new Vector3(0f, 30f, 0f);

            float duration = 0.45f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                _scoreDeltaText.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            _scoreDeltaText.transform.localPosition = startPos;
            _scoreDeltaText.gameObject.SetActive(false);
        }

        private void UpdateScoreDisplay(int score)
        {
            if (_currentScoreText != null)
            {
                _currentScoreText.text = score.ToString();
            }
        }

        private void UpdateBestScoreDisplay(int bestScore)
        {
            if (_hudBestScoreText != null)
            {
                _hudBestScoreText.text = $"Best: {bestScore}";
            }
        }

        private void HandleGameOver(int finalScore, int bestScore, bool isNewBest)
        {
            if (_tapHintText != null)
                _tapHintText.gameObject.SetActive(false);

            if (_finalScoreText != null)
                _finalScoreText.text = finalScore.ToString();

            if (_gameOverBestScoreText != null)
                _gameOverBestScoreText.text = $"Best: {bestScore}";

            if (_newBestBadge != null)
                _newBestBadge.SetActive(isNewBest);

            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
                StartCoroutine(FadeInGameOver());
            }
        }

        private IEnumerator FadeInGameOver()
        {
            if (_gameOverCanvasGroup != null)
            {
                _gameOverCanvasGroup.alpha = 0f;
                float duration = 0.22f;
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    _gameOverCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                    yield return null;
                }

                _gameOverCanvasGroup.alpha = 1f;
            }
        }

        private void OnRestartClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }

        private void OnMainMenuClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToMainMenu();
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
                GameManager.Instance.OnGameOver -= HandleGameOver;
            }

            if (TowerController.Instance != null)
            {
                TowerController.Instance.OnBlockPlaced -= HandleBlockPlaced;
            }

            if (_restartButton != null)
                _restartButton.onClick.RemoveListener(OnRestartClicked);

            if (_mainMenuButton != null)
                _mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }
    }
}

