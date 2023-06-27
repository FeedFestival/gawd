using Game.Shared.Enums;
using UnityEngine;

namespace Game.Shared.Structs
{
    public class HexPartsStructs : MonoBehaviour
    {
        [System.Serializable]
        public struct HexPlane
        {
            public int Version;
            public GameObject Model;
        }

        [System.Serializable]
        public struct Bridge
        {
            public SlopeDir SlopeDir;
            public int Elevation;
            public int Version;
            public GameObject Model;

            public string CreateModelName()
            {
                var slope = SlopeDir == SlopeDir.LEVEL
                    ? "L"
                    : SlopeDir == SlopeDir.DESC ? "D" : "A";
                return "Bridge." + slope + Elevation + "." + Version;
            }

            public override string ToString()
            {
                return "{ "
                    + " SlopeDir: " + SlopeDir
                    + ", Elevation: " + Elevation
                    + ", Version: " + Version
                    + "}";
            }
        }
    }
}
