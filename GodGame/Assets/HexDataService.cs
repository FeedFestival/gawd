using System;
using System.Collections.Generic;
using UnityEngine;

public class HexDataService
{
    public HexDataService() { }

    // Start is called before the first frame update
    internal List<Hex> GetHexes()
    {
        return mapDTOtoHex(_dBHexes);
    }

    private List<Hex> mapDTOtoHex(List<HexDTO> hexes)
    {
        var hexList = new List<Hex>();
        hexes.ForEach(hDto =>
        {
            // Debug.Log("hDto: " + hDto.NeighborsValue);
            var hex = new Hex(hDto);

            hexList.Add(hex);
        });
        return hexList;
    }

    private List<HexDTO> _dBHexes = new List<HexDTO>
    {
        new HexDTO() {
            ID = 1, Y = 1, X = 1, Elevation = 0,
            // "NE.0;E.0;SE.0;SW.0;W.0;NW.0;"
            NeighborsValue = Hex.ToNeighborsValue(new Dictionary<CoordDir, int>() {
                { CoordDir.NE, 2 }, { CoordDir.E, 3 }, { CoordDir.SE, 4 },
                { CoordDir.SW, 5 }, { CoordDir.W, 6 }, { CoordDir.NW, 7 }
            })
        },
        new HexDTO() {
            ID = 2, Y = 2, X = 1, Elevation = 0,
            NeighborsValue = Hex.ToNeighborsValue(new Dictionary<CoordDir, int>() {
                { CoordDir.NE, 0 }, { CoordDir.E, 0 }, { CoordDir.SE, 3 },
                { CoordDir.SW, 1 }, { CoordDir.W, 7 }, { CoordDir.NW, 0 }
            })
        },
        new HexDTO() {
            ID = 3, Y = 1, X = 2, Elevation = 0,
            NeighborsValue = Hex.ToNeighborsValue(new Dictionary<CoordDir, int>() {
                { CoordDir.NE, 0 }, { CoordDir.E, 0 }, { CoordDir.SE, 0 },
                { CoordDir.SW, 4 }, { CoordDir.W, 1 }, { CoordDir.NW, 2 }
            })
        },
        new HexDTO() {
            ID = 4, Y = 0, X = 1, Elevation = 0,
            NeighborsValue = Hex.ToNeighborsValue(new Dictionary<CoordDir, int>() {
                { CoordDir.NE, 3 }, { CoordDir.E, 0 }, { CoordDir.SE, 0 },
                { CoordDir.SW, 0 }, { CoordDir.W, 5 }, { CoordDir.NW, 1 }
            })
        },
        new HexDTO() {
            ID = 5, Y = 0, X = 0, Elevation = 0,
            NeighborsValue = Hex.ToNeighborsValue(new Dictionary<CoordDir, int>() {
                { CoordDir.NE, 1 }, { CoordDir.E, 4 }, { CoordDir.SE, 0 },
                { CoordDir.SW, 0 }, { CoordDir.W, 0 }, { CoordDir.NW, 6 }
            })
        },
        new HexDTO() {
            ID = 6, Y = 1, X = 0, Elevation = 0,
            NeighborsValue = Hex.ToNeighborsValue(new Dictionary<CoordDir, int>() {
                { CoordDir.NE, 7 }, { CoordDir.E, 1 }, { CoordDir.SE, 5 },
                { CoordDir.SW, 0 }, { CoordDir.W, 0 }, { CoordDir.NW, 0 }
            })
        },
        new HexDTO() {
            ID = 7, Y = 2, X = 0, Elevation = 0,
            NeighborsValue = Hex.ToNeighborsValue(new Dictionary<CoordDir, int>() {
                { CoordDir.NE, 0 }, { CoordDir.E, 2 }, { CoordDir.SE, 1 },
                { CoordDir.SW, 6 }, { CoordDir.W, 0 }, { CoordDir.NW, 0 }
            })
        },
    };
}

public enum CoordDir
{
    NE, E, SE, SW, W, NW
}
public enum EdgeCoordDir
{
    N, NE, SE, S, SW, NW
}

public enum SlopeDir { ASC, DESC, LEVEL }
public enum EdgeSide { LEFT, RIGHT }

public enum HexProp { NAME, SLOPE_ELEVATION, SIDE, COORD, VARIATION }

public class Hex
{
    public int ID;
    public int Y;
    public int X;

    public int Elevation;
    public HexRenderer HexRenderer;

    public Dictionary<CoordDir, HexBuilder.Coord> Neighbors;
    public Dictionary<CoordDir, int> NeighborsIds;
    public RenderSettings RenderSettings;

    public Hex()
    {
    }

    public Hex(int id, int y, int x)
    {
        ID = id;
        Y = y;
        X = x;
    }

    public Hex(HexDTO hexDto)
    {
        ID = hexDto.ID;
        Y = hexDto.Y;
        X = hexDto.X;

        Elevation = hexDto.Elevation;

        NeighborsIds = getNeighborsIds(hexDto.NeighborsValue);
        RenderSettings = getRenderSettings(hexDto.RenderSettingsValue);
    }

    public static string ToNeighborsValue(Dictionary<CoordDir, int> NeighborsIds)
    {
        string value = string.Empty;
        foreach (var kvp in NeighborsIds)
        {
            value += kvp.Key.ToString() + "." + kvp.Value + ";";
        }
        return value;
    }

