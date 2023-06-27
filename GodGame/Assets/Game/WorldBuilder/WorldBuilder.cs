using Game.Shared.DataModels;
using Game.Shared.Enums;
using Game.Shared.Interfaces;
using Game.Shared.Structs;
using Game.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using static Game.Shared.Structs.HexPartsStructs;

namespace Game.WorldBuilder
{
    public class WorldBuilder : MonoBehaviour
    {
        public HexPartsSO HexParts;
        [SerializeField]
        private Transform _hexParentT;

        public ICoord MiddleCoord;

        public Dictionary<int, Dictionary<int, IHex>> Hexes; // rename to WorldHexes
        public Queue<ICoord> OnEdgeHexes;
        private ICoord _curBuildCoord;
        private int _currentBuilderHexId = 0;

        private Subject<int> _discoverAdjacent__s = new Subject<int>();

        public void PreSetup()
        {
            // Civ 6 gives an big map of 66/106 (h/w)
            MiddleCoord = new Coord(66 / 2, 106 / 2);
            _curBuildCoord = MiddleCoord;
            Hexes = new Dictionary<int, Dictionary<int, IHex>>();
            OnEdgeHexes = new Queue<ICoord>();

            HexParts.Init();

            _discoverAdjacent__s
                .Do((int hexCount) =>
                {
                    for (int i = 0; i < hexCount - 1; i++)
                    {
                        var hex = Hexes.GetAtCoord(_curBuildCoord);

                        var hasAllNeighbors = hex.Neighbors.Count == 6;
                        if (hasAllNeighbors)
                        {
                            _curBuildCoord = OnEdgeHexes.GetEdgeCoord(ref Hexes);
                            i--;
                            continue;
                        }

                        var dir = hex.Neighbors.GetMissingNeighbor();

                        var isOddRow = HexUtils.IsOddRow(hex.Y);
                        var offset = HexUtils.GetCoordOffset(isOddRow, dir);
                        var newNeighborCoord = Coord.AddTogheter(offset, _curBuildCoord);
                        Debug.Log("newNeighborCoord: " + newNeighborCoord);

                        _currentBuilderHexId++;
                        var newHex = new Hex(_currentBuilderHexId, newNeighborCoord.Y, newNeighborCoord.X);
                        Hexes.AddHex(newHex);
                        OnEdgeHexes.Enqueue(new Coord(newHex.Y, newHex.X));

                        placeHexOnBoard(newHex);

                        hex.AddNeighbor(dir, newNeighborCoord, _currentBuilderHexId);
                        addMultipleNeighbors(_curBuildCoord, dir, Hexes[newNeighborCoord.Y][newNeighborCoord.X], ref Hexes);

                        //Debug.Log("_moveHexes[" + newNeighborCoord.Y + "][" + newNeighborCoord.X + "].Neighbors: " + _moveHexes[newNeighborCoord.Y][newNeighborCoord.X].Neighbors.ToStringDebug());
                    }
                })
                .DelayFrame(60)
                .Do(_ =>
                {

                    foreach (var hexY in Hexes)
                    {
                        foreach (var hexX in hexY.Value)
                        {
                            buildBridges(hexX.Value);
                        }
                    }
                })
                .DelayFrame(60)
                .Do(_ =>
                {

                    foreach (var hexY in Hexes)
                    {
                        foreach (var hexX in hexY.Value)
                        {
                            // needs rework with EdgeStitch solution
                            //buildEdges(hexX.Value);
                        }
                    }
                })
                .Subscribe();
        }

        public void Init()
        {
            createFirstHex();

            //calculateCombinationsForEdgeStitch();
        }

        public void DiscoverWorld(int visionRange)
        {
            int hexCount = HexUtils.GetHexCountByRange(visionRange);
            _discoverAdjacent__s.OnNext(hexCount);
        }

        public static void addMultipleNeighbors(ICoord curCoord, Dir dir, IHex hex, ref Dictionary<int, Dictionary<int, IHex>> hexes)
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
                hex.AddNeighbor(neighborDirCoord.PerspectiveDir, curCoord, hexes.GetAtCoord(curCoord).ID);
            }

