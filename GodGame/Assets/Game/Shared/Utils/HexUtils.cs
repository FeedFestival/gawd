using Game.Shared.DataModels;
using Game.Shared.Enums;
using Game.Shared.Interfaces;
using Game.Shared.Structs;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Game.Shared.Structs.HexPartsStructs;

namespace Game.Shared.Utils
{
    public class HexUtils : MonoBehaviour
    {
        public const float ROW_Y_______________OFFSET = 1.5f;
        public const float VERTICAL_ADJACENT___OFFSET = 0.8660254f;
        public const float HORIZONTAL_ADJACENT_OFFSET = 1.7320508f;

        public static readonly Dictionary<Dir, Vector3> DIR_NEW_POSITION = new Dictionary<Dir, Vector3>()
        {
            { Dir.NE, new Vector3(0, 0, HORIZONTAL_ADJACENT_OFFSET) },
            { Dir.E, new Vector3(ROW_Y_______________OFFSET, 0, VERTICAL_ADJACENT___OFFSET) },
            { Dir.SE, new Vector3(ROW_Y_______________OFFSET, 0, -VERTICAL_ADJACENT___OFFSET) },
            { Dir.SW, new Vector3(0, 0, -HORIZONTAL_ADJACENT_OFFSET) },
            { Dir.W, new Vector3(-ROW_Y_______________OFFSET, 0, -VERTICAL_ADJACENT___OFFSET) },
            { Dir.NW, new Vector3(-ROW_Y_______________OFFSET, 0, VERTICAL_ADJACENT___OFFSET) }
        };

        public static readonly Dictionary<Dir, ICoord> COORD_ODD__OFFSET = new Dictionary<Dir, ICoord>()
        {
            { Dir.NE, new Coord(1, 0) },
            { Dir.E, new Coord(0, 1) },
            { Dir.SE, new Coord(-1, 0) },
            { Dir.SW, new Coord(-1, -1) },
            { Dir.W, new Coord(0, -1) },
            { Dir.NW, new Coord(1, -1) }
        };
        public static readonly Dictionary<Dir, ICoord> COORD_EVEN_OFFSET = new Dictionary<Dir, ICoord>()
        {
            { Dir.NE, new Coord(1, 1) },
            { Dir.E, new Coord(0, 1) },
            { Dir.SE, new Coord(-1, 1) },
            { Dir.SW, new Coord(-1, 0) },
            { Dir.W, new Coord(0, -1) },
            { Dir.NW, new Coord(1, 0) }
        };
        public static readonly Dictionary<ICoord, Dir> ODD__OFFSET_TO_DIR = new Dictionary<ICoord, Dir>()
        {
            { COORD_ODD__OFFSET[Dir.NE], Dir.NE },
            { COORD_ODD__OFFSET[Dir.E], Dir.E },
            { COORD_ODD__OFFSET[Dir.SE], Dir.SE },
            { COORD_ODD__OFFSET[Dir.SW], Dir.SW },
            { COORD_ODD__OFFSET[Dir.W], Dir.W },
            { COORD_ODD__OFFSET[Dir.NW], Dir.NW }
        };
        public static readonly Dictionary<ICoord, Dir> EVEN_OFFSET_TO_DIR = new Dictionary<ICoord, Dir>()
        {
            { COORD_EVEN_OFFSET[Dir.NE], Dir.NE },
            { COORD_EVEN_OFFSET[Dir.E], Dir.E },
            { COORD_EVEN_OFFSET[Dir.SE], Dir.SE },
            { COORD_EVEN_OFFSET[Dir.SW], Dir.SW },
            { COORD_EVEN_OFFSET[Dir.W], Dir.W },
            { COORD_EVEN_OFFSET[Dir.NW], Dir.NW }
        };

        public static readonly Dir[] DIRECTIONS = new Dir[6] { Dir.NE, Dir.E, Dir.SE, Dir.SW, Dir.W, Dir.NW };
        public static readonly Dir[] DIRECTIONS_REVERSED = new Dir[6] { Dir.NW, Dir.W, Dir.SW, Dir.SE, Dir.E, Dir.NE };

        public static readonly Dictionary<Dir, Dir> OPPOSITE_DIR = new Dictionary<Dir, Dir>()
        {
            { Dir.NE, Dir.SW },
            { Dir.E, Dir.W },
            { Dir.SE, Dir.NW },
            { Dir.SW, Dir.NE },
            { Dir.W, Dir.E },
            { Dir.NW, Dir.SE }
        };

