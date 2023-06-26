using DG.Tweening;
using Game.Shared.DataModels;
using Game.Shared.Utils;
using System;
using System.Collections.Generic;
using Tarodev_Pathfinding._Scripts;
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

        private ControllerPath _controllerPathRef;

        private List<GameObject> _pathDots;
        private int _requestPathY;
        private int _requestPathX;

        private Subject<int> _moveSubject__s = new Subject<int>();

        private const float MOVE_DURATION = 0.3f;
        public static Hex.Coord COORD_ZERO = new Hex.Coord(0, 0);

        private void Awake()
        {
            _moveSubject__s
                .Do((int i) =>
                {
                    var pos = HexUtils.GetHexPosition(_controllerPathRef.Path[i]);
                    _currentUnit.transform.DOMove(pos, MOVE_DURATION).SetEase(Ease.Linear);
                })
                .Delay(TimeSpan.FromSeconds(MOVE_DURATION))
                .Subscribe((int i) =>
                {
                    if (i == _controllerPathRef.Path.Count - 1)
                    {
                        _currentUnit.Coord = _controllerPathRef.To;

                        ContinueTurn();
                        return;
                    }
                    int newIndex = i + 1;
                    _moveSubject__s.OnNext(newIndex);
                });

            _showAvailablePathSubject__s
                .Select((int _currentEnergy) =>
                {
                    var moveHexesMaxMoves = HexUtils.GetRangeByHexCount(_controllerPathRef.MoveHexes.Count());
                    var isEnough = moveHexesMaxMoves == _currentEnergy;
                    if (isEnough == false)
                    {
                        _controllerPathRef.ShouldSeeCountBasedOnEnergy = HexUtils.GetHexCountByRange(_currentEnergy);
                        if (moveHexesMaxMoves == 0)
                        {
                            createMoveHexes(_controllerPathRef.ShouldSeeCountBasedOnEnergy);
                            return true;
                        }
                        else
                        {
                            var currentHexCount = HexUtils.GetHexCountByRange(moveHexesMaxMoves);
                            Debug.Log("currentHexCount: " + currentHexCount);

                            if (_controllerPathRef.ShouldSeeCountBasedOnEnergy > currentHexCount)
                            {
                                var shouldAddCount = _controllerPathRef.ShouldSeeCountBasedOnEnergy - currentHexCount;
                                Debug.Log("shouldAddCount: " + shouldAddCount);

                                // add more hexes using _controllerPathRef.MoveHexesEdge
                            }
                            else
                            {
                                _controllerPathRef.disableExtraHexes(currentHexCount);
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
                    _controllerPathRef.ShowAvailableMoveHexes();
                })
                .Subscribe();
        }

        public void PreSetup()
        {
            _controllerPathRef = new ControllerPath();

            _cameraController = Camera.main.GetComponent<CameraController>();
        }

        public void Init()
        {
            createCenterMoveHex();
        }

        public void StartTurn(UnitComponent unitComponent)
        {
            _currentUnit = unitComponent;
            _controllerPathRef.ResetPathfindingVariables();
            _cameraController.FocusOnUnit(_currentUnit.transform.position);

            if (_controllerPathRef.AllHexes[0].IsInitialized == false)
            {
                _controllerPathRef.AllHexes[0].IsInitialized = true;
                _controllerPathRef.AllHexes[0].transform.position = _currentUnit.transform.position;
            }

            _showAvailablePathSubject__s.OnNext(_currentUnit.CurrentEnergy);
        }

        internal void ContinueTurn()
        {
            if (_currentUnit.CurrentEnergy == 0)
            {
                _controllerPathRef.HideHexPath();
                return;
            }

            _controllerPathRef.ShowHexPath();
            _showAvailablePathSubject__s.OnNext(_currentUnit.CurrentEnergy);
        }

        private void requestPath(int y, int x)
        {
            if (_requestPathY == y && _requestPathX == x) { return; }

            _requestPathY = y;
            _requestPathX = x;
            var path = Pathfinding.FindPath(_controllerPathRef.MoveHexes[0][0], _controllerPathRef.MoveHexes[y][x], ref _controllerPathRef.MoveHexes);

            preparePathDots(path.Count);
            showPath(path);
        }

        private void confirmDestination(int y, int x)
        {
            var path = Pathfinding.FindPath(_controllerPathRef.MoveHexes[0][0], _controllerPathRef.MoveHexes[y][x], ref _controllerPathRef.MoveHexes);
            foreach (var p in path)
            {
                Debug.Log(p);
            }
            Debug.Log("--------------------: ");
            path = _controllerPathRef.FormatPath(path, _currentUnit.Coord);
            foreach (var p in path)
            {
                Debug.Log(p);
            }
            Debug.Log("--------------------: ");
            _controllerPathRef.ApplyPathToWorld(path);

            hidePathDots();
            _controllerPathRef.HideHexPath();

            _currentUnit.UseEnergy(_controllerPathRef.Path.Count);

            _moveSubject__s.OnNext(0);
        }

        private void showPath(List<Hex.Coord> path)
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

            _controllerPathRef.AllHexes.Add(moveHex);
            _controllerPathRef.MoveHexes.AddMoveHex(moveHex);
            _controllerPathRef.CurBuildCoord = COORD_ZERO;
        }

        private void createMoveHexes(int hexCount)
        {
            for (int i = 0; i < hexCount - 1; i++)
            {
                var curMoveHex = _controllerPathRef.MoveHexes.GetAtCoord(_controllerPathRef.CurBuildCoord);

                var hasAllNeighbors = curMoveHex.Neighbors.Count == 6;
                if (hasAllNeighbors)
                {
                    _controllerPathRef.CurBuildCoord = _controllerPathRef.MoveHexesEdge.GetEdgeCoord(ref _controllerPathRef.MoveHexes);
                    i--;
                    continue;
                }

                var dir = curMoveHex.Neighbors.GetMissingNeighbor();

                var isOddRow = HexUtils.IsOddRow(curMoveHex.Y);
                var offset = HexUtils.GetCoordOffset(isOddRow, dir);
                var newNeighborCoord = offset.Plus(_controllerPathRef.CurBuildCoord);

                var moveHex = createMoveHex(newNeighborCoord);
                moveHex.RequestPath += requestPath;
                moveHex.ConfirmDestination += confirmDestination;
                moveHex.transform.position = getPathfindingPos(newNeighborCoord);

                _controllerPathRef.AllHexes.Add(moveHex);
                _controllerPathRef.MoveHexes.AddMoveHex(moveHex);
                _controllerPathRef.MoveHexesEdge.Enqueue(newNeighborCoord);

                curMoveHex.Neighbors.Add(dir, newNeighborCoord);
                _controllerPathRef.MoveHexes[newNeighborCoord.Y][newNeighborCoord.X].Neighbors
                    .AddMultipleNeighbors(_controllerPathRef.CurBuildCoord, dir, ref _controllerPathRef.MoveHexes);
            }
        }

        private void moveHexesToUnit()
        {
            Vector3 oldPos = Vector3.zero, newPos = Vector3.zero;

            var isNewTurn = _controllerPathRef.From == null;
            if (isNewTurn)
            {
                oldPos = _controllerPathRef.AllHexes[0].transform.position;
                newPos = HexUtils.GetHexPosition(_currentUnit.Coord);
            }
            else
            {
                oldPos = HexUtils.GetHexPosition(_controllerPathRef.From);
                newPos = HexUtils.GetHexPosition(_controllerPathRef.To);
            }
            var offset = newPos - oldPos;
            Debug.Log("offset: " + offset);

            foreach (var hex in _controllerPathRef.AllHexes)
            {
                hex.transform.position = hex.transform.position + offset;
            }
        }

        private MoveHex createMoveHex(Hex.Coord coord)
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

        private Vector3 getPathfindingPos(Hex.Coord coord, int elevation = 0)
        {
            return HexUtils.GetHexPosition(_currentUnit.Coord) + HexUtils.GetHexPosition(coord, elevation);
        }
    }
}
