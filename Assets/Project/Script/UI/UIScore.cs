using System.Collections;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Utilities;
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

        [SerializeField]
        private UISequenceText _sequenceTextPrefab;

        [SerializeField]
        private Transform _sequenceTextContainer;

        private float _countDuration = 0.4f;
        private Coroutine _countCoroutine;
        private int _displayedScore;

        private ObjectPool<UISequenceText> _sequenceTextsPool;

        private void Awake()
        {
            _sequenceTextsPool = new ObjectPool<UISequenceText>(
                _sequenceTextPrefab,
                15,
                _sequenceTextContainer
            );
        }

        public void SetTargetScore(int targetScore) =>
            _targetScoreText.SetText($"Target: {targetScore}");

        public void UpdateScore(
            int currentScore,
            int earnedPoints,
            int matchType,
            int comboLevel,
            ResolveType type
        )
        {
            if (_countCoroutine != null)
            {
                StopCoroutine(_countCoroutine);
            }

            if (earnedPoints > 0)
            {
                _sequenceTextsPool
                    .Get()
                    .SetText(
                        $"<b><size=+3><color=#00FFFF>+{earnedPoints} pts</color> <color=#FFD700>Combo x{comboLevel}</color></size>\n{GetTypeName(type)}</b>",
                        _sequenceTextsPool
                    );
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

        private static string GetTypeName(ResolveType type)
        {
            return type switch
            {
                ResolveType.Simple => "Simples 3",
                ResolveType.Square => "Quadrado 2x2",
                ResolveType.FourSequence => "Sequência de 4",
                ResolveType.FiveSequence => "Sequência de 5",
                _ => string.Empty,
            };
        }
    }
}
