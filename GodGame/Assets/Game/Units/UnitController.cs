using DG.Tweening;
using Game.Shared.Interfaces;
using Game.Shared.Structs;
using Game.Shared.Utils;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Game.UnitController
{
    public class UnitController : MonoBehaviour
    {
        [SerializeField]
        private UnitControllerSettingsSO _unitControllerSettings;
        //[Header("Props")]
        private UnitComponent _currentUnit;
        private CameraController _cameraController;
        private Subject<int> _showAvailablePathSubject__s = new Subject<int>();

        private Pathfinding _pathfinding;

        private List<GameObject> _pathDots;
        private int _requestPathY;
        private int _requestPathX;

        private Subject<int> _moveSubject__s = new Subject<int>();

        private const float MOVE_DURATION = 0.3f;
        public static Coord COORD_ZERO = new Coord(0, 0);

        private void Awake()
        {
            _moveSubject__s
                .Do((int i) =>
                {
                    var pos = HexUtils.GetHexPosition(_pathfinding.Path[i]);
                    _currentUnit.transform.DOMove(pos, MOVE_DURATION).SetEase(Ease.Linear);
                })
                .Delay(TimeSpan.FromSeconds(MOVE_DURATION))
                .Subscribe((int i) =>
                {
                    if (i == _pathfinding.Path.Count - 1)
                    {
                        _currentUnit.Coord = _pathfinding.To;

                        ContinueTurn();
                        return;
                    }
                    int newIndex = i + 1;
                    _moveSubject__s.OnNext(newIndex);
                });

            _showAvailablePathSubject__s
                .Select((int _currentEnergy) =>
                {
                    var moveHexesMaxMoves = HexUtils.GetRangeByHexCount(_pathfinding.MoveHexes.Count());
                    var isEnough = moveHexesMaxMoves == _currentEnergy;
                    if (isEnough == false)
                    {
                        _pathfinding.ShouldSeeCountBasedOnEnergy = HexUtils.GetHexCountByRange(_currentEnergy);
                        if (moveHexesMaxMoves == 0)
                        {
                            createMoveHexes(_pathfinding.ShouldSeeCountBasedOnEnergy);
                            return true;
                        }
                        else
                        {
                            var currentHexCount = HexUtils.GetHexCountByRange(moveHexesMaxMoves);
                            Debug.Log("currentHexCount: " + currentHexCount);

                            if (_pathfinding.ShouldSeeCountBasedOnEnergy > currentHexCount)
                            {
                                var shouldAddCount = _pathfinding.ShouldSeeCountBasedOnEnergy - currentHexCount;
                                Debug.Log("shouldAddCount: " + shouldAddCount);

                                // add more hexes using _pathfinding.MoveHexesEdge
                            }
                            else
                            {
                                _pathfinding.disableExtraHexes(currentHexCount);
                            }
                        }
                    }
                    return false;
                })
                //.DelayFrame(10)
                .Do((bool createdAnew) =>
                {
                    if (createdAnew) { return; }
                    moveHexesToUnit();
                })
                .DelayFrame(10)
                .Do((bool createdAnew) =>
                {
                    _pathfinding.ShowAvailableMoveHexes();
                })
                .Subscribe();
        }

        public void PreSetup()
        {
            _pathfinding = new Pathfinding();

            _cameraController = Camera.main.GetComponent<CameraController>();
        }

        public void Init()
        {
            createCenterMoveHex();
        }

        public void StartTurn(UnitComponent unitComponent)
        {
            _currentUnit = unitComponent;
            _pathfinding.ResetPathfindingVariables();
            _cameraController.FocusOnUnit(_currentUnit.transform.position);

            if (_pathfinding.AllHexes[0].IsInitialized == false)
            {
                _pathfinding.AllHexes[0].IsInitialized = true;
                _pathfinding.AllHexes[0].Transform.position = _currentUnit.transform.position;
            }

            _showAvailablePathSubject__s.OnNext(_currentUnit.CurrentEnergy);
        }

        internal void ContinueTurn()
        {
            if (_currentUnit.CurrentEnergy == 0)
            {
                _pathfinding.HideHexPath();
                return;
            }

            _pathfinding.ShowHexPath();
            _showAvailablePathSubject__s.OnNext(_currentUnit.CurrentEnergy);
        }

        private void requestPath(int y, int x)
        {
            if (_requestPathY == y && _requestPathX == x) { return; }

            _requestPathY = y;
            _requestPathX = x;
            var path = Pathfinding.FindPath(_pathfinding.MoveHexes[0][0], _pathfinding.MoveHexes[y][x], ref _pathfinding.MoveHexes);

            preparePathDots(path.Count);
            showPath(path);
        }

        private void confirmDestination(int y, int x)
        {
            var path = Pathfinding.FindPath(_pathfinding.MoveHexes[0][0], _pathfinding.MoveHexes[y][x], ref _pathfinding.MoveHexes);
            foreach (var p in path)
            {
                Debug.Log(p);
            }
            Debug.Log("--------------------: ");
            path = _pathfinding.FormatPath(path, _currentUnit.Coord);
            foreach (var p in path)
            {
                Debug.Log(p);
            }
            Debug.Log("--------------------: ");
            _pathfinding.ApplyPathToWorld(path);

            hidePathDots();
            _pathfinding.HideHexPath();

            _currentUnit.UseEnergy(_pathfinding.Path.Count);

            _moveSubject__s.OnNext(0);
        }

        private void showPath(List<ICoord> path)
        {
            for (int i = 0; i < path.Count; i++)
            {
                _pathDots[i].transform.position = getPathfindingPos(path[i], 1);
            }
        }

        private void createCenterMoveHex()
        {
            var moveHex = createMoveHex(0, 0);
            moveHex.transform.position = HexUtils.GetHexPosition(COORD_ZERO);

            _pathfinding.AllHexes.Add(moveHex);
            _pathfinding.MoveHexes.AddMoveHex(moveHex);
            _pathfinding.CurBuildCoord = COORD_ZERO;
        }

        private void createMoveHexes(int hexCount)
        {
            for (int i = 0; i < hexCount - 1; i++)
            {
                var curMoveHex = _pathfinding.MoveHexes.GetAtCoord(_pathfinding.CurBuildCoord);

                var hasAllNeighbors = curMoveHex.Neighbors.Count == 6;
                if (hasAllNeighbors)
                {
                    _pathfinding.CurBuildCoord = _pathfinding.MoveHexesEdge.GetEdgeCoord(ref _pathfinding.MoveHexes);
                    i--;
                    continue;
                }

                var dir = curMoveHex.Neighbors.GetMissingNeighbor();

                var isOddRow = HexUtils.IsOddRow(curMoveHex.Y);
                var offset = HexUtils.GetCoordOffset(isOddRow, dir);
                var newNeighborCoord = Coord.AddTogheter(offset, _pathfinding.CurBuildCoord);

                var moveHex = createMoveHex(newNeighborCoord);
                moveHex.RequestPath += requestPath;
                moveHex.ConfirmDestination += confirmDestination;
                moveHex.transform.position = getPathfindingPos(newNeighborCoord);

                _pathfinding.AllHexes.Add(moveHex);
                _pathfinding.MoveHexes.AddMoveHex(moveHex);
                _pathfinding.MoveHexesEdge.Enqueue(newNeighborCoord);

                curMoveHex.Neighbors.Add(dir, newNeighborCoord);
                _pathfinding.MoveHexes[newNeighborCoord.Y][newNeighborCoord.X].Neighbors
                    .AddMultipleNeighbors(_pathfinding.CurBuildCoord, dir, ref _pathfinding.MoveHexes);
            }
        }

        private void moveHexesToUnit()
        {
            Vector3 oldPos = Vector3.zero, newPos = Vector3.zero;

            var isNewTurn = _pathfinding.From == null;
            if (isNewTurn)
            {
                oldPos = _pathfinding.AllHexes[0].Transform.position;
                newPos = HexUtils.GetHexPosition(_currentUnit.Coord);
            }
            else
            {
                oldPos = HexUtils.GetHexPosition(_pathfinding.From);
                newPos = HexUtils.GetHexPosition(_pathfinding.To);
            }
            var offset = newPos - oldPos;
            Debug.Log("offset: " + offset);

            foreach (var hex in _pathfinding.AllHexes)
            {
                hex.Transform.position = hex.Transform.position + offset;
            }
        }

        private MoveHex createMoveHex(ICoord coord)
        {
            return createMoveHex(coord.Y, coord.X);
        }

        private MoveHex createMoveHex(int y, int x)
        {
            var go = Instantiate(_unitControllerSettings.HexFadedPrefab, transform);
            var moveHex = go.GetComponent<MoveHex>();

            moveHex.Init(y, x);

            return moveHex;
        }

        

        private void preparePathDots(int count)
        {
            if (_pathDots == null)
            {
                _pathDots = new List<GameObject>();
            }

            var isEnough = _pathDots.Count >= count;
            if (isEnough == false)
            {
                var toAddCount = count - _pathDots.Count;
                for (int i = 0; i < toAddCount; i++)
                {
                    var go = Instantiate(_unitControllerSettings.PathDotPrefab);
                    _pathDots.Add(go);
                }
            }

            for (int i = 0; i < _pathDots.Count; i++)
            {
                if (i < count)
                {
                    _pathDots[i].SetActive(true);
                }
                else
                {
                    _pathDots[i].SetActive(false);
                }
            }
        }

        private void hidePathDots()
        {
            for (int i = 0; i < _pathDots.Count; i++)
            {
                _pathDots[i].SetActive(false);
            }
        }

        private Vector3 getPathfindingPos(ICoord coord, int elevation = 0)
        {
            return HexUtils.GetHexPosition(_currentUnit.Coord) + HexUtils.GetHexPosition(coord, elevation);
        }
    }
}
