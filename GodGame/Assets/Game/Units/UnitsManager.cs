using Game.Shared.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UnitController
{
    public class UnitsManager : MonoBehaviour
    {
        public UnitComponent GodUnit;

        public List<UnitComponent> Units;

        private void Awake()
        {
            Units = new List<UnitComponent>();
            Units.Add(GodUnit);
        }

        public UnitComponent GetUnitTurn()
        {
            return GodUnit;
        }

        public void Init(ICoord middleCoord)
        {
            GodUnit.Init();
            GodUnit.SetCoord(middleCoord);
        }
    }
}
