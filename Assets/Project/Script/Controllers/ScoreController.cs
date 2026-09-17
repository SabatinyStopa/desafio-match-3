using System;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Controllers
{
    public class ScoreController
    {
        public Action<int> OnScoreUpdated;
        private int _currentScore;

        private const int BASE_POINTS = 1;

        public void RegisterMatch(int tileType, int tileCount, int comboLevel)
        {
            int earnedPoints = Mathf.RoundToInt(tileCount * BASE_POINTS);

            _currentScore += earnedPoints;

            OnScoreUpdated?.Invoke(_currentScore);
        }

        public int GetCurrentScore() => _currentScore;

        public void ResetScore()
        {
            _currentScore = 0;
            OnScoreUpdated?.Invoke(_currentScore);
        }
    }
}