        public static readonly Dictionary<Dir, NeighborAdjacentCoord[]> ODD__NEIGHBORS_COORDS_OF_DIR = new Dictionary<Dir, NeighborAdjacentCoord[]>()
        {
            {
                Dir.NE,
                new NeighborAdjacentCoord[3] {
                    new NeighborAdjacentCoord(Dir.W, new Coord(1, -1)),
                    new NeighborAdjacentCoord(Dir.SW, new Coord(1, 0)),
                    new NeighborAdjacentCoord(Dir.SE, new Coord(0, 1))
                }
            },
            {
                Dir.E,
                new NeighborAdjacentCoord[3] {
                    new NeighborAdjacentCoord(Dir.NW, new Coord(1, 0)),
                    new NeighborAdjacentCoord(Dir.W, new Coord(0, 1)),
                    new NeighborAdjacentCoord(Dir.SW, new Coord(-1, 0))
                }
            },
            {
                Dir.SE,
                new NeighborAdjacentCoord[3] {
                    new NeighborAdjacentCoord(Dir.NE, new Coord(0, 1)),
                    new NeighborAdjacentCoord(Dir.NW, new Coord(-1, 0)),
                    new NeighborAdjacentCoord(Dir.W, new Coord(-1, -1))
                }
            },
            {
                Dir.SW,
                new NeighborAdjacentCoord[3] {
                    new NeighborAdjacentCoord(Dir.E, new Coord(-1, 0)),
                    new NeighborAdjacentCoord(Dir.NE, new Coord(-1, -1)),
                    new NeighborAdjacentCoord(Dir.NW, new Coord(0, -1))
                }
            },
            {
                Dir.W,
                new NeighborAdjacentCoord[3] {
                    new NeighborAdjacentCoord(Dir.SE, new Coord(-1, -1)),
                    new NeighborAdjacentCoord(Dir.E, new Coord(0, -1)),
                    new NeighborAdjacentCoord(Dir.NE, new Coord(1, -1))
                }
            },
            {
                Dir.NW,
                new NeighborAdjacentCoord[3] {
                    new NeighborAdjacentCoord(Dir.SW, new Coord(0, -1)),
                    new NeighborAdjacentCoord(Dir.SE, new Coord(1, -1)),
                    new NeighborAdjacentCoord(Dir.E, new Coord(1, 0))
                }
            },
        };
        public static readonly Dictionary<Dir, NeighborAdjacentCoord[]> EVEN_NEIGHBORS_COORDS_OF_DIR = new Dictionary<Dir, NeighborAdjacentCoord[]>()
        {
            {
                Dir.NE,
                new NeighborAdjacentCoord[3] {
                    new NeighborAdjacentCoord(Dir.W, new Coord(1, 0)),
                    new NeighborAdjacentCoord(Dir.SW, new Coord(1, 1)),
                    new NeighborAdjacentCoord(Dir.SE, new Coord(0, 1))
                }
            },
            {
                Dir.E,
                new NeighborAdjacentCoord[3] {
                    new NeighborAdjacentCoord(Dir.NW, new Coord(1, 1)),
                    new NeighborAdjacentCoord(Dir.W, new Coord(0, 1)),
                    new NeighborAdjacentCoord(Dir.SW, new Coord(-1, 1))
                }
            },
            {
                Dir.SE,
                new NeighborAdjacentCoord[3] {
                    new NeighborAdjacentCoord(Dir.NE, new Coord(0, 1)),
                    new NeighborAdjacentCoord(Dir.NW, new Coord(-1, 1)),
                    new NeighborAdjacentCoord(Dir.W, new Coord(-1, 0))
                }
            },
            {
                Dir.SW,
                new NeighborAdjacentCoord[3] {
                    new NeighborAdjacentCoord(Dir.E, new Coord(-1, 1)),
                    new NeighborAdjacentCoord(Dir.NE, new Coord(-1, 0)),
                    new NeighborAdjacentCoord(Dir.NW, new Coord(0, -1))
                }
            },
            {
                Dir.W,
                new NeighborAdjacentCoord[3] {
                    new NeighborAdjacentCoord(Dir.SE, new Coord(-1, 0)),
                    new NeighborAdjacentCoord(Dir.E, new Coord(0, -1)),
                    new NeighborAdjacentCoord(Dir.NE, new Coord(1, 0))
                }
            },
            {
                Dir.NW,
                new NeighborAdjacentCoord[3] {
                    new NeighborAdjacentCoord(Dir.SW, new Coord(0, -1)),
                    new NeighborAdjacentCoord(Dir.SE, new Coord(1, 0)),
                    new NeighborAdjacentCoord(Dir.E, new Coord(1, 1))
                }
            },
        };

