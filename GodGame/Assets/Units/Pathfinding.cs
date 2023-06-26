using Game.Shared.DataModels;
using Game.Shared.Enums;
using Game.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.UnitController
{
    public class Pathfinding
    {
        public List<MoveHex> AllHexes;
        public Dictionary<int, Dictionary<int, MoveHex>> MoveHexes;
        public Hex.Coord CurBuildCoord;
        public Queue<Hex.Coord> MoveHexesEdge;

        public List<Hex.Coord> Path;
        public ICoord From;
        public ICoord To;

        public int ShouldSeeCountBasedOnEnergy { get; internal set; }

        public Pathfinding()
        {
            AllHexes = new List<MoveHex>();
            MoveHexes = new Dictionary<int, Dictionary<int, MoveHex>>();
            MoveHexesEdge = new Queue<Hex.Coord>();
        }

        /// <summary>
        /// This algorithm is written for readability. Although it would be perfectly fine in 80% of games, please
        /// don't use this in an RTS without first applying some optimization mentioned in the video: https://youtu.be/i0x5fj4PqP4
        /// If you enjoyed the explanation, be sure to subscribe!
        ///
        /// Also, setting colors and text on each hex affects performance, so removing that will also improve it marginally.
        /// </summary>
        public static List<Hex.Coord> FindPath(MoveHex startNode, MoveHex targetNode, ref Dictionary<int, Dictionary<int, MoveHex>> moveHexes)
        {
            var toSearch = new List<MoveHex>() { startNode };
            var processed = new List<MoveHex>();

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
                    var path = new List<Hex.Coord>();
                    var count = 100;
                    while (currentPathTile != startNode)
                    {
                        path.Add(new Hex.Coord(currentPathTile.Y, currentPathTile.X));
                        currentPathTile = currentPathTile.Connection;
                        count--;
                        if (count < 0) throw new Exception();
                    }
                    return path;
                }

                var moveHexNeighbors = new List<MoveHex>();
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

        internal void HideHexPath()
        {
            foreach (var hx in AllHexes)
            {
                hx.gameObject.SetActive(false);
            }
        }

        internal void ShowHexPath()
        {
            foreach (var hx in AllHexes)
            {
                hx.gameObject.SetActive(true);
            }
        }

        public List<Hex.Coord> FormatPath(List<Hex.Coord> path, ICoord startCoord)
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

        public List<Hex.Coord> ApplyPathToWorld(List<Hex.Coord> path)
        {
            if (Path == null) { Path = new List<Hex.Coord>(); }
            
            Path.Clear();
            foreach (var p in path)
            {
                Path.Add(
                    Hex.Coord.AddTogheter(From, p)
                );
            }
            To = Path[Path.Count - 1];

            return Path;
        }

        private List<Hex.Coord> transformEvenPathToOdd(List<Hex.Coord> path)
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
            var newPath = new List<Hex.Coord>();
            var decomposedPath = new List<(RowIs fromRow, Hex.Coord usedCoord, RowIs rowIs, Dir dir)>();
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
                var usedCoord = Hex.Coord.Minus(path[i], path[i - 1]);
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
                var newCoord = Hex.Coord.AddTogheter(newPath[i - 1], fromRow == RowIs.Even
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
                if (hex.Available) { hex.gameObject.SetActive(true); }
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
    }
}