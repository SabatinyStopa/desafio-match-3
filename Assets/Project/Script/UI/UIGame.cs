using Gazeus.DesafioMatch3.Controllers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.UI
{
    public class UIGame : MonoBehaviour
    {
        [SerializeField]
        private UIScore _score;

        [SerializeField]
        private TextMeshProUGUI _currentMovesText;

        [SerializeField]
        private TextMeshProUGUI _levelText;

        [SerializeField]
        private Canvas _gameOverCanvas;

        [SerializeField]
        private Button _restartButton;

        public void SetupRestartButton(UnityEngine.Events.UnityAction action)
        {
            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveAllListeners();
                _restartButton.onClick.AddListener(action);
            }
        }

        public void SetEnableRestartScreen(bool active) => _gameOverCanvas.enabled = active;

        public void SetCurrentMoves(int movesQuantity) =>
            _currentMovesText.SetText($"Moves: {movesQuantity}");

        public void SubscribeEvents(ScoreController scoreController) =>
            scoreController.OnScoreUpdated += _score.UpdateScore;

        public void UnsubscribeEvents(ScoreController scoreController) =>
            scoreController.OnScoreUpdated -= _score.UpdateScore;

        public void SetLevel(int level) => _levelText.SetText($"Level: {level}");

        public void SetTargetScore(int targetScore) => _score.SetTargetScore(targetScore);
    }
}
