using Game.Shared.DataModels;
using Game.Shared.Enums;
using Game.Shared.Interfaces;
using Game.Shared.Structs;
using Game.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private List<ICoord> _justAdded;

        private Subject<int> _discoverAdjacent__s = new Subject<int>();
        private Subject<int> _buildAdjacentBridges__s = new Subject<int>();
        private Subject<int> _buildAdjacentEdges__s = new Subject<int>();

        private int _frameDelay = 10;

        public void PreSetup()
        {
            // Civ 6 gives an big map of 66/106 (h/w)
            MiddleCoord = new Coord(66 / 2, 106 / 2);
            _curBuildCoord = MiddleCoord;
            Hexes = new Dictionary<int, Dictionary<int, IHex>>();
            OnEdgeHexes = new Queue<ICoord>();
            _justAdded = new List<ICoord>();

            HexParts.Init();

            _buildAdjacentBridges__s
                //.DelayFrame(_frameDelay)
                .Do(i =>
                {
                    buildBridges(Hexes[_justAdded[i].Y][_justAdded[i].X]);

                    i++;
                    if (i == _justAdded.Count)
                    {
                        _buildAdjacentEdges__s.OnNext(0);
                        return;
                    }

                    _buildAdjacentBridges__s.OnNext(i);
                })
                .Subscribe();

            _buildAdjacentEdges__s
                .DelayFrame(_frameDelay)
                .Do(i =>
                {
                    buildEdges(Hexes[_justAdded[i].Y][_justAdded[i].X]);

                    i++;
                    if (i == _justAdded.Count)
                    {
                        return;
                    }

                    _buildAdjacentEdges__s.OnNext(i);
                })
                .Subscribe();

            _discoverAdjacent__s
                .Do((int hexCount) => discoverAdjacent(hexCount))
                .DelayFrame(_frameDelay)
                .Do(_ => _buildAdjacentBridges__s.OnNext(0))
                .Subscribe();
        }

        public void Init()
        {
            createFirstHex();

            calculateCombinationsForEdgeStitch();
        }

        public void DiscoverWorld(int visionRange)
        {
            int hexCount = HexUtils.GetHexCountByRange(visionRange);
            _discoverAdjacent__s.OnNext(hexCount);
        }

        private void discoverAdjacent(int hexCount)
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

                _currentBuilderHexId++;
                var newHex = new Hex(_currentBuilderHexId, newNeighborCoord.Y, newNeighborCoord.X, _currentBuilderHexId == 2 ? 1 : 0);
                Hexes.AddHex(newHex);
                //OnEdgeHexes.Enqueue(new Coord(newHex.Y, newHex.X));
                OnEdgeHexes.Enqueue(newNeighborCoord);

                placeHexOnBoard(newHex);

                hex.AddNeighbor(dir, newNeighborCoord, _currentBuilderHexId);
                addMultipleNeighbors(_curBuildCoord, dir, Hexes[newNeighborCoord.Y][newNeighborCoord.X], ref Hexes);

                _justAdded.Add(newNeighborCoord);

                //Debug.Log("_moveHexes[" + newNeighborCoord.Y + "][" + newNeighborCoord.X + "].Neighbors: " + _moveHexes[newNeighborCoord.Y][newNeighborCoord.X].Neighbors.ToStringDebug());
            }
        }

        private void addMultipleNeighbors(ICoord curCoord, Dir dir, IHex hex, ref Dictionary<int, Dictionary<int, IHex>> hexes)
        {
            var isOddRow = HexUtils.IsOddRow(curCoord.Y);
            var neighborAdjacentCoords = isOddRow
                ? HexUtils.ODD__NEIGHBORS_COORDS_OF_DIR[dir]
                : HexUtils.EVEN_NEIGHBORS_COORDS_OF_DIR[dir];

            var neighborDirCoord = neighborAdjacentCoords[(int)DirWay.Left];
            var dirCoord = neighborDirCoord.Coord.Plus(curCoord);
            bool exists = hexes.HexExist(dirCoord);
            if (exists)
            {
                hex.AddNeighbor(neighborDirCoord.PerspectiveDir, curCoord, hexes.GetAtCoord(curCoord).ID);
            }

            neighborDirCoord = neighborAdjacentCoords[(int)DirWay.Origin];
            var coord = neighborDirCoord.Coord.Plus(curCoord);
            exists = hexes.HexExist(coord);
            if (exists)
            {
                hex.AddNeighbor(neighborDirCoord.PerspectiveDir, coord, hexes.GetAtCoord(coord).ID);
                var oppositeDir = HexUtils.OPPOSITE_DIR[neighborDirCoord.PerspectiveDir];
                hexes[coord.Y][coord.X].AddNeighbor(oppositeDir, dirCoord, hex.ID);
            }

            neighborDirCoord = neighborAdjacentCoords[(int)DirWay.Right];
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

        private void buildEdges(IHex currentHex)
        {
            foreach (var edgeDir in HexUtils.EDGE_DIRECTIONS)
            {
                if (currentHex.ID == 2 && edgeDir == EdgeDir.S)
                {
                    var x = 0;
                }

                var hex = new Dictionary<DirWay, (IHex hex, HexComponent component, EdgeDir edgeDir)>();
                hex.Add(DirWay.Origin, getHexTuple(currentHex, edgeDir));

                var hasEdgeStitch = hex[DirWay.Origin].component.EdgeStitches.ContainsKey(edgeDir);
                //Debug.Log("[" + currentHex.Y + ", " + currentHex.X + "] - " + edgeDir + " - hasEdgeStitch: " + hasEdgeStitch);
                if (hasEdgeStitch) { continue; }

                var originEdgeStitchProps = getEdgeStitchProperties(hex[DirWay.Origin].component, edgeDir);
                if (!originEdgeStitchProps.hasLeftBridge) { continue; }

                hex.Add(DirWay.Left, getHexTuple(
                    Hexes.GetAtCoord(hex[DirWay.Origin].hex.Neighbors[originEdgeStitchProps.lDir]),
                    HexUtils.LEFT_OPPOSITE_EDGE_DIR[edgeDir]
                ));

                var leftEdgeStitchProps = getEdgeStitchProperties(hex[DirWay.Left].component, hex[DirWay.Left].edgeDir);
                if (!leftEdgeStitchProps.hasLeftBridge) { continue; }

                hex.Add(DirWay.Right, getHexTuple(
                    Hexes.GetAtCoord(hex[DirWay.Left].hex.Neighbors[leftEdgeStitchProps.lDir]),
                    HexUtils.LEFT_OPPOSITE_EDGE_DIR[hex[DirWay.Left].edgeDir]
                ));

                var rightEdgeStitchProps = getEdgeStitchProperties(hex[DirWay.Right].component, hex[DirWay.Right].edgeDir);
                if (!rightEdgeStitchProps.hasLeftBridge) { continue; }

                // if we get here then we have a Bbridge Whole and can create a Stitch

                var combination = new int[3] { hex[DirWay.Origin].hex.Elevation, hex[DirWay.Left].hex.Elevation, hex[DirWay.Right].hex.Elevation };
                var lowestDirWay = StitchElevation.GetMinIndex(combination);
                var yPos = hex[lowestDirWay].component.Transform.position.y;
                var defaultCombination = StitchElevation.GetDefaultCombination(combination);
                var rotationCount = StitchElevation.GetRotation(combination, defaultCombination);

                var version = 0;

                placeEdgeStitchOnBoard(hex[DirWay.Origin].component, edgeDir, rotationCount, yPos,
                    stitchElevation: new StitchElevation(defaultCombination),
                    planeVersion: hex[DirWay.Origin].component.Version,
                    leftBridgeVersion: hex[DirWay.Origin].component.Bridges[originEdgeStitchProps.lDir].Version,
                    leftPlaneVersion: hex[DirWay.Left].component.Version,
                    oppositeBridgeVersion: hex[DirWay.Left].component.Bridges[leftEdgeStitchProps.lDir].Version, // not sure if this points to the correct one
                    rightPlaneVersion: hex[DirWay.Right].component.Version,
                    rightBridgeVersion: hex[DirWay.Origin].component.Bridges[originEdgeStitchProps.rDir].Version,
                    version
                );

                hex[DirWay.Origin].component.EdgeStitches.Add(edgeDir, true);
                hex[DirWay.Left].component.EdgeStitches.Add(hex[DirWay.Left].edgeDir, true);
                hex[DirWay.Right].component.EdgeStitches.Add(hex[DirWay.Right].edgeDir, true);
            }
        }

        private (IHex hex, HexComponent component, EdgeDir edgeDir) getHexTuple(IHex hex, EdgeDir edgeDir)
        {
            return (
            hex: hex,
            component: hex.HexComponent as HexComponent,
            edgeDir: edgeDir
            );
        }

        private (Dictionary<Adjacent, Dir> adjacentDirs, Dir lDir, Dir rDir, bool hasLeftBridge) getEdgeStitchProperties(HexComponent hexComponent, EdgeDir edgeDir)
        {
            (Dictionary<Adjacent, Dir> adjacentDirs, Dir lDir, Dir rDir, bool hasLeftBridge) properties;

            properties.adjacentDirs = HexUtils.ADJACENT_DIR_OF_EDGE_DIR[edgeDir];
            properties.lDir = properties.adjacentDirs[Adjacent.Left];
            properties.rDir = properties.adjacentDirs[Adjacent.Right];
            properties.hasLeftBridge = hexComponent.Bridges.ContainsKey(properties.lDir);

            return properties;
        }

        private void placeEdgeStitchOnBoard(IHexComponent hexComponent, EdgeDir edgeDir, int rotationCount, float yPos,
            StitchElevation stitchElevation,
            int planeVersion,
            int leftBridgeVersion,
            int leftPlaneVersion,
            int oppositeBridgeVersion,
            int rightPlaneVersion,
            int rightBridgeVersion,
            int version
        )
        {
            var edgeStitchPrefab = HexParts.GetEdgeStitch(
                stitchElevation,
                planeVersion,
                leftBridgeVersion,
                leftPlaneVersion,
                oppositeBridgeVersion,
                rightPlaneVersion,
                rightBridgeVersion,
                version
            );

            try
            {
                var helperGo = new GameObject(edgeDir.ToString() + "_helper");
                helperGo.transform.SetParent(hexComponent.Transform);
                helperGo.transform.localPosition = Vector3.zero;
                helperGo.transform.localEulerAngles = HexUtils.GetBridgeEdgeRotationByCoord(edgeDir);

                var go = Instantiate(edgeStitchPrefab.Model, helperGo.transform);
                go.transform.localPosition = HexUtils.EDGE_STITCH_POSITION[EdgeDir.N];
                go.transform.position = new Vector3(go.transform.position.x, yPos, go.transform.position.z);
                go.transform.localEulerAngles = HexUtils.GetEdgeStitchRotationByCoord(edgeDir, rotationCount);
                go.name = go.name.Replace("(Clone)", "");
            }
            catch (Exception e)
            {
                var name = EdgeStitch.CreateEdgeStitchName(stitchElevation, planeVersion, leftBridgeVersion, leftPlaneVersion, oppositeBridgeVersion, rightPlaneVersion, rightBridgeVersion, version);
                Debug.LogWarning("name: " + name + ", go: " + hexComponent.Transform.gameObject.name + " => " + e.Message);
            }
        }

        private void createFirstHex()
        {
            _currentBuilderHexId++;
            var hex = new Hex(_currentBuilderHexId, MiddleCoord, 1);

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

            var s = "";
            foreach (var combination in filteredCombinations)
            {
                s += "new int[3] { " + combination[0] + ", " + combination[1] + ", " + combination[2] + " }\n";
                //s += "" + combination[0] + ", " + combination[1] + ", " + combination[2];
            }
            Debug.Log("combinations: " + s);
        }
    }
}