using System;

namespace Engine.Models
{
    /// <summary>
    /// Represents the player's status read from the Healthbar region (R1),
    /// combining OCR text scanning and visual red health bar tracking.
    /// </summary>
    public class PlayerHealthInfo
    {
        /// <summary>Character name (e.g. "LEGIT DEEP").</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Current health points (e.g. 90).</summary>
        public int CurrentHealth { get; set; }

        /// <summary>Maximum health points (e.g. 102).</summary>
        public int MaxHealth { get; set; }

        /// <summary>Player level if visible (e.g. 41).</summary>
        public int Level { get; set; }

        /// <summary>Health percentage calculated from OCR text (CurrentHealth / MaxHealth * 100.0), or -1 if unavailable.</summary>
        public double TextPercentage { get; set; } = -1.0;

        /// <summary>Raw detected current health from OCR before/without BG removal.</summary>
        public int OcrCurrentHp { get; set; } = -1;

        /// <summary>Detection method indicator: "Direct", "Retry1", "Retry2", "Retry3", "Guessed", or "None".</summary>
        public string DetectionMethod { get; set; } = "None";

        // Comparison fields kept for backward compatibility
        public int CurrentHpBeforeBg { get; set; }
        public int MaxHpBeforeBg { get; set; }
        public double TextPercentageBeforeBg { get; set; } = -1.0;
        public string RawTextBeforeBg { get; set; } = string.Empty;

        public int CurrentHpAfterBg { get; set; }
        public int MaxHpAfterBg { get; set; }
        public double TextPercentageAfterBg { get; set; } = -1.0;
        public string RawTextAfterBg { get; set; } = string.Empty;

        /// <summary>Health percentage calculated from the visual health bar line relative to R1 width or full bar track.</summary>
        public double VisualBarPercentage { get; set; }

        /// <summary>Whether the text OCR reading passed validation (0% - 100% and consistent with visual bar).</summary>
        public bool IsTextValid { get; set; }

        /// <summary>Final health percentage decided by the dual tracking engine.</summary>
        public double FinalHealthPercentage { get; set; }

        /// <summary>Health percentage (0.0% - 100.0%), pointing to FinalHealthPercentage.</summary>
        public double HealthPercentage => FinalHealthPercentage;

        /// <summary>True if health is at or below 40%.</summary>
        public bool IsLowHealth => FinalHealthPercentage > 0 && FinalHealthPercentage <= 40.0;

        /// <summary>Indicates whether an auto-heal action (key H) was triggered on this frame.</summary>
        public bool HealTriggered { get; set; }

        /// <summary>Timestamp when this info was scanned.</summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>Raw OCR text detected in the region.</summary>
        public string RawOcrText { get; set; } = string.Empty;

        public override string ToString()
        {
            string methodSuffix = (!string.IsNullOrEmpty(DetectionMethod) && DetectionMethod != "Direct") 
                ? " [" + DetectionMethod + "]" 
                : "";
            string txtStatus = IsTextValid 
                ? $"{CurrentHealth}/{MaxHealth} ({TextPercentage:F0}%){methodSuffix}" 
                : "None";
            string healTag = HealTriggered ? " [HEAL H TRIGGERED]" : "";
            return $"[PlayerHealth] Text: {txtStatus} | BarWidth: {VisualBarPercentage:F0}% -> Final: {FinalHealthPercentage:F0}%, Low: {IsLowHealth}{healTag}";
        }
    }
}
