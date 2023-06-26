using Game.Shared.DataModels;
using Game.Shared.Enums;
using Game.Shared.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UnitController
{
    public class ControllerPath
    {
        public List<MoveHex> AllHexes;
        public Dictionary<int, Dictionary<int, MoveHex>> MoveHexes;
        public Hex.Coord CurBuildCoord;
        public Queue<Hex.Coord> MoveHexesEdge;

        public List<Hex.Coord> Path;
        public ICoord From;
        public ICoord To;

        public int ShouldSeeCountBasedOnEnergy { get; internal set; }

        public ControllerPath()
        {
            AllHexes = new List<MoveHex>();
            MoveHexes = new Dictionary<int, Dictionary<int, MoveHex>>();
            MoveHexesEdge = new Queue<Hex.Coord>();
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