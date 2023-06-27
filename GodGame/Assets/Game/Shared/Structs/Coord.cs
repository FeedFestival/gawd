using Game.Shared.Interfaces;

namespace Game.Shared.Structs
{

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

        // these two methods (Plus, Minus) are buggy, keep close eye on them
        public ICoord Plus(ICoord hexCoord)
        {
            Y = Y + hexCoord.Y;
            X = X + hexCoord.X;
            return this;
        }

        public ICoord Minus(ICoord hexCoord)
        {
            Y = Y - hexCoord.Y;
            X = X - hexCoord.X;
            return this;
        }

        public static ICoord Difference(ICoord to, ICoord from)
        {
            return new Coord(to.Y - from.Y, to.X - from.X);
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
    }
}