        public static readonly EdgeDir[] EDGE_DIRECTIONS = new EdgeDir[6] { EdgeDir.NE, EdgeDir.SE, EdgeDir.S, EdgeDir.SW, EdgeDir.NW, EdgeDir.N };
        public static readonly Dictionary<EdgeDir, EdgeDir> LEFT_OPPOSITE_EDGE_DIR = new Dictionary<EdgeDir, EdgeDir>()
        {
            { EdgeDir.NE, EdgeDir.S },
            { EdgeDir.SE, EdgeDir.SW },
            { EdgeDir.S, EdgeDir.NW },
            { EdgeDir.SW, EdgeDir.N },
            { EdgeDir.NW, EdgeDir.NE },
            { EdgeDir.N, EdgeDir.SE },
        };

        public static readonly Dictionary<EdgeDir, Dictionary<Adjacent, Dir>> ADJACENT_DIR_OF_EDGE_DIR = new Dictionary<EdgeDir, Dictionary<Adjacent, Dir>>()
        {
            {
                EdgeDir.N, new Dictionary<Adjacent, Dir>()
                {
                    { Adjacent.Left, Dir.NW },
                    { Adjacent.Right, Dir.NE }
                }
            },
            {
                EdgeDir.NE, new Dictionary<Adjacent, Dir>()
                {
                    { Adjacent.Left, Dir.NE },
                    { Adjacent.Right, Dir.E }
                }
            },
            {
                EdgeDir.SE, new Dictionary<Adjacent, Dir>()
                {
                    { Adjacent.Left, Dir.E },
                    { Adjacent.Right, Dir.SE }
                }
            },
            {
                EdgeDir.S, new Dictionary<Adjacent, Dir>()
                {
                    { Adjacent.Left, Dir.SE },
                    { Adjacent.Right, Dir.SW }
                }
            },
            {
                EdgeDir.SW, new Dictionary<Adjacent, Dir>()
                {
                    { Adjacent.Left, Dir.SW },
                    { Adjacent.Right, Dir.W }
                }
            },
            {
                EdgeDir.NW, new Dictionary<Adjacent, Dir>()
                {
                    { Adjacent.Left, Dir.W },
                    { Adjacent.Right, Dir.NW }
                }
            },
        };

        public static readonly Dictionary<Dir, EdgeDir[]> ADJACENT_EDGE_DIR_OF_DIR = new Dictionary<Dir, EdgeDir[]>()
        {
            {
                Dir.NE, new EdgeDir[2] { EdgeDir.N, EdgeDir.NE }
            },
            {
                Dir.E, new EdgeDir[2] { EdgeDir.NE, EdgeDir.SE }
            },
            {
                Dir.SE, new EdgeDir[2] { EdgeDir.SE, EdgeDir.S }
            },
            {
                Dir.SW, new EdgeDir[2] { EdgeDir.S, EdgeDir.SW }
            },
            {
                Dir.W, new EdgeDir[2] { EdgeDir.SW, EdgeDir.NW }
            },
            {
                Dir.NW, new EdgeDir[2] { EdgeDir.NW, EdgeDir.N }
            },
        };

        public static readonly Dictionary<EdgeDir, Vector3> EDGE_STITCH_POSITION = new Dictionary<EdgeDir, Vector3>()
        {
            { EdgeDir.NE, new Vector3(VERTICAL_ADJACENT___OFFSET, 0, 0.5f) },
            { EdgeDir.SE, new Vector3(VERTICAL_ADJACENT___OFFSET, 0, -0.5f) },
            { EdgeDir.S, new Vector3(0, 0, -1) },
            { EdgeDir.SW, new Vector3(-VERTICAL_ADJACENT___OFFSET, 0, -0.5f) },
            { EdgeDir.NW, new Vector3(-VERTICAL_ADJACENT___OFFSET, 0, 0.5f) },
            { EdgeDir.N, new Vector3(0, 0, 1) },
        };

        public static readonly Dictionary<int, Vector3> RAY_FOG_OFFSETS = new Dictionary<int, Vector3>()
        {
            { 0, new Vector3(0, 0, 0.666667f) },
            { 1, new Vector3(0.288675f, 0, 0.166667f) },
            { 2, new Vector3(0, 0, -0.666667f) },
            { 3, new Vector3(-0.288675f, 0, -0.166667f) },
        };

