using Game.Shared.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Shared.Interfaces
{
    public interface IHex : IHexCoord
    {
        //public int Y { get; set; }
        //public int X { get; set; }
        //public Dictionary<Dir, ICoord> Neighbors { get; set; }
        public int ID { get; set; }
        public int Elevation { get; set; }
        public IHexComponent HexComponent { get; set; }

        void AddNeighbor(Dir dir, ICoord newNeighborCoord, int id);
    }
}
