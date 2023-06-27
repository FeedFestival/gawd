using Game.Shared.Enums;
using System.Collections.Generic;

namespace Game.Shared.Interfaces
{
    public interface IHexCoord
    {
        int Y { get; set; }
        int X { get; set; }
        Dictionary<Dir, ICoord> Neighbors { get; set; }
    }
}