        public static SlopeDir Opposite(SlopeDir slopeDir)
        {
            if (slopeDir == SlopeDir.LEVEL) { return slopeDir; }
            return slopeDir == SlopeDir.ASC ? SlopeDir.DESC : SlopeDir.ASC;
        }

        public static RowIs Opposite(RowIs rowIs)
        {
            return rowIs == RowIs.Even ? RowIs.Odd : RowIs.Even;
        }

        public struct NeighborAdjacentCoord
        {
            public Dir PerspectiveDir;
            public Coord Coord;

            public NeighborAdjacentCoord(Dir dir, Coord coord)
            {
                PerspectiveDir = dir;
                Coord = coord;
            }
        }

        public static ICoord GetCoordOffset(bool isOddRow, Dir dir)
        {
            return isOddRow ? COORD_ODD__OFFSET[dir] : COORD_EVEN_OFFSET[dir];
        }

        public static Vector3 GetHexPosition(IHex hex)
        {
            return GetHexPosition(hex.Y, hex.X, hex.Elevation);
        }

        public static Vector3 GetHexPosition(ICoord coord, int elevation = 0)
        {
            return GetHexPosition(coord.Y, coord.X, elevation);
        }

        public static Vector3 GetHexPosition(int y, int x, int elevation = 0)
        {
            float xPos;
            if (IsOddRow(y))
            {
                var isFirst = x == 0;
                if (isFirst)
                {
                    xPos = -VERTICAL_ADJACENT___OFFSET;
                }
                else
                {
                    xPos = (x * HORIZONTAL_ADJACENT_OFFSET) - VERTICAL_ADJACENT___OFFSET;
                }
            }
            else
            {
                xPos = x * HORIZONTAL_ADJACENT_OFFSET;
            }
            return new Vector3(xPos, 0.2f * elevation, y * ROW_Y_______________OFFSET);
        }

        public static bool IsOddRow(int y)
        {
            return !(y % 2 == 0);
        }

        public static int GetRangeByHexCount(int count)
        {
            var sum = (count - 1) / 6;
            var i = 0;
            while (sum > 0)
            {
                i++;
                sum = sum - i;
            }
            return i;
        }

        public static int GetHexCountByRange(int maxMoves)
        {
            int sum = 0;
            for (int i = 1; i <= maxMoves; i++)
            {
                sum += i;
            }
            return 1 + (6 * sum);
        }

        public static Vector3 GetBridgeRotationByCoord(Dir dir)
        {
            return new Vector3(0, ((int)dir + 1) * 60f, 0);
        }

        public static Vector3 GetBridgeEdgeRotationByCoord(EdgeDir edgeDir)
        {
            return new Vector3(0, ((int)edgeDir) * 60f, 0);
        }

        public static Vector3 GetEdgeStitchRotationByCoord(EdgeDir dir, int rotationCount)
        {
            return new Vector3(0, rotationCount * -120, 0);
        }

        public static SlopeDir GetSlopeDir(int elevation, int neighborElevation)
        {
            if (elevation > neighborElevation)
            {
                return SlopeDir.DESC;
            }
            else if (elevation < neighborElevation)
            {
                return SlopeDir.ASC;
            }
            else
            {
                return SlopeDir.LEVEL;
            }
        }
    }

    public static class DictionaryExtensions
    {
        public static void AddAt<TKey1, TKey2, TValue>(this Dictionary<TKey1, Dictionary<TKey2, TValue>> dictionary, TKey1 key1, TKey2 key2, TValue value)
        {
            if (!dictionary.ContainsKey(key1))
            {
                dictionary.Add(key1, new Dictionary<TKey2, TValue>());
            }
            dictionary[key1].Add(key2, value);
        }

        //public static int Count(this Dictionary<int, Dictionary<int, IHexCoord>> dictionary)
        //{
        //    int count = 0;
        //    for (int i = 0; i < dictionary.Count; i++)
        //    {
        //        count += dictionary.ElementAt(i).Value.Count;
        //    }
        //    return count;
        //}

        public static Dir GetMissingNeighbor(this Dictionary<Dir, ICoord> dictionary)
        {
            if (dictionary == null || dictionary.Count == 0) { return Dir.NE; }

            foreach (Dir dir in HexUtils.DIRECTIONS)
            {
                if (dictionary.ContainsKey(dir))
                {
                    continue;
                }
                return dir;
            }
            return Dir.NW;
        }
    }
}
