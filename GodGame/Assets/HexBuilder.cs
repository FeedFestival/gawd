using System;
using System.Collections.Generic;
using UnityEngine;

public class HexBuilder : MonoBehaviour
{
    public HexPartsSO HexParts;
    [SerializeField]
    private Transform _hexParentT;

    public struct Coord
    {
        public int Y;
        public int X;

        public Coord(int y, int x)
        {
            Y = y;
            X = x;
        }

        public override string ToString()
        {
            return Y + "_" + X;
        }

        internal Coord Plus(Coord hexCoord)
        {
            Y = Y + hexCoord.Y;
            X = X + hexCoord.X;
            return this;
        }
    }
    // Civ 6 gives an big map of 66/106 (h/w)
    private readonly Coord _middleCoord = new Coord(66 / 2, 106 / 2);

    public Dictionary<int, Dictionary<int, Hex>> Hexes;
    //public Dictionary<int, Dictionary<int, Hex>> OnEdgeHexes;
    public Queue<Coord> OnEdgeHexes;

    private HexDataService _hexDataService = new HexDataService();

    public const float VERTICAL_ADJACENT___OFFSET = 0.8660254f;
    public const float HORIZONTAL_ADJACENT_OFFSET = 1.7320508f;

    private int _currentBuilderHexId = 0;
    private Coord _currBuilderHexCoord;

