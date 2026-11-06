using Engine.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.References.Models
{
    public class ClusterResult
    {
        public Item Item { get; set; }
        public int CenterRow { get; set; }
        public int CenterCol { get; set; }
        public int DistanceToPlayer { get; set; }
    }
}
