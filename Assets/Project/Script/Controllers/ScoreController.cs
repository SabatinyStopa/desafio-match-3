using System;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Controllers
{
    public class ScoreController
    {
        public Action<int, int, int, ResolveType> OnScoreUpdated;
        private int _currentScore;

        private const int BASE_POINTS = 1;

        public void RegisterMatch(
            int tileType,
            int tileCount,
            int comboLevel,
            ResolveType resolveType
        )
        {
            int earnedPoints = Mathf.RoundToInt(tileCount * BASE_POINTS * comboLevel);

            _currentScore += earnedPoints;

            OnScoreUpdated?.Invoke(_currentScore, tileCount * BASE_POINTS, comboLevel, resolveType);
        }

        public int GetCurrentScore() => _currentScore;

        public void ResetScore()
        {
            _currentScore = 0;
            OnScoreUpdated?.Invoke(_currentScore, 0, 0, ResolveType.None);
        }
    }
}
