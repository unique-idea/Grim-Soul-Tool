using Engine.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Engine.Services.Interfaces
{
    public interface IDetector
    {
        Item DetectItemByColors(List<Color> scannedColors);
        int DectectItemDurability();
        int DetectItemStackSize();
        int DectectHealthBarRemaining();
        bool DetectEmptySlot();
    }
}
