using Game.Shared.DataModels;
using Game.Shared.Enums;
using Game.Shared.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Game.UnitController
{
    public static class MoveHexDictionary
    {
        public static string ToString(this Dictionary<int, Dictionary<int, MoveHex>> dictionary)
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

        public static string ToStringDebug(this Dictionary<Dir, Hex.Coord> dictionary)
        {
            var s = string.Empty;
            foreach (var iKvp in dictionary)
            {
                s += "[" + iKvp.Key.ToString() + "]: { " + iKvp.Value.ToString() + "}";
            }
            return s;
        }

        public static void AddMoveHex(this Dictionary<int, Dictionary<int, MoveHex>> dictionary, MoveHex value)
        {
            if (!dictionary.ContainsKey(value.Y))
            {
                dictionary.Add(value.Y, new Dictionary<int, MoveHex>());
            }
            dictionary[value.Y].Add(value.X, value);
        }

        public static int Count(this Dictionary<int, Dictionary<int, MoveHex>> dictionary)
        {
            int count = 0;
            for (int i = 0; i < dictionary.Count; i++)
            {
                count += dictionary.ElementAt(i).Value.Count;
            }
            return count;
        }

        public static MoveHex GetAtCoord(this Dictionary<int, Dictionary<int, MoveHex>> dictionary, Hex.Coord coord)
        {
            return dictionary[coord.Y][coord.X];
        }

        public static bool MoveHexExist(this Dictionary<int, Dictionary<int, MoveHex>> dictionary, Hex.Coord coord)
        {
            var exists = dictionary.ContainsKey(coord.Y);
            if (exists)
            {
                exists = dictionary[coord.Y].ContainsKey(coord.X);
            }
            return exists;
        }

        public static Hex.Coord GetEdgeCoord(this Queue<Hex.Coord> queue, ref Dictionary<int, Dictionary<int, MoveHex>> moveHexes)
        {
            var coord = queue.Dequeue();
            if (moveHexes[coord.Y][coord.X].Neighbors.Count < 6)
            {
                return coord;
            }
            return queue.GetEdgeCoord(ref moveHexes);
        }

        public static void AddMultipleNeighbors(this Dictionary<Dir, ICoord> neighbors, ICoord curCoord, Dir dir, ref Dictionary<int, Dictionary<int, MoveHex>> moveHexes)
        {
            var isOddRow = HexUtils.IsOddRow(curCoord.Y);
            var neighborAdjacentCoords = isOddRow
                ? HexUtils.ODD__NEIGHBORS_COORDS_OF_DIR[dir]
                : HexUtils.EVEN_NEIGHBORS_COORDS_OF_DIR[dir];

            var neighborDirCoord = neighborAdjacentCoords[(int)DirAdjacent.Center];
            var dirCoord = neighborDirCoord.Coord.Plus(curCoord);
            bool exists = moveHexes.MoveHexExist(dirCoord);
            if (exists)
            {
                neighbors.TryAdd(neighborDirCoord.PerspectiveDir, curCoord);
            }

            neighborDirCoord = neighborAdjacentCoords[(int)DirAdjacent.Left];
            var coord = neighborDirCoord.Coord.Plus(curCoord);
            exists = moveHexes.MoveHexExist(coord);
            if (exists)
            {
                neighbors.TryAdd(neighborDirCoord.PerspectiveDir, coord);
                var oppositeDir = HexUtils.OPPOSITE_DIR[neighborDirCoord.PerspectiveDir];
                moveHexes[coord.Y][coord.X].Neighbors.TryAdd(oppositeDir, dirCoord);
            }

            neighborDirCoord = neighborAdjacentCoords[(int)DirAdjacent.Right];
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
