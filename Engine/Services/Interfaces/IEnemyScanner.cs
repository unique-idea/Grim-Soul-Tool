using Engine.Models;
using System;
using System.Threading.Tasks;

namespace Engine.Services.Interfaces
{
    public interface IEnemyScanner : IDisposable
    {
        /// <summary>
        /// Scans the Enemy Bar region for red line HP presence and the attached Energy region for OCR text.
        /// </summary>
        Task<EnemyEnergyInfo> ScanEnemyAndEnergyFromScreenAsync(
            int enemyX, int enemyY, int enemyW, int enemyH,
            int energyX, int energyY, int energyW, int energyH,
            int maxEnergy = 100);

        /// <summary>
        /// Fast screen check to see if an enemy red bar is currently present.
        /// </summary>
        bool CheckEnemyRedBarFromScreen(int enemyX, int enemyY, int enemyW, int enemyH);
    }
}
