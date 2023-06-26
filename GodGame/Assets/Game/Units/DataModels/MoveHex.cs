using Game.Shared.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UnitController
{
    public class MoveHex : MonoBehaviour//, IHexCoord
    {
        public int Y { get; set; }
        public int X { get; set; }
        public Dictionary<Dir, ICoord> Neighbors;
        // Center Move Hex
        public bool IsInitialized;

        //-------------------
        public MoveHex Connection { get; private set; }
        public float G { get; private set; }
        public float H { get; private set; }
        public float F => G + H;
        public ICoords Coords;
        public float GetDistance(MoveHex other) => Coords.GetDistance(other.Coords); // Helper to reduce noise in pathfinding
        public bool Walkable { get; private set; }
        public bool Available { get; internal set; }

        public Action<int, int> RequestPath;
        public Action<int, int> ConfirmDestination;

        //------------------------------------

        public void Init(int y, int x)
        {
            Y = y;
            X = x;

            Coords = new HexCoords(x: X, y: Y);
            Walkable = true;
            Available = true;

            Neighbors = new Dictionary<Dir, ICoord>();

            // debug

            gameObject.name = "mhx_" + Y + "_" + X;
        }

        private void OnMouseOver()
        {
            RequestPath?.Invoke(Y, X);
        }

        private void OnMouseDown()
        {
            Debug.Log(gameObject.name + " - " + Y + ", " + X);
            ConfirmDestination?.Invoke(Y, X);
        }

        //
        public void SetConnection(MoveHex nodeBase)
        {
            Connection = nodeBase;
        }

        public void SetG(float g)
        {
            G = g;
            //SetText();
        }

        public void SetH(float h)
        {
            H = h;
            //SetText();
        }

        public void ResetPathfinding()
        {
            G = 0;
            H = 0;
            Available = true;
            Walkable = true;
            Coords.Pos = X * new Vector2(HexCoords.SQRT3, 0) + Y * new Vector2(HexCoords.SQRT3 / 2, 1.5f); ;
        }

        public void SetAvailable(bool available)
        {
            Available = available;
            if (Available == false)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
