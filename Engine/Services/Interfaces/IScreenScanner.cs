using Engine.References.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Services.Interfaces
{
    public interface IScreenScanner
    {
        List<ItemScan> ScanGridSpiral(int screenX, int screenY, int width, int height, int gridN, int minRadius);
    }
}
