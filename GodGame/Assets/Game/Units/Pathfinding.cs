using Game.Shared.Enums;
using Game.Shared.Interfaces;
using Game.Shared.Structs;
using Game.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.UnitController
{
    public class Pathfinding
    {
        public List<IMoveHex> AllHexes;
        public Dictionary<int, Dictionary<int, IMoveHex>> MoveHexes;
        public ICoord CurBuildCoord;
        public Queue<ICoord> OnEdgeHexes;

        public List<ICoord> Path;
        public ICoord From;
        public ICoord To;

        public int ShouldSeeCountBasedOnEnergy { get; internal set; }

        public Pathfinding()
        {
            AllHexes = new List<IMoveHex>();
            MoveHexes = new Dictionary<int, Dictionary<int, IMoveHex>>();
            OnEdgeHexes = new Queue<ICoord>();
        }

        /// <summary>
        /// This algorithm is written for readability. Although it would be perfectly fine in 80% of games, please
        /// don't use this in an RTS without first applying some optimization mentioned in the video: https://youtu.be/i0x5fj4PqP4
        /// If you enjoyed the explanation, be sure to subscribe!
        ///
        /// Also, setting colors and text on each hex affects performance, so removing that will also improve it marginally.
        /// </summary>
        public static List<ICoord> FindPath(IMoveHex startNode, IMoveHex targetNode, ref Dictionary<int, Dictionary<int, IMoveHex>> moveHexes)
        {
            var toSearch = new List<IMoveHex>() { startNode };
            var processed = new List<IMoveHex>();

            while (toSearch.Any())
            {

                var current = toSearch[0];
                foreach (var t in toSearch)
                {
                    if (t.F < current.F || t.F == current.F && t.H < current.H)
                    {
                        current = t;
                    }
                }

                processed.Add(current);
                toSearch.Remove(current);

                if (current == targetNode)
                {
                    var currentPathTile = targetNode;
                    var path = new List<ICoord>();
                    var count = 100;
                    while (currentPathTile != startNode)
                    {
                        path.Add(new Coord(currentPathTile.Y, currentPathTile.X));
                        currentPathTile = currentPathTile.Connection;
                        count--;
                        if (count < 0) throw new Exception();
                    }
                    return path;
                }

                var moveHexNeighbors = new List<IMoveHex>();
                foreach (var neighbor in current.Neighbors)
                {
                    var moveHexNeighbor = moveHexes[neighbor.Value.Y][neighbor.Value.X];
                    var isWalkable = moveHexNeighbor.Walkable;
                    var isNotProcessed = !processed.Contains(moveHexNeighbor);
                    if (isWalkable && isNotProcessed)
                    {
                        moveHexNeighbors.Add(moveHexNeighbor);
                    }
                }

                foreach (var neighbor in moveHexNeighbors)
                {
                    var inSearch = toSearch.Contains(neighbor);

                    var costToNeighbor = current.G + current.GetDistance(neighbor);

                    if (!inSearch || costToNeighbor < neighbor.G)
                    {
                        neighbor.SetG(costToNeighbor);
                        neighbor.SetConnection(current);

                        if (!inSearch)
                        {
                            neighbor.SetH(neighbor.GetDistance(targetNode));
                            toSearch.Add(neighbor);
                        }
                    }
                }
            }
            return null;
        }

        internal List<Vector3> GetEdgePoints()
        {
            var allPoints = new List<Vector3>();
            foreach (var onEdgeHex in OnEdgeHexes)
            {
                foreach (var dir in HexUtils.DIRECTIONS)
                {
                    var hasNeighbor = MoveHexes[onEdgeHex.Y][onEdgeHex.X].Neighbors.ContainsKey(dir);
                    var edgeDirList = new List<EdgeDir>();
                    if (!hasNeighbor)
                    {
                        var edgeDirs = HexUtils.ADJACENT_EDGE_DIR_OF_DIR[dir];
                        foreach (var edgeDir in edgeDirs)
                        {
                            if (!edgeDirList.Contains(edgeDir))
                            {
                                var pos = MoveHexes[onEdgeHex.Y][onEdgeHex.X].Transform.position + HexUtils.EDGE_STITCH_POSITION[edgeDir];
                                allPoints.Add(pos);
                            }
                        }
                    }
                }
            }
            allPoints = VectorUtils.RemoveClosePoints(allPoints, 0.1f);

            return VectorUtils.SortPointsCounterClockwise(allPoints);
            //return VectorUtils.SortPoints(allPoints);
        }

        internal void HideHexPath()
        {
            foreach (var hx in AllHexes)
            {
                hx.Transform.gameObject.SetActive(false);
            }
        }

        internal void ShowHexPath()
        {
            foreach (var hx in AllHexes)
            {
                hx.Transform.gameObject.SetActive(true);
            }
        }

        public List<ICoord> FormatPath(List<ICoord> path, ICoord startCoord)
        {
            From = startCoord;
            path.Reverse();
            var isOddRow = HexUtils.IsOddRow(From.Y);

            if (isOddRow)
            {
                path = transformEvenPathToOdd(path);
            }
            return path;
        }

        public List<ICoord> ApplyPathToWorld(List<ICoord> path)
        {
            if (Path == null) { Path = new List<ICoord>(); }

            Path.Clear();
            foreach (var p in path)
            {
                Path.Add(
                    Coord.AddTogheter(From, p)
                );
            }
            To = Path[Path.Count - 1];

            return Path;
        }

        private List<ICoord> transformEvenPathToOdd(List<ICoord> path)
        {
            Dir dir;
            if (path.Count == 1)
            {
                dir = HexUtils.EVEN_OFFSET_TO_DIR[path[0]];
                var coord = HexUtils.COORD_ODD__OFFSET[dir];
                path[0] = coord;
                return path;
            }

            RowIs rowIs;
            var newPath = new List<ICoord>();
            var decomposedPath = new List<(RowIs fromRow, ICoord usedCoord, RowIs rowIs, Dir dir)>();
            for (int i = 0; i < path.Count; i++)
            {
                if (i == 0)
                {
                    rowIs = path[0].Y != 0 ? HexUtils.Opposite(RowIs.Even) : RowIs.Even;
                    dir = HexUtils.EVEN_OFFSET_TO_DIR[path[0]];
                    decomposedPath.Add((
                        fromRow: RowIs.Even,
                        usedCoord: path[0],
                        rowIs: rowIs,
                        dir: dir
                    ));
                    newPath.Add(HexUtils.COORD_ODD__OFFSET[dir]);
                    continue;
                }

                var fromRow = decomposedPath[i - 1].rowIs;
                var usedCoord = Coord.Difference(path[i], path[i - 1]);
                rowIs = usedCoord.Y == 0
                    ? fromRow
                    : HexUtils.Opposite(fromRow);
                dir = fromRow == RowIs.Even
                    ? HexUtils.EVEN_OFFSET_TO_DIR[usedCoord]
                    : HexUtils.ODD__OFFSET_TO_DIR[usedCoord];
                decomposedPath.Add((
                    fromRow: fromRow,
                    usedCoord: usedCoord,
                    rowIs: rowIs,
                    dir: dir
                ));
                var newCoord = Coord.AddTogheter(newPath[i - 1], fromRow == RowIs.Even
                    ? HexUtils.COORD_ODD__OFFSET[dir]
                    : HexUtils.COORD_EVEN_OFFSET[dir]
                );
                newPath.Add(newCoord);
            }

            return newPath;
        }

        internal void ShowAvailableMoveHexes()
        {
            foreach (var hex in AllHexes)
            {
                if (hex.Available) { hex.Transform.gameObject.SetActive(true); }
            }
        }

        internal void disableExtraHexes(int currentHexCount)
        {
            for (int i = currentHexCount - 1; i >= ShouldSeeCountBasedOnEnergy; i--)
            {
                AllHexes[i].SetAvailable(false);
            }
        }

        public void ResetPathfindingVariables()
        {
            From = null;
            // We don't reset To, as we need it when we switch to another unit
            Path = null;
            foreach (var hex in AllHexes)
            {
                hex.ResetPathfinding();
            }
        }

        public Mesh CreateMesh(List<Vector3> vertices)
        {
            return Triangulator.CreateConcaveHullMesh(vertices);
        }
    }
}