using Engine.References.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Services.Interfaces
{
    public interface IItemCluster
    {
        /// <summary>
        /// Gom nhóm các ô liền kề chứa cùng một loại Item thành 1 ClusterResult duy nhất.
        /// </summary>
        /// <param name="scannedGrids">Danh sách các ô đã quét và nhận diện được Item</param>
        /// <param name="playerRow">Hàng nhân vật đang đứng (thường là N/2)</param>
        /// <param name="playerCol">Cột nhân vật đang đứng (thường là N/2)</param>
        List<ClusterResult> GroupAdjacentItems(List<ItemScan> scannedGrids, int playerRow, int playerCol);
    }
}
