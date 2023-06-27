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