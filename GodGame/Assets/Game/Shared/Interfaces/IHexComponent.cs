using Game.Shared.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Shared.Interfaces
{
    public interface IHexComponent
    {
        Transform Transform { get; }
        void Init(IHex hex);
        void AddNeighbor(Dir dir, ICoord newNeighborCoord, int id);
    }
}
