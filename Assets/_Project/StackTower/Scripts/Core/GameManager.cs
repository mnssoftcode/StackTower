using UnityEngine;
using UnityEngine.SceneManagement;

namespace StackTower.Core
{
    /// <summary>
    /// Coordinates the high-level run state, score tracking, and local best score persistence.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public const string BestScoreKey = "StackTower_BestScore_v1";
        public const string MainMenuSceneName = "MainMenu";
        public const string GameplaySceneName = "Gameplay";

        [Header("State")]
        [SerializeField] private GameState _currentState = GameState.Playing;

        public GameState CurrentState => _currentState;
        public int CurrentScore { get; private set; }
        public int BestScore { get; private set; }
        public bool IsNewBest { get; private set; }

        public event System.Action<int, int, bool> OnScoreChanged;
        public event System.Action<GameState> OnStateChanged;
        public event System.Action<int, int, bool> OnGameOver;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            LoadBestScore();
        }

        private void Start()
        {
            // Initial score event
            OnScoreChanged?.Invoke(CurrentScore, 0, false);
        }

        public void LoadBestScore()
        {
            BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        }

        public void StartGame()
        {
            CurrentScore = 0;
            IsNewBest = false;
            LoadBestScore();
            SetState(GameState.Playing);
            OnScoreChanged?.Invoke(CurrentScore, 0, false);
        }

        public void AddScore(int points, bool isPerfect)
        {
            if (_currentState != GameState.Playing) return;

            CurrentScore += points;

            if (CurrentScore > BestScore)
            {
                BestScore = CurrentScore;
                IsNewBest = true;
                PlayerPrefs.SetInt(BestScoreKey, BestScore);
                PlayerPrefs.Save();
            }

            OnScoreChanged?.Invoke(CurrentScore, points, isPerfect);
        }

        public void TriggerGameOver()
        {
            if (_currentState == GameState.GameOver) return;

            SetState(GameState.GameOver);

            if (CurrentScore > BestScore)
            {
                BestScore = CurrentScore;
                IsNewBest = true;
            }

            PlayerPrefs.SetInt(BestScoreKey, BestScore);
            PlayerPrefs.Save();

            OnGameOver?.Invoke(CurrentScore, BestScore, IsNewBest);
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(GameplaySceneName);
        }

        public void GoToMainMenu()
        {
            SceneManager.LoadScene(MainMenuSceneName);
        }

        private void SetState(GameState newState)
        {
            _currentState = newState;
            OnStateChanged?.Invoke(_currentState);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}

