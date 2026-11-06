using Engine.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Engine.Services.Interfaces
{
    public interface IMiniMapScanner
    {
        List<DetectedMapObject> ScanObjects(Bitmap bitmap, List<Item>? targets = null);
        Task<List<DetectedMapObject>> ScanObjectsFromScreenAsync(int screenX, int screenY, int width, int height, List<Item>? targets = null);
        List<DetectedMapObject> ScanObjectsFromScreen(int screenX, int screenY, int width, int height, List<Item>? targets = null);
    }
}
