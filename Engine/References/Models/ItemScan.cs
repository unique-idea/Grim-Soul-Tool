using Engine.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.References.Models
{
    public class ItemScan
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public int Radius { get; set; }
        public double Confidence { get; set; } = 0.0;

        public Item item { get; set; } = new Item();
    }
}
