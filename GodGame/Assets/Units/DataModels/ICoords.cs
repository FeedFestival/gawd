using UnityEngine;

namespace Game.UnitController
{
    public interface ICoords
    {
        public float GetDistance(ICoords other);
        public Vector2 Pos { get; set; }
    }
}
