using UnityEngine;

namespace Game.Shared.DataModels
{
    public interface ICoords
    {
        public float GetDistance(ICoords other);
        public Vector2 Pos { get; set; }
    }
}
