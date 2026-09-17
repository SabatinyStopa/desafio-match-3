using System;
using System.Collections.Generic;
using System.Linq;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Controllers
{
    public class BuffController
    {
        private readonly Dictionary<BuffType, float> _activeBuffs = new();
        private readonly Dictionary<BuffType, int> _activeLevels = new();

        private const int MIN_BONUS = 5;
        private const int MAX_BONUS = 30;

        public void ResetBuffs()
        {
            _activeBuffs.Clear();
            _activeLevels.Clear();
        }

        public List<BuffData> GetRandomBuffOptions(int count = 3)
        {
            List<BuffType> allTypes = Enum.GetValues(typeof(BuffType)).Cast<BuffType>().ToList();

            allTypes = allTypes.OrderBy(_ => UnityEngine.Random.value).Take(count).ToList();

            List<BuffData> options = new();

            foreach (var type in allTypes)
            {
                float currentBonus = _activeBuffs.TryGetValue(type, out float accumulated)
                    ? accumulated
                    : 0f;
                int currentLevel = _activeLevels.TryGetValue(type, out int level) ? level : 0;

                int randomPercentage = UnityEngine.Random.Range(MIN_BONUS, MAX_BONUS + 1);
                float rolledBonus = randomPercentage / 100f;

                options.Add(new BuffData(type, currentLevel + 1, currentBonus, rolledBonus));
            }

            return options;
        }

        public void ApplyBuff(BuffData chosenBuff)
        {
            if (!_activeBuffs.ContainsKey(chosenBuff.Type))
            {
                _activeBuffs[chosenBuff.Type] = 0f;
                _activeLevels[chosenBuff.Type] = 0;
            }

            _activeBuffs[chosenBuff.Type] += chosenBuff.RolledBonus;
            _activeLevels[chosenBuff.Type] = chosenBuff.Level;

            int totalPercent = Mathf.RoundToInt(_activeBuffs[chosenBuff.Type] * 100f);
        }

        public float GetMultiplier(ResolveType resolveType)
        {
            return resolveType switch
            {
                ResolveType.Simple => GetMultiplierForPattern(BuffType.Simple),
                ResolveType.Square => GetMultiplierForPattern(BuffType.Square2x2),
                ResolveType.FourSequence => GetMultiplierForPattern(BuffType.Line4),
                ResolveType.FiveSequence => GetMultiplierForPattern(BuffType.Line5),
                _ => 1,
            };
        }

        private float GetMultiplierForPattern(BuffType type)
        {
            if (_activeBuffs.TryGetValue(type, out float totalBonus))
            {
                return 1f + totalBonus;
            }

            return 1f;
        }
    }
}
