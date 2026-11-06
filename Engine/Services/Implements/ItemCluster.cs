using Engine.References.Models;
using Engine.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Services.Implements
{
    public class ItemCluster : IItemCluster
    {
        public List<ClusterResult> GroupAdjacentItems(List<ItemScan> scannedGrids, int playerRow, int playerCol)
        {
            var clusters = new List<ClusterResult>();

            // Chỉ lọc những ô thực sự nhận diện được Item
            var validGrids = scannedGrids.Where(g => g.item != null).ToList();
            var visited = new HashSet<ItemScan>();

            foreach (var grid in validGrids)
            {
                if (visited.Contains(grid))
                    continue;

                // Tìm tất cả các ô liền kề chứa CÙNG loại Item bằng thuật toán loang (BFS/DFS)
                var currentGroup = new List<ItemScan>();
                FindConnectedGrids(grid, validGrids, visited, currentGroup);

                // Tính tọa độ tâm (Average Row & Col) của cụm ô này
                int avgRow = (int)Math.Round(currentGroup.Average(g => g.Row));
                int avgCol = (int)Math.Round(currentGroup.Average(g => g.Col));

                // Tính khoảng cách từ Tâm cụm này đến Nhân vật (Manhattan Distance)
                int distance = Math.Abs(avgRow - playerRow) + Math.Abs(avgCol - playerCol);

                clusters.Add(new ClusterResult
                {
                    Item = grid.item!,
                    CenterRow = avgRow,
                    CenterCol = avgCol,
                    DistanceToPlayer = distance
                });
            }

            // Ưu tiên sắp xếp các cụm Item từ GẦN nhân vật nhất đến XA nhất
            return clusters.OrderBy(c => c.DistanceToPlayer).ToList();
        }

        private void FindConnectedGrids(
            ItemScan current,
            List<ItemScan> allGrids,
            HashSet<ItemScan> visited,
            List<ItemScan> currentGroup)
        {
            visited.Add(current);
            currentGroup.Add(current);

            // Tìm các ô xung quanh trong bán kính 1 ô (tính cả 8 hướng chéo)
            var neighbors = allGrids.Where(g =>
                !visited.Contains(g) &&
                g.item?.Name == current.item?.Name &&
                Math.Abs(g.Row - current.Row) <= 1 &&
                Math.Abs(g.Col - current.Col) <= 1
            ).ToList();

            foreach (var neighbor in neighbors)
            {
                FindConnectedGrids(neighbor, allGrids, visited, currentGroup);
            }
        }
    }
}
