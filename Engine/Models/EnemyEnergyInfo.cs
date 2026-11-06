namespace Engine.Models
{
    /// <summary>
    /// Information scanned from the Enemy Bar (R5) and attached Energy sub-region.
    /// </summary>
    public class EnemyEnergyInfo
    {
        /// <summary>True if a red line representing enemy HP is detected in the enemy region.</summary>
        public bool HasEnemy { get; set; }

        /// <summary>Estimated enemy health percentage based on red bar width.</summary>
        public double EnemyHpPercentage { get; set; }

        /// <summary>Current player energy value read from OCR (-1 if unavailable).</summary>
        public int CurrentEnergy { get; set; } = -1;

        /// <summary>Configured max energy value (default 100).</summary>
        public int MaxEnergy { get; set; } = 100;

        /// <summary>Current energy percentage (CurrentEnergy / MaxEnergy * 100.0).</summary>
        public double EnergyPercentage { get; set; } = -1;

        /// <summary>Raw OCR text string recognized from energy region.</summary>
        public string RawEnergyText { get; set; } = string.Empty;
    }
}