    void Start()
    {
        Hexes = new Dictionary<int, Dictionary<int, Hex>>();

        // theOldWay();

        createFirstHex();

        _currBuilderHexCoord = _middleCoord;
        var firstHex = Hexes[_currBuilderHexCoord.Y][_currBuilderHexCoord.X];
        placeCamera(firstHex);
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.E))
        {
            discoverNewHex();
        }
    }

    private void discoverNewHex()
    {
        var hex = Hexes[_currBuilderHexCoord.Y][_currBuilderHexCoord.X];

        if (hex.Neighbors != null && hex.Neighbors.Count == 6)
        {
            foreach (var hexKvp in hex.Neighbors)
            {
                EdgeCoordDir[] edgeCoordDirs = HexRenderer.GetEdgeCoordDirsOfCoordDir(hexKvp.Key);
                hex.HexRenderer.buildEdge(EdgeSide.LEFT, hexKvp.Key, edgeCoordDirs[(int)EdgeSide.LEFT], ref HexParts);
                hex.HexRenderer.buildEdge(EdgeSide.RIGHT, hexKvp.Key, edgeCoordDirs[(int)EdgeSide.RIGHT], ref HexParts);
                buildRemainingBridgesOrEdges(hexKvp.Value);
            }

            // we are done with this hex
            _currBuilderHexCoord = getAnHexEdge();
            discoverNewHex();
            return;
        }

        var coordDir = getMissingNeighborCoordDir(hex.Neighbors);
        var isOddRow = IsOddRow(hex.Y);
        var offset = GetCoordOffset(isOddRow, coordDir);
        var newNeighborCoord = new Coord(hex.Y + offset.Y, hex.X + offset.X);

        _currentBuilderHexId++;
        var newHex = new Hex(_currentBuilderHexId, newNeighborCoord.Y, newNeighborCoord.X);

        addHexToHexes(newHex);
        placeHexOnBoard(newHex);

        hex.AddNeighbor(coordDir, newNeighborCoord, Hexes[newNeighborCoord.Y][newNeighborCoord.X].ID);
        Hexes[newNeighborCoord.Y][newNeighborCoord.X].AddNeighbor(GetOppositeCoord(coordDir), _currBuilderHexCoord, Hexes[_currBuilderHexCoord.Y][_currBuilderHexCoord.X].ID);

        Hexes[hex.Y][hex.X].HexRenderer.BuildBridges(coordDir, Hexes[newNeighborCoord.Y][newNeighborCoord.X], ref HexParts);

        buildNeighboringBridges(_currBuilderHexCoord);
    }

    private void createFirstHex()
    {
        _currentBuilderHexId++;
        var hex = new Hex(_currentBuilderHexId, _middleCoord.Y, _middleCoord.X);
        hex.Elevation = 1;

        addHexToHexes(hex);
        placeHexOnBoard(hex);
    }

    private void theOldWay()
    {
        createDatabaseHexes();
        //createBridge();
    }

    private void createDatabaseHexes()
    {
        var hexes = _hexDataService.GetHexes();
        foreach (var hex in hexes)
        {
            addHexToHexes(hex);
            placeHexOnBoard(hex);
        }
    }

    private void placeHexOnBoard(Hex hex)
    {
        var go = Instantiate(HexParts.BasicHex, _hexParentT);
        go.name = "hex" + hex.Y + "_" + hex.X;

        float x;
        var yOffset = 1.5f;
        if (IsOddRow(hex.Y))
        {
            var isFirst = hex.X == 0;
            if (isFirst)
            {
                x = -VERTICAL_ADJACENT___OFFSET;
            }
            else
            {
                x = (hex.X * HORIZONTAL_ADJACENT_OFFSET) - VERTICAL_ADJACENT___OFFSET;
            }
        }
        else
        {
            x = hex.X * HORIZONTAL_ADJACENT_OFFSET;
        }
        go.transform.position = new Vector3(x, 0.2f * hex.Elevation, hex.Y * yOffset);

        Hexes[hex.Y][hex.X].HexRenderer = go.GetComponent<HexRenderer>();
        Hexes[hex.Y][hex.X].HexRenderer.InitSettings(Hexes[hex.Y][hex.X]);

        var elevation = 1;
        var version = 1;
        Hexes[hex.Y][hex.X].HexRenderer.SetHexVersion(elevation, version, ref HexParts);

        addHexToEdgeHexes(Hexes[hex.Y][hex.X]);
    }

    private void addHexToHexes(Hex hex)
    {
        var exists = Hexes.ContainsKey(hex.Y);
        if (!exists)
        {
            Hexes.Add(hex.Y, new Dictionary<int, Hex>());
        }
        Hexes[hex.Y].Add(hex.X, hex);
    }

    private void addHexToEdgeHexes(Hex hex)
    {
        if (OnEdgeHexes == null)
        {
            OnEdgeHexes = new Queue<Coord>();
        }
        OnEdgeHexes.Enqueue(new Coord(hex.Y, hex.X));
    }

    private Coord getAnHexEdge()
    {
        var coord = OnEdgeHexes.Dequeue();
        if (Hexes[coord.Y][coord.X].Neighbors.Count < 6)
        {
            return coord;
        }
        return getAnHexEdge();
    }

    private void placeCamera(Hex hex)
    {
        var camOffset = new Vector3(0, 4f, -2.9f);
        Camera.main.transform.position = hex.HexRenderer.transform.position + camOffset;
    }

    private void buildNeighboringBridges(Coord hexCoord)
    {
        Debug.Log("--------------- " + hexCoord.ToString() + " ---------------");
        foreach (CoordDir coordDir in (CoordDir[])Enum.GetValues(typeof(CoordDir)))
        {
            var isOddRow = IsOddRow(hexCoord.Y);
            var coords = GetCoordNeighborsOfNeighbors(isOddRow, coordDir);

            var dirCoord = coords[1].Plus(hexCoord);
            bool exists = DoesHexExist(dirCoord, ref Hexes);
            if (!exists) { return; }

            var leftCoord = coords[0].Plus(hexCoord);
            exists = DoesHexExist(leftCoord, ref Hexes);
            if (exists)
            {
                Debug.Log("leftCoord: " + leftCoord.ToString() + " <<-- dirCoord: " + dirCoord.ToString());
                //var hasBridge = Hexes[leftCoord.Y][leftCoord.X].HexRenderer.BridgeSettings.ContainsKey(oppositeCoordDir);
                // make Neighbor -> dirCoord
                // make Bridge -> dirCoord
            }

            var rightCoord = coords[2].Plus(hexCoord);
            exists = DoesHexExist(rightCoord, ref Hexes);
            if (exists)
            {
                Debug.Log("dirCoord: " + dirCoord.ToString() + " -->> rightCoord: " + rightCoord.ToString());
                // make Neighbor -> dirCoord
                // make Bridge -> dirCoord
            }
        }
        //Dictionary<CoordDir, Coord> coordsToBuildBridgesOrEdges = Hexes[hexCoord.Y][hexCoord.X].CalculateAndSetNeighbors(ref Hexes);
        //if (coordsToBuildBridgesOrEdges != null)
        //{
        //    foreach (var kvp in coordsToBuildBridgesOrEdges)
        //    {
        //        var oppositeCoordDir = GetOppositeCoord(kvp.Key);
        //        var hasBridge = Hexes[kvp.Value.Y][kvp.Value.X].HexRenderer.BridgeSettings.ContainsKey(oppositeCoordDir);
        //        if (hasBridge)
        //        {
        //            var alreadySetSettings = Hexes[hexCoord.Y][hexCoord.X].HexRenderer.BridgeSettings.ContainsKey(kvp.Key);
        //            if (alreadySetSettings) { break; }

        //            var bridgeSettings = Hexes[kvp.Value.Y][kvp.Value.X].HexRenderer.BridgeSettings[oppositeCoordDir];
        //            if (bridgeSettings.SlopeDir != SlopeDir.LEVEL)
        //            {
        //                bridgeSettings.SlopeDir = bridgeSettings.SlopeDir == SlopeDir.ASC ? SlopeDir.DESC : SlopeDir.ASC;
        //            }
        //            Hexes[hexCoord.Y][hexCoord.X].HexRenderer.BridgeSettings.Add(kvp.Key, bridgeSettings);
        //        }
        //        else
        //        {
        //            Hexes[hexCoord.Y][hexCoord.X].HexRenderer
        //                .BuildBridges(kvp.Key, Hexes[kvp.Value.Y][kvp.Value.X], ref HexParts);
        //        }
        //    }
        //}
    }

    private void buildRemainingBridgesOrEdges(Coord hexCoord)
    {
        Dictionary<CoordDir, Coord> coordsToBuildBridgesOrEdges = Hexes[hexCoord.Y][hexCoord.X].CalculateAndSetNeighbors(ref Hexes);
        if (coordsToBuildBridgesOrEdges != null)
        {
            foreach (var kvp in coordsToBuildBridgesOrEdges)
            {
                var oppositeCoordDir = GetOppositeCoord(kvp.Key);
                var hasBridge = Hexes[kvp.Value.Y][kvp.Value.X].HexRenderer.BridgeSettings.ContainsKey(oppositeCoordDir);
                if (hasBridge)
                {
                    var alreadySetSettings = Hexes[hexCoord.Y][hexCoord.X].HexRenderer.BridgeSettings.ContainsKey(kvp.Key);
                    if (alreadySetSettings) { break; }

                    var bridgeSettings = Hexes[kvp.Value.Y][kvp.Value.X].HexRenderer.BridgeSettings[oppositeCoordDir];
                    if (bridgeSettings.SlopeDir != SlopeDir.LEVEL)
                    {
                        bridgeSettings.SlopeDir = bridgeSettings.SlopeDir == SlopeDir.ASC ? SlopeDir.DESC : SlopeDir.ASC;
                    }
                    Hexes[hexCoord.Y][hexCoord.X].HexRenderer.BridgeSettings.Add(kvp.Key, bridgeSettings);
                }
                else
                {
                    Hexes[hexCoord.Y][hexCoord.X].HexRenderer
                        .BuildBridges(kvp.Key, Hexes[kvp.Value.Y][kvp.Value.X], ref HexParts);
                }

                var edgeCoordDirs = HexRenderer.GetEdgeCoordDirsOfCoordDir(kvp.Key);
                Hexes[hexCoord.Y][hexCoord.X].HexRenderer.buildEdge(EdgeSide.LEFT, kvp.Key, edgeCoordDirs[(int)EdgeSide.LEFT], ref HexParts);
                Hexes[hexCoord.Y][hexCoord.X].HexRenderer.buildEdge(EdgeSide.RIGHT, kvp.Key, edgeCoordDirs[(int)EdgeSide.RIGHT], ref HexParts);
            }
        }
    }

    public static Coord GetCoordOffset(bool isOddRow, CoordDir coordDir)
    {
        switch (coordDir)
        {
            case CoordDir.NE:
                return isOddRow ? new Coord(1, 0) : new Coord(1, 1);
            case CoordDir.E:
                return new Coord(0, 1);
            case CoordDir.SE:
                return isOddRow ? new Coord(-1, 0) : new Coord(-1, 1);
            case CoordDir.SW:
                return isOddRow ? new Coord(-1, -1) : new Coord(-1, 0);
            case CoordDir.W:
                return new Coord(0, -1);
            case CoordDir.NW:
            default:
                return isOddRow ? new Coord(1, -1) : new Coord(1, 0);
        }
    }

    public static Coord[] GetCoordNeighborsOfNeighbors(bool isOddRow, CoordDir coordDir)
    {
        switch (coordDir)
        {
            case CoordDir.NE:
                return new Coord[3] {
                    isOddRow ? new Coord(1, -1) : new Coord(1, 0),
                    isOddRow ? new Coord(1, 0) : new Coord(1, 1),
                    new Coord(0, 1)
                };
            case CoordDir.E:
                return new Coord[3] {
                    isOddRow ? new Coord(1, 0) : new Coord(1, 1),
                    new Coord(0, 1),
                    isOddRow ? new Coord(-1, 0) : new Coord(-1, 1)
                };
            case CoordDir.SE:
                return new Coord[3] {
                    new Coord(0, 1),
                    isOddRow ? new Coord(-1, 0) : new Coord(-1, 1),
                    isOddRow ? new Coord(-1, -1) : new Coord(-1, 0)
                };
            case CoordDir.SW:
                return new Coord[3] {
                    isOddRow ? new Coord(-1, 0) : new Coord(-1, 1),
                    isOddRow ? new Coord(-1, -1) : new Coord(-1, 0),
                    new Coord(0, -1)
                };
            case CoordDir.W:
                return new Coord[3] {
                    isOddRow ? new Coord(-1, -1) : new Coord(-1, 0),
                    new Coord(0, -1),
                    isOddRow ? new Coord(1, -1) : new Coord(1, 0)
                };
            case CoordDir.NW:
            default:
                return new Coord[3] {
                    new Coord(0, -1),
                    isOddRow ? new Coord(1, -1) : new Coord(1, 0),
                    isOddRow ? new Coord(1, 0) : new Coord(1, 1)
                };
        }
    }

    public static CoordDir GetOppositeCoord(CoordDir coord)
    {
        switch (coord)
        {
            case CoordDir.NE:
                return CoordDir.SW;
            case CoordDir.E:
                return CoordDir.W;
            case CoordDir.SE:
                return CoordDir.NW;
            case CoordDir.SW:
                return CoordDir.NE;
            case CoordDir.W:
                return CoordDir.E;
            case CoordDir.NW:
            default:
                return CoordDir.SE;
        }
    }

    private CoordDir getMissingNeighborCoordDir(Dictionary<CoordDir, Coord> hexNeighbors)
    {
        if (hexNeighbors == null) { return CoordDir.NE; }

        foreach (CoordDir coordDir in (CoordDir[])Enum.GetValues(typeof(CoordDir)))
        {
            if (hexNeighbors.ContainsKey(coordDir))
            {
                continue;
            }
            return coordDir;
        }
        return CoordDir.NW;
    }

    public static bool IsOddRow(int y)
    {
        return !(y % 2 == 0);
    }

    public static bool DoesHexExist(Coord coord, ref Dictionary<int, Dictionary<int, Hex>> hexes)
    {
        var exists = hexes.ContainsKey(coord.Y);
        if (exists)
        {
            exists = hexes[coord.Y].ContainsKey(coord.X);
        }
        return exists;
    }
}
