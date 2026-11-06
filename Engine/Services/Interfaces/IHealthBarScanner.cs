using Engine.Models;
using System.Drawing;
using System.Threading.Tasks;

namespace Engine.Services.Interfaces
{
    public interface IHealthBarScanner
    {
        /// <summary>
        /// Captures the specified screen bounds of the Healthbar region and parses player status.
        /// </summary>
        Task<PlayerHealthInfo> ScanFromScreenAsync(int screenX, int screenY, int width, int height, int maxHealth = 120, int barHeightPercent = 25);

        /// <summary>
        /// Captures two separate regions attached to each other:
        /// 1) Bar Region: detects visual health bar fill width.
        /// 2) Under Region (Text): reads exact current health number via OCR.
        /// Combines both detections into PlayerHealthInfo.
        /// </summary>
        Task<PlayerHealthInfo> ScanSeparateRegionsFromScreenAsync(
            int barX, int barY, int barW, int barH,
            int textX, int textY, int textW, int textH,
            int maxHealth = 120,
            int healThresholdPercent = 40);

        /// <summary>
        /// Parses player status from an existing Bitmap.
        /// </summary>
        Task<PlayerHealthInfo> ParseHealthBitmapAsync(Bitmap bitmap, int maxHealth = 120, int barHeightPercent = 25);
    }
}
