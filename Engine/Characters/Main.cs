using Engine.Models;
using System;

namespace Engine.Characters
{
    public class Main : BaseCharacter
    {
        public PlayerHealthInfo? HealthInfo { get; set; }

        public string Name => HealthInfo?.Name ?? string.Empty;
        public int CurrentHealth => HealthInfo?.CurrentHealth ?? 0;
        public int MaxHealth => HealthInfo?.MaxHealth ?? 0;
        public double HealthPercentage => HealthInfo?.HealthPercentage ?? 0.0;
        public int Level => HealthInfo?.Level ?? 0;
        public bool IsLowHealth => HealthInfo?.IsLowHealth ?? false;
    }
}
