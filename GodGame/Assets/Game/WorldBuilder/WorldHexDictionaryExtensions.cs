using Game.Shared.Enums;
using Game.Shared.Interfaces;
using Game.Shared.Utils;
using System.Collections.Generic;

namespace Game.WorldBuilder
{
    public static class WorldHexDictionaryExtensions
    {
        public static void AddHex(this Dictionary<int, Dictionary<int, IHex>> dictionary, IHex value)
        {
            if (!dictionary.ContainsKey(value.Y))
            {
                dictionary.Add(value.Y, new Dictionary<int, IHex>());
            }
            dictionary[value.Y].Add(value.X, value);
        }

        public static void AddMultipleNeighbors(this Dictionary<Dir, ICoord> neighbors, ICoord curCoord, Dir dir, ref Dictionary<int, Dictionary<int, IHex>> hexes)
        {
            var isOddRow = HexUtils.IsOddRow(curCoord.Y);
            var neighborAdjacentCoords = isOddRow
                ? HexUtils.ODD__NEIGHBORS_COORDS_OF_DIR[dir]
                : HexUtils.EVEN_NEIGHBORS_COORDS_OF_DIR[dir];

            var neighborDirCoord = neighborAdjacentCoords[(int)DirAdjacent.Center];
            var dirCoord = neighborDirCoord.Coord.Plus(curCoord);
            bool exists = hexes.HexExist(dirCoord);
            if (exists)
            {
                neighbors.TryAdd(neighborDirCoord.PerspectiveDir, curCoord);
            }

            neighborDirCoord = neighborAdjacentCoords[(int)DirAdjacent.Left];
            var coord = neighborDirCoord.Coord.Plus(curCoord);
            exists = hexes.HexExist(coord);
            if (exists)
            {
                neighbors.TryAdd(neighborDirCoord.PerspectiveDir, coord);
                var oppositeDir = HexUtils.OPPOSITE_DIR[neighborDirCoord.PerspectiveDir];
                hexes[coord.Y][coord.X].Neighbors.TryAdd(oppositeDir, dirCoord);
            }

            neighborDirCoord = neighborAdjacentCoords[(int)DirAdjacent.Right];
            coord = neighborDirCoord.Coord.Plus(curCoord);
            exists = hexes.HexExist(coord);
            if (exists)
            {
                neighbors.TryAdd(neighborDirCoord.PerspectiveDir, coord);
                var oppositeDir = HexUtils.OPPOSITE_DIR[neighborDirCoord.PerspectiveDir];
                hexes[coord.Y][coord.X].Neighbors.TryAdd(oppositeDir, dirCoord);
            }
        }

        public static bool HexExist(this Dictionary<int, Dictionary<int, IHex>> dictionary, ICoord coord)
        {
            var exists = dictionary.ContainsKey(coord.Y);
            if (exists)
            {
                exists = dictionary[coord.Y].ContainsKey(coord.X);
            }
            return exists;
        }

        public static IHex GetAtCoord(this Dictionary<int, Dictionary<int, IHex>> dictionary, ICoord coord)
        {
            return dictionary[coord.Y][coord.X];
        }

        public static ICoord GetEdgeCoord(this Queue<ICoord> queue, ref Dictionary<int, Dictionary<int, IHex>> hexes)
        {
            var coord = queue.Dequeue();
            if (hexes[coord.Y][coord.X].Neighbors.Count < 6)
            {
                return coord;
            }
            return queue.GetEdgeCoord(ref hexes);
        }
    }
}