using Game.Shared.Enums;
using TMPro;
using UnityEngine;

namespace Game.WorldBuilder
{
    public class HexDebug : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _idTMP;
        [SerializeField]
        private TMP_Text _coordTMP;
        [SerializeField]
        private TMP_Text _NEidTMP;
        [SerializeField]
        private TMP_Text _EidTMP;
        [SerializeField]
        private TMP_Text _SEidTMP;
        [SerializeField]
        private TMP_Text _SWidTMP;
        [SerializeField]
        private TMP_Text _WidTMP;
        [SerializeField]
        private TMP_Text _NWidTMP;

        internal void SetId(int id)
        {
            _idTMP.text = id.ToString();
        }

        internal string GetId()
        {
            return _idTMP.text;
        }

        internal void SetCoord(int y, int x)
        {
            _coordTMP.text = y + "_" + x;
        }

        internal void SetNeighbor(Dir coordDir, int id)
        {
            switch (coordDir)
            {
                case Dir.NE:
                    _NEidTMP.text = id.ToString();
                    break;
                case Dir.E:
                    _EidTMP.text = id.ToString();
                    break;
                case Dir.SE:
                    _SEidTMP.text = id.ToString();
                    break;
                case Dir.SW:
                    _SWidTMP.text = id.ToString();
                    break;
                case Dir.W:
                    _WidTMP.text = id.ToString();
                    break;
                case Dir.NW:
                default:
                    _NWidTMP.text = id.ToString();
                    break;
            }
        }
    }
}
