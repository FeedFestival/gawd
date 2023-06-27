using Game.Shared.Interfaces;
using Game.Shared.Utils;
using System;
using UnityEngine;

namespace Game.UnitController
{
    public class UnitComponent : MonoBehaviour
    {
        public int MaxEnergy = 3;
        [HideInInspector]
        public int CurrentEnergy;

        public int VisionRange { get { return MaxEnergy + 1; } }

        public ICoord Coord;

        internal void Init()
        {
            CurrentEnergy = MaxEnergy;
        }

        public void StartTurn()
        {
            CurrentEnergy = MaxEnergy;
        }

        public bool UseEnergy(int energy)
        {
            if (energy > CurrentEnergy) { return false; }
            CurrentEnergy -= energy;
            return true;
        }

        internal void SetCoord(ICoord middleCoord)
        {
            Coord = middleCoord;
            transform.position = HexUtils.GetHexPosition(Coord);
        }
    }
}
