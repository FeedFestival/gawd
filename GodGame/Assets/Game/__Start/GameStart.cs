using UnityEngine;
using UniRx;
using System;
using Game.WorldBuilder;    // rename to World
using Game.UnitController;
using Game.TurnController;

namespace Game.Start
{
    public class GameStart : MonoBehaviour
    {
        [SerializeField]
        private TurnController.TurnController _turnController;
        [SerializeField]
        private WorldBuilder.WorldBuilder _worldBuilder;
        [SerializeField]
        private UnitController.UnitController _unitController;
        [SerializeField]
        private UnitsManager _unitsManager;

        private Subject<bool> _startSubject__s = new Subject<bool>();

        private void Awake()
        {
            _startSubject__s
                .Delay(TimeSpan.FromMilliseconds(100))
                .Do((bool _) =>
                {
                // _unitsManager.PreSetup();
                _turnController.NextTurnCallback += nextTurn;
                    _worldBuilder.PreSetup();
                    _unitController.PreSetup();
                })
                .Delay(TimeSpan.FromMilliseconds(100))
                .Do((bool _) =>
                {
                    _unitsManager.Init(_worldBuilder.MiddleCoord);
                    _worldBuilder.Init();
                    _unitController.Init();
                })
                .Delay(TimeSpan.FromMilliseconds(100))
                .Do((bool _) =>
                {
                    var unit = _unitsManager.GetUnitTurn();
                    _worldBuilder.DiscoverWorld(unit.VisionRange);
                    _unitController.StartTurn(unit);
                })
                .Subscribe();
        }

        private void nextTurn()
        {
            var unit = _unitsManager.GetUnitTurn();
            unit.StartTurn();
            //_worldBuilder.discoverWorld(unit.MaxEnergy);
            _unitController.StartTurn(unit);
        }

        private void Start()
        {
            _startSubject__s.OnNext(false);
        }
    }
}
