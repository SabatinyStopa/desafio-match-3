using System.Collections;
using TMPro;
using UnityEngine;

namespace Gazeus.DesafioMatch3.UI
{
    public class UIScore : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _scoreText;

        [SerializeField]
        private TextMeshProUGUI _targetScoreText;

        private float _countDuration = 0.4f;
        private Coroutine _countCoroutine;
        private int _displayedScore;

        public void SetTargetScore(int targetScore) =>
            _targetScoreText.SetText($"Target: {targetScore}");

        public void UpdateScore(int currentScore)
        {
            if (_countCoroutine != null)
            {
                StopCoroutine(_countCoroutine);
            }

            _countCoroutine = StartCoroutine(IncreaseGradually(currentScore));
        }

        private IEnumerator IncreaseGradually(int targetScore)
        {
            int startScore = _displayedScore;
            float elapsedTime = 0f;

            while (elapsedTime < _countDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / _countDuration;

                _displayedScore = Mathf.RoundToInt(Mathf.Lerp(startScore, targetScore, progress));
                _scoreText.SetText($"Score: {_displayedScore}");

                yield return null;
            }

            _displayedScore = targetScore;
            _scoreText.SetText($"Score: {targetScore}");
            _countCoroutine = null;
        }
    }
}
