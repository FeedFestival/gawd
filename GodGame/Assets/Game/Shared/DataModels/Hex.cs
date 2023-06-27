using Game.Shared.Enums;
using Game.Shared.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Shared.DataModels
{
    public class Hex : IHex
    {
        public int ID { get; set; }
        public int Y { get; set; }
        public int X { get; set; }
        public int Elevation { get; set; }
        public Dictionary<Dir, ICoord> Neighbors { get; set; }
        public RenderSettings RenderSettings;

        public IHexComponent HexComponent { get; set; }
        //public Transform Transform { get { return (HexComponent as HexComponent).transform; } }

        public Hex() { }
        public Hex(int id, ICoord coord)
        {
            setProperties(id, coord.Y, coord.X);
        }
        //public Hex(int id, Coord coord)
        //{
        //    setProperties(id, coord.Y, coord.X);
        //}
        public Hex(int id, int y, int x)
        {
            setProperties(id, y, x);
        }

        

        public void AddNeighbor(Dir dir, ICoord newNeighborCoord, int id)
        {
            Neighbors.Add(dir, newNeighborCoord);

            HexComponent?.AddNeighbor(dir, newNeighborCoord, id);
        }

        private void setProperties(int id, int y, int x)
        {
            ID = id;
            Y = y;
            X = x;
            Neighbors = new Dictionary<Dir, ICoord>();
        }
    }
}
