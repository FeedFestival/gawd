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
        public Hex(int id, Coord coord)
        {
            setProperties(id, coord.Y, coord.X);
        }
        public Hex(int id, int y, int x)
        {
            setProperties(id, y, x);
        }

        [System.Serializable]
        public struct Coord : ICoord
        {
            public int Y { get; set; }
            public int X { get; set; }

            public Coord(int y, int x)
            {
                Y = y;
                X = x;
            }

            public Coord(Coord coord)
            {
                Y = coord.Y;
                X = coord.X;
            }

            public override string ToString()
            {
                return Y + "_" + X;
            }

            public Coord Plus(ICoord hexCoord)
            {
                Y = Y + hexCoord.Y;
                X = X + hexCoord.X;
                return this;
            }

            public Coord Plus(Coord hexCoord)
            {
                Y = Y + hexCoord.Y;
                X = X + hexCoord.X;
                return this;
            }

            internal Coord Minus(Coord hexCoord)
            {
                Y = Y - hexCoord.Y;
                X = X - hexCoord.X;
                return this;
            }

            internal void Reset()
            {
                Y = 0;
                X = 0;
            }

            public static Coord AddTogheter(ICoord a, ICoord b)
            {
                return new Coord(a.Y + b.Y, a.X + b.X);
            }

            public static Coord AddTogheter(Coord a, Coord b)
            {
                return new Coord(a.Y + b.Y, a.X + b.X);
            }
            public static Coord Minus(Coord to, Coord from)
            {
                return new Coord(to.Y - from.Y, to.X - from.X);
            }
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
