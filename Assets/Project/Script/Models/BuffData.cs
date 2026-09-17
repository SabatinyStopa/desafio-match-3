using UnityEngine;

namespace Gazeus.DesafioMatch3.Models
{
    public class BuffData
    {
        public BuffType Type { get; private set; }
        public int Level { get; private set; }
        public float CurrentTotalBonus { get; private set; }
        public float RolledBonus { get; private set; }

        public BuffData(BuffType type, int level, float currentTotalBonus, float rolledBonus)
        {
            Type = type;
            Level = level;
            CurrentTotalBonus = currentTotalBonus;
            RolledBonus = rolledBonus;
        }

        public string GetTitle() =>
            Type switch
            {
                BuffType.Simple => $"Trinca Simples (Nv. {Level})",
                BuffType.Square2x2 => $"Quadrado 2x2 (Nv. {Level})",
                BuffType.Line4 => $"Linha de 4 (Nv. {Level})",
                BuffType.Line5 => $"Linha de 5 (Nv. {Level})",
                _ => "Buff",
            };

        public string GetDescription()
        {
            int rolledPercent = Mathf.RoundToInt(RolledBonus * 100f);
            int nextTotalPercent = Mathf.RoundToInt((CurrentTotalBonus + RolledBonus) * 100f);

            return $"Aumenta os pontos em <color=#00FF00>+{rolledPercent}%</color>.\n"
                + $"Bônus total após escolher: <color=#FFD700>+{nextTotalPercent}%</color>";
        }
    }
}
