using System;
using UnityEngine;

namespace Game.TurnController
{
    public class TurnController : MonoBehaviour
    {
        public Action NextTurnCallback;

        public void NextTurnClick()
        {
            NextTurnCallback?.Invoke();
        }
    }
}
