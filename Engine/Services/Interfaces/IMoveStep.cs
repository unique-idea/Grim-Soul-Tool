using Engine.Enums;
using Engine.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Services.Interfaces
{
    public interface IMoveStep
    {
        void MoveFromCenterZone(Coordinate targetCoordinate, int gridN, int centerIgnoredRadius);
    }
}
