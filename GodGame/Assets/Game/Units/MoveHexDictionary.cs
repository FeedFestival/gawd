using Game.Shared.Enums;
using Game.Shared.Interfaces;
using Game.Shared.Structs;
using Game.Shared.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Game.UnitController
{
    public static class MoveHexDictionary
    {
        public static string ToString(this Dictionary<int, Dictionary<int, IMoveHex>> dictionary)
        {
            var s = string.Empty;

            foreach (var iKvp in dictionary)
            {
                s += "[" + iKvp.Key + "]: {\n";
                foreach (var jKvp in iKvp.Value)
                {
                    s += "\t[" + jKvp.Key + "]: { " + jKvp.Value.ToString() + "}\n";
                }
                s += "\n}";
            }

            return s;
        }

        public static string ToStringDebug(this Dictionary<Dir, Coord> dictionary)
        {
            var s = string.Empty;
            foreach (var iKvp in dictionary)
            {
                s += "[" + iKvp.Key.ToString() + "]: { " + iKvp.Value.ToString() + "}";
            }
            return s;
        }

        public static void AddMoveHex(this Dictionary<int, Dictionary<int, IMoveHex>> dictionary, IMoveHex value)
        {
            if (!dictionary.ContainsKey(value.Y))
            {
                dictionary.Add(value.Y, new Dictionary<int, IMoveHex>());
            }
            dictionary[value.Y].Add(value.X, value);
        }

        public static int Count(this Dictionary<int, Dictionary<int, IMoveHex>> dictionary)
        {
            int count = 0;
            for (int i = 0; i < dictionary.Count; i++)
            {
                count += dictionary.ElementAt(i).Value.Count;
            }
            return count;
        }

        public static IHexCoord GetAtCoord(this Dictionary<int, Dictionary<int, IMoveHex>> dictionary, ICoord coord)
        {
            return dictionary[coord.Y][coord.X];
        }

        public static bool MoveHexExist(this Dictionary<int, Dictionary<int, IMoveHex>> dictionary, ICoord coord)
        {
            var exists = dictionary.ContainsKey(coord.Y);
            if (exists)
            {
                exists = dictionary[coord.Y].ContainsKey(coord.X);
            }
            return exists;
        }

        public static ICoord GetEdgeCoord(this Queue<ICoord> queue, ref Dictionary<int, Dictionary<int, IMoveHex>> moveHexes)
        {
            var coord = queue.Dequeue();
            if (moveHexes[coord.Y][coord.X].Neighbors.Count < 6)
            {
                return coord;
            }
            return queue.GetEdgeCoord(ref moveHexes);
        }
        public static ICoord GetEdgeCoord(this Queue<ICoord> queue, ref Dictionary<int, Dictionary<int, IHexCoord>> moveHexes)
        {
            var coord = queue.Dequeue();
            if (moveHexes[coord.Y][coord.X].Neighbors.Count < 6)
            {
                return coord;
            }
            return queue.GetEdgeCoord(ref moveHexes);
        }

        public static void AddMultipleNeighbors(this Dictionary<Dir, ICoord> neighbors, ICoord curCoord, Dir dir, ref Dictionary<int, Dictionary<int, IMoveHex>> moveHexes)
        {
            var isOddRow = HexUtils.IsOddRow(curCoord.Y);
            var neighborAdjacentCoords = isOddRow
                ? HexUtils.ODD__NEIGHBORS_COORDS_OF_DIR[dir]
                : HexUtils.EVEN_NEIGHBORS_COORDS_OF_DIR[dir];

            var neighborDirCoord = neighborAdjacentCoords[(int)DirWay.Left];
            var dirCoord = neighborDirCoord.Coord.Plus(curCoord);
            bool exists = moveHexes.MoveHexExist(dirCoord);
            if (exists)
            {
                neighbors.TryAdd(neighborDirCoord.PerspectiveDir, curCoord);
            }

            neighborDirCoord = neighborAdjacentCoords[(int)DirWay.Origin];
            var coord = neighborDirCoord.Coord.Plus(curCoord);
            exists = moveHexes.MoveHexExist(coord);
            if (exists)
            {
                neighbors.TryAdd(neighborDirCoord.PerspectiveDir, coord);
                var oppositeDir = HexUtils.OPPOSITE_DIR[neighborDirCoord.PerspectiveDir];
                moveHexes[coord.Y][coord.X].Neighbors.TryAdd(oppositeDir, dirCoord);
            }

            neighborDirCoord = neighborAdjacentCoords[(int)DirWay.Right];
            coord = neighborDirCoord.Coord.Plus(curCoord);
            exists = moveHexes.MoveHexExist(coord);
            if (exists)
            {
                neighbors.TryAdd(neighborDirCoord.PerspectiveDir, coord);
                var oppositeDir = HexUtils.OPPOSITE_DIR[neighborDirCoord.PerspectiveDir];
                moveHexes[coord.Y][coord.X].Neighbors.TryAdd(oppositeDir, dirCoord);
            }
        }
    }
}
