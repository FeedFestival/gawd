using System.Linq;
//using Tarodev_Pathfinding._Scripts.Grid;
using UnityEngine;

//namespace _Scripts.Tiles {
//    public class HexNode : NodeBase {
//        public override void CacheNeighbors() {
//            Neighbors = GridManager.Instance.Tiles.Where(t => Coords.GetDistance(t.Value.Coords) == 1).Select(t=>t.Value).ToList();
//        }
//    }
//}

namespace Game.Shared.DataModels
{

    public struct HexCoords : ICoords
    {
        private readonly int _x;
        private readonly int _y;

        public HexCoords(int x, int y)
        {
            _x = x;
            _y = y;
            Pos = _x * new Vector2(SQRT3, 0) + _y * new Vector2(SQRT3 / 2, 1.5f);
        }

        public float GetDistance(ICoords other) => (this - (HexCoords)other).AxialLength();

        public static readonly float SQRT3 = Mathf.Sqrt(3);

        public Vector2 Pos { get; set; }

        private int AxialLength()
        {
            if (_x == 0 && _y == 0) return 0;
            if (_x > 0 && _y >= 0) return _x + _y;
            if (_x <= 0 && _y > 0) return -_x < _y ? _y : -_x;
            if (_x < 0) return -_x - _y;
            return -_y > _x ? -_y : _x;
        }

        public static HexCoords operator -(HexCoords a, HexCoords b)
        {
            return new HexCoords(a._x - b._x, a._y - b._y);
        }

        public override string ToString()
        {
            return "_q: " + _x + ", _r: " + _y + ", Pos: " + Pos;
        }
    }
}