            neighborDirCoord = neighborAdjacentCoords[(int)DirAdjacent.Left];
            var coord = neighborDirCoord.Coord.Plus(curCoord);
            exists = hexes.HexExist(coord);
            if (exists)
            {
                hex.AddNeighbor(neighborDirCoord.PerspectiveDir, coord, hexes.GetAtCoord(coord).ID);
                var oppositeDir = HexUtils.OPPOSITE_DIR[neighborDirCoord.PerspectiveDir];
                hexes[coord.Y][coord.X].AddNeighbor(oppositeDir, dirCoord, hex.ID);
            }

            neighborDirCoord = neighborAdjacentCoords[(int)DirAdjacent.Right];
            coord = neighborDirCoord.Coord.Plus(curCoord);
            exists = hexes.HexExist(coord);
            if (exists)
            {
                hex.AddNeighbor(neighborDirCoord.PerspectiveDir, coord, hexes.GetAtCoord(coord).ID);
                var oppositeDir = HexUtils.OPPOSITE_DIR[neighborDirCoord.PerspectiveDir];
                hexes[coord.Y][coord.X].AddNeighbor(oppositeDir, dirCoord, hex.ID);
            }
        }

        private void buildBridges(IHex hex)
        {
            foreach (var hxN in hex.Neighbors)
            {
                var hexComponent = hex.HexComponent as HexComponent;
                var hasHexBridgeToNeighbor = hexComponent.Bridges.ContainsKey(hxN.Key);

                if (hasHexBridgeToNeighbor) { continue; }

                var neighborHex = Hexes[hxN.Value.Y][hxN.Value.X];

                var bridge = new Bridge()
                {
                    SlopeDir = HexUtils.GetSlopeDir(hex.Elevation, neighborHex.Elevation),
                    Version = 0
                };
                bridge.Elevation = (bridge.SlopeDir == SlopeDir.LEVEL)
                    ? 0
                    : (bridge.SlopeDir == SlopeDir.ASC) ? neighborHex.Elevation - hex.Elevation : hex.Elevation - neighborHex.Elevation;

                placeBridgeOnBoard(hex, bridge, hxN.Key);
                hexComponent.Bridges.Add(hxN.Key, bridge);

                var neighborHexComponent = neighborHex.HexComponent as HexComponent;
                var oppositeDir = HexUtils.OPPOSITE_DIR[hxN.Key];

                var hasOppositeHexBridgeToNeighbor = neighborHexComponent.Bridges.ContainsKey(oppositeDir);
                if (hasOppositeHexBridgeToNeighbor) { continue; }

                bridge.SlopeDir = HexUtils.Opposite(bridge.SlopeDir);
                // bridge.Elevation = Diff
                placeBridgeOnBoard(neighborHex, bridge, oppositeDir);

                neighborHexComponent.Bridges.Add(oppositeDir, bridge);

                //Debug.Log("hex[" + hex.Y + "," + hex.X + "].Bridge[" + hxN.Key + "]: " + (hex.HexComponent as HexComponent).Bridges[hxN.Key]);
                //Debug.Log("nHex[" + hxN.Value.Y + "," + hxN.Value.X + "].Bridge[" + oppositeDir + "]: " + (Hexes[hxN.Value.Y][hxN.Value.X].HexComponent as HexComponent).Bridges[oppositeDir]);
            }
        }

        private void placeBridgeOnBoard(IHex hex, Bridge bridge, Dir dir)
        {
            var bridgePrefab = HexParts.GetBridge(bridge);
            try
            {
                var go = Instantiate(bridgePrefab.Model, hex.HexComponent.Transform);
                //go.transform.position = HexUtils.GetHexPosition(hex.Y, hex.X, bridge.Elevation);
                go.transform.eulerAngles = HexUtils.GetBridgeRotationByCoord(dir);
                go.name = go.name.Replace("(Clone)", "_" + dir.ToString());
            }
            catch (Exception e)
            {
                Debug.LogWarning("Can't find " + bridge.CreateModelName() + " for hex[" + hex.Y + ", " + hex.X + "] => " + e.Message);
            }
        }

        private void buildEdges(IHex hex)
        {
            foreach (var edgeDir in HexUtils.EDGE_DIRECTIONS)
            {
                var originHexComponent = hex.HexComponent as HexComponent;
                var hasBridgeEdge = originHexComponent.BridgeEdges.ContainsKey(edgeDir);

                if (hasBridgeEdge) { continue; }

                var originBridgeEdgeProps = getBridgeEdgeProperties(originHexComponent, edgeDir);

                if (!originBridgeEdgeProps.hasLeftBridge) { continue; }

                var leftNeighborHex = Hexes.GetAtCoord(hex.Neighbors[originBridgeEdgeProps.lDir]);
                var leftNeighborHexComponent = leftNeighborHex.HexComponent as HexComponent;
                var leftNeighborEdgeDir = HexUtils.LEFT_OPPOSITE_EDGE_DIR[edgeDir];

                var leftBridgeEdgeProperties = getBridgeEdgeProperties(leftNeighborHexComponent, leftNeighborEdgeDir);

                if (!leftBridgeEdgeProperties.hasLeftBridge) { continue; }

                var rightNeighborHex = Hexes.GetAtCoord(leftNeighborHex.Neighbors[leftBridgeEdgeProperties.lDir]);
                var rightNeighborHexComponent = rightNeighborHex.HexComponent as HexComponent;
                var rightNeighborEdgeDir = HexUtils.LEFT_OPPOSITE_EDGE_DIR[leftNeighborEdgeDir];

                var rightBridgeEdgeProperties = getBridgeEdgeProperties(rightNeighborHexComponent, rightNeighborEdgeDir);

                if (!rightBridgeEdgeProperties.hasLeftBridge) { continue; }

                // if we get here then we have a bridgeWhole and can create the edge bridges

                var planeVersion = originHexComponent.Version;

                var leftBridge = originHexComponent.Bridges[originBridgeEdgeProps.lDir];
                var rightBridge = originHexComponent.Bridges[originBridgeEdgeProps.rDir];

                placeBridgeEdgeOnBoard(Adjacent.Left, planeVersion, leftBridge, originHexComponent, edgeDir, originBridgeEdgeProps.lDir, rightBridge);
                placeBridgeEdgeOnBoard(Adjacent.Right, planeVersion, rightBridge, originHexComponent, edgeDir, originBridgeEdgeProps.rDir, leftBridge);

                originHexComponent.BridgeEdges.Add(edgeDir, true);
            }
        }

        private (Dictionary<Adjacent, Dir> adjacentDirs, Dir lDir, Dir rDir, bool hasLeftBridge) getBridgeEdgeProperties(HexComponent hexComponent, EdgeDir edgeDir)
        {
            (Dictionary<Adjacent, Dir> adjacentDirs, Dir lDir, Dir rDir, bool hasLeftBridge) properties;

            properties.adjacentDirs = HexUtils.ADJACENT_DIR_OF_EDGE_DIR[edgeDir];
            properties.lDir = properties.adjacentDirs[Adjacent.Left];
            properties.rDir = properties.adjacentDirs[Adjacent.Right];
            properties.hasLeftBridge = hexComponent.Bridges.ContainsKey(properties.lDir);

            return properties;
        }

        private void placeBridgeEdgeOnBoard(Adjacent adjacent, int planeVersion, Bridge bridge, IHexComponent hexComponent, EdgeDir edgeDir, Dir dir, Bridge otherBridge)
        {
            var bridgeEdgePrefab = HexParts.GetBridgeEdge(
                adjacent,
                bridge.SlopeDir, bridge.Elevation,
                bridge.Version,
                planeVersion,
                otherBridge.SlopeDir, otherBridge.Elevation,
                otherBridge.Version
            );
            var name = BridgeEdge.CreateBridgeEdgeName(
                adjacent,
                bridge.SlopeDir, bridge.Elevation,
                bridge.Version,
                planeVersion,
                otherBridge.SlopeDir, otherBridge.Elevation,
                otherBridge.Version,
                0
            );
            Debug.Log("name: " + name
                + " on for { dir: " + dir + ", edgeDir: " + edgeDir + ", go: " + hexComponent.Transform.gameObject.name);
            //try
            //{
            var go = Instantiate(bridgeEdgePrefab.Model, hexComponent.Transform);
            go.transform.eulerAngles = HexUtils.GetBridgeEdgeRotationByCoord(edgeDir);
            go.name = go.name.Replace("(Clone)", "_[" + (adjacent == Adjacent.Left ? "L" : "R") + "_" + dir.ToString() + "]");
            //}
            //catch (Exception e)
            //{
            //    var name = BridgeEdge.CreateBridgeEdgeName(adjacent, planeVersion, bridge.SlopeDir, bridge.Elevation, bridge.Version);

            //    Debug.LogWarning("Can't find " + name
            //        + " on for { dir: " + dir + ", edgeDir: " + edgeDir + ", go: " + hexComponent.Transform.gameObject.name + " => " + e.Message);
            //}
        }

        private void createFirstHex()
        {
            _currentBuilderHexId++;
            var hex = new Hex(_currentBuilderHexId, MiddleCoord);
            hex.Elevation = 1;

            Hexes.AddHex(hex);
            OnEdgeHexes.Enqueue(new Coord(hex.Y, hex.X));

            placeHexOnBoard(hex);
        }

        private void placeHexOnBoard(IHex hex)
        {
            var version = 0;
            var prefab = HexParts.GetHexPlanes(version);
            var go = Instantiate(prefab.Model, _hexParentT);
            go.name = "hex" + hex.Y + "_" + hex.X;

            var pos = HexUtils.GetHexPosition(hex);
            go.transform.position = pos;

            Hexes[hex.Y][hex.X].HexComponent = go.GetComponent<HexComponent>();
            Hexes[hex.Y][hex.X].HexComponent.Init(Hexes[hex.Y][hex.X]);

            //Hexes[hex.Y][hex.X].HexRenderer.SetHexVersion(hex.Elevation, version, ref HexParts
        }

        private void calculateCombinationsForEdgeStitch()
        {
            var maxDifferenceInLevel = 4; // 8 is too much and 3 doesn't cover a wide range of terrain
            var combinations = new List<int[]>();
            for (int i = 0; i <= maxDifferenceInLevel; i++)
            {
                for (int j = 0; j <= maxDifferenceInLevel; j++)
                {
                    for (int k = 0; k <= maxDifferenceInLevel; k++)
                    {
                        var combination = new int[3] { i, j, k };
                        combinations.Add(combination);
                    }
                }
            }
            Debug.Log("combinations.Count: " + combinations.Count);

            var filteredCombinations = new List<int[]>();
            foreach (var combination in combinations)
            {
                var minNr = combination.Min();
                var i = combination[0] - minNr;
                var j = combination[1] - minNr;
                var k = combination[2] - minNr;
                var newComb = new int[3] { i, j, k };

                var exists = filteredCombinations.Find((int[] comb) =>
                {
                    var exactSameCombination =
                           comb[0] == newComb[0]
                        && comb[1] == newComb[1]
                        && comb[2] == newComb[2];
                    var variant1 =
                           comb[0] == newComb[1]
                        && comb[1] == newComb[2]
                        && comb[2] == newComb[0];
                    var variant2 =
                           comb[0] == newComb[2]
                        && comb[1] == newComb[0]
                        && comb[2] == newComb[1];
                    return exactSameCombination || variant1 || variant2;
                });
                if (exists != null) { continue; }

                filteredCombinations.Add(newComb);
            }
            Debug.Log("filteredCombinations.Count: " + filteredCombinations.Count);

            foreach (var combination in filteredCombinations)
            {
                Debug.Log("combination: " + combination[0] + ", " + combination[1] + ", " + combination[2]);
            }

            throw new Exception();
        }
    }
}