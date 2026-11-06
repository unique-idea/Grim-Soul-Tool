using Engine.Models;
using System.Collections.Generic;
using System.Drawing;

namespace Engine.Services.Interfaces
{
    public interface IChestScanner
    {
        /// <summary>
        /// Checks whether the chest/loot window is open in Region 6 by detecting the dark slot background colors or item sprites.
        /// </summary>
        bool IsChestOpen(int chestX, int chestY, int chestW, int chestH);

        /// <summary>
        /// Scans all 5x5 slots in Region 6, identifying empty vs occupied slots and recognizing items.
        /// </summary>
        List<ChestSlotResult> ScanChestSlots(int chestX, int chestY, int chestW, int chestH, IEnumerable<Item>? candidateItems = null);

        /// <summary>
        /// Scans a provided bitmap containing a 5x5 chest/loot grid.
        /// </summary>
        List<ChestSlotResult> ScanChestSlotsFromBitmap(Bitmap bitmap, int rows = 5, int cols = 5, IEnumerable<Item>? candidateItems = null);
    }
}
