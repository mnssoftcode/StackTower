using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using StackTower.Core;

namespace StackTower.UI
{
    /// <summary>
    /// Manages the Main Menu UI elements, best score display, and Play button.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button _playButton;
        [SerializeField] private TextMeshProUGUI _bestScoreText;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _instructionText;

        private void Start()
        {
            if (_playButton != null)
            {
                _playButton.onClick.AddListener(OnPlayClicked);
            }

            int bestScore = PlayerPrefs.GetInt(GameManager.BestScoreKey, 0);
            if (_bestScoreText != null)
            {
                _bestScoreText.text = $"Best: {bestScore}";
            }

            if (_titleText != null)
            {
                _titleText.text = "STACK\nTOWER";
            }

            if (_instructionText != null)
            {
                _instructionText.text = "Tap to stack. Build as high as you can.";
            }
        }

        private void OnPlayClicked()
        {
            SceneManager.LoadScene(GameManager.GameplaySceneName);
        }

        private void OnDestroy()
        {
            if (_playButton != null)
            {
                _playButton.onClick.RemoveListener(OnPlayClicked);
            }
        }
    }
}
