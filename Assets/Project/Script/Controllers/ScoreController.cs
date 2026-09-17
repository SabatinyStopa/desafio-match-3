using System;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Controllers
{
    public class ScoreController
    {
        public Action<int, int, int, ResolveType> OnScoreUpdated;
        private int _currentScore;
        private BuffController _buffController;

        private const int BASE_POINTS = 35;

        public ScoreController(BuffController buffController) => _buffController = buffController;

        public void RegisterMatch(
            int tileType,
            int tileCount,
            int comboLevel,
            ResolveType resolveType
        )
        {
            int earnedPoints = Mathf.RoundToInt(tileCount * BASE_POINTS * _buffController.GetMultiplier(resolveType));

            _currentScore += earnedPoints;

            OnScoreUpdated?.Invoke(_currentScore, earnedPoints, comboLevel, resolveType);
        }

        public int GetCurrentScore() => _currentScore;

        public void ResetScore()
        {
            _currentScore = 0;
            OnScoreUpdated?.Invoke(_currentScore, 0, 0, ResolveType.None);
        }
    }
}