    internal Dictionary<CoordDir, HexBuilder.Coord> CalculateAndSetNeighbors(ref Dictionary<int, Dictionary<int, Hex>> hexes)
    {
        Dictionary<CoordDir, HexBuilder.Coord> coordsToBuildBridgesOrEdges = null;
        foreach (CoordDir coordDir in (CoordDir[])Enum.GetValues(typeof(CoordDir)))
        {
            var isOddRow = HexBuilder.IsOddRow(Y);
            var offset = HexBuilder.GetCoordOffset(isOddRow, coordDir);
            var chkCoord = new HexBuilder.Coord() { Y = Y + offset.Y, X = X + offset.X };

            var exists = HexBuilder.DoesHexExist(chkCoord, ref hexes);

            if (exists)
            {
                if (coordsToBuildBridgesOrEdges == null)
                {
                    coordsToBuildBridgesOrEdges = new Dictionary<CoordDir, HexBuilder.Coord>();
                }

                var areNeighbors = Neighbors.ContainsKey(coordDir);
                if (!areNeighbors)
                {
                    AddNeighbor(coordDir, chkCoord, hexes[chkCoord.Y][chkCoord.X].ID);
                    var oppositeCoord = new HexBuilder.Coord(Y, X);
                    hexes[chkCoord.Y][chkCoord.X].AddNeighbor(HexBuilder.GetOppositeCoord(coordDir), oppositeCoord, hexes[oppositeCoord.Y][oppositeCoord.X].ID);
                }
                coordsToBuildBridgesOrEdges.Add(coordDir, chkCoord);
            }
        }
        return coordsToBuildBridgesOrEdges;
    }

    internal void AddNeighbor(CoordDir coordDir, HexBuilder.Coord newNeighborCoord, int id)
    {
        HexRenderer.HexDebug?.SetNeighbor(coordDir, id);

        if (Neighbors == null)
        {
            Neighbors = new Dictionary<CoordDir, HexBuilder.Coord>();
        }

        Neighbors.Add(coordDir, newNeighborCoord);

        if (NeighborsIds == null)
        {
            NeighborsIds = new Dictionary<CoordDir, int>();
        }

        NeighborsIds.Add(coordDir, id);
    }

    private Dictionary<CoordDir, int> getNeighborsIds(string neighborsValue)
    {
        var neighbors = new Dictionary<CoordDir, int>();
        if (string.IsNullOrEmpty(neighborsValue)) { return neighbors; }

        string[] neighborsArr = neighborsValue.Split(';');

        for (int i = 0; i < neighborsArr.Length; i++)
        {
            if (string.IsNullOrEmpty(neighborsArr[i])) { continue; }

            var kvp = neighborsArr[i].Split('.');
            var coordDir = (CoordDir)Enum.Parse(typeof(CoordDir), kvp[0]);
            var id = int.Parse(kvp[1]);
            neighbors.Add(coordDir, id);
        }
        return neighbors;
    }

    private RenderSettings getRenderSettings(string renderSettingsValue)
    {
        var renderSettings = new RenderSettings();
        if (string.IsNullOrEmpty(renderSettingsValue)) { return renderSettings; }

        string[] renderSettingsArr = renderSettingsValue.Split(';');
        renderSettings.Bridges = new Dictionary<CoordDir, Bridge>();

        for (int i = 0; i < renderSettingsArr.Length; i++)
        {
            if (string.IsNullOrEmpty(renderSettingsArr[i])) { continue; }

            var settings = renderSettingsArr[i].Split('.');
            switch (settings[(int)HexProp.NAME].Trim())
            {
                case "Bridge":
                    var coordDir = (CoordDir)Enum.Parse(typeof(CoordDir), settings[(int)HexProp.COORD]);
                    int elevation;
                    var slope = getSlope(settings[(int)HexProp.SLOPE_ELEVATION], out elevation);
                    var bridge = new Bridge()
                    {
                        SlopeDir = slope,
                        Elevation = elevation
                    };
                    renderSettings.Bridges.Add(coordDir, bridge);
                    break;
                case "BridgeEdge":
                    break;
                default:
                    break;
            }
        }
        return renderSettings;
    }

    private SlopeDir getSlope(string slopeString, out int elevation)
    {
        switch (slopeString[0])
        {
            case 'A':
                slopeString = slopeString.Substring(1);
                elevation = int.Parse(slopeString);
                return SlopeDir.ASC;
            case 'D':
                slopeString = slopeString.Substring(1);
                elevation = int.Parse(slopeString);
                return SlopeDir.ASC;
            default:
                elevation = 0;
                return SlopeDir.LEVEL;
        }
    }
}

public class HexDTO
{
    public int ID;
    public int Y;
    public int X;

    public int Elevation;

    // [coordDir].[hexId];
    // NE.1;
    // E.2;
    public string NeighborsValue;

    // [name].[slope, elevation].[coordDir].[variation].[side]:[side]
    //                                              side=[side].[slope, elevation].[variation]
    // Bridge.A1.NE.1;
    // Bridge.A1.E.1;
    // Bridge.A1.SE.1;
    // BridgeEdge..N.1.L.A1.1:R.A1.1;
    public string RenderSettingsValue;
}

public class RenderSettings
{
    public Dictionary<CoordDir, Bridge> Bridges;
    public Dictionary<CoordDir, BridgeEdge> BridgeEdges;
}

public class Bridge
{
    public SlopeDir SlopeDir;
    public int Elevation;
}

public class BridgeEdge
{
    public SlopeDir SlopeDir;
    public int Elevation;
}
