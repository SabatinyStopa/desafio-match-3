using Gazeus.DesafioMatch3.Controllers;
using TMPro;
using UnityEngine;

namespace Gazeus.DesafioMatch3.UI
{
    public class UIGame : MonoBehaviour
    {
        [SerializeField]
        private UIScore _score;

        [SerializeField]
        private TextMeshProUGUI _currentMovesText;

        public void SetCurrentMoves(int movesQuantity) =>
            _currentMovesText.SetText($"Moves: {movesQuantity.ToString()}");

        public void SubscribeEvents(ScoreController scoreController)
        {
            scoreController.OnScoreUpdated += _score.UpdateScore;
        }

        public void UnsubscribeEvents(ScoreController scoreController)
        {
            scoreController.OnScoreUpdated -= _score.UpdateScore;
        }

        public void SetTargetScore(int targetScore) => _score.SetTargetScore(targetScore);
    }
}
