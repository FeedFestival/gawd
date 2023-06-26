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

        [System.Serializable]
        public struct BridgeEdge
        {
            public Adjacent Adjacent;

            public SlopeDir SlopeDir;
            public int Elevation;

            public int BridgeVersion;

            public SlopeDir OtherEdge_SlopeDir;
            public int OtherEdge_Elevation;

            public int OtherBridgeEdge_Version;

            public int PlaneVersion;
            public int Version;

            public GameObject Model;

            enum BEP
            {
                ADJACENT = 0,
                NAME = 1,
                SLOPE_ELEVATION = 2,
                BRIDGE_VERSION = 3,
                PLANE_VERSION = 4,
                OTHER_SLOPE_ELEVATION = 5,
                OTHER_BRIDGE_VERSION = 6,
                VERSION = 7
            }

            public BridgeEdge(GameObject model)
            {
                var props = model.name.Split('.');

                Adjacent = props[(int)BEP.ADJACENT] == "L" ? Adjacent.Left : Adjacent.Right;
                SlopeDir = GetSlopeFromString(props[(int)BEP.SLOPE_ELEVATION],
                    out Elevation);
                BridgeVersion = GetBridgeVersionFromString(props[(int)BEP.BRIDGE_VERSION]);
                PlaneVersion = GetPlaneVersionFromString(props[(int)BEP.PLANE_VERSION]);
                OtherEdge_SlopeDir = GetOtherSlopeFromString(props[(int)BEP.OTHER_SLOPE_ELEVATION],
                    out OtherEdge_Elevation);
                OtherBridgeEdge_Version = GetOtherBridgeEdgeVersionFromString(props[(int)BEP.OTHER_BRIDGE_VERSION]);
                Version = GetNumberFromString(props[(int)BEP.VERSION]);

                Model = model;
            }

            public static string CreateBridgeEdgeName(
                Adjacent adjacent,
                SlopeDir bridgeSlopeDir, int bridgeElevation,
                int bridgeVersion,
                int planeVersion,
                SlopeDir otherBridgeSlopeDir, int otherBridgeElevation,
                int otherBridgeVersion,
                int version
            )
            {
                var slope = GetStringFromSlope(bridgeSlopeDir);
                var otherSlope = GetStringFromSlope(otherBridgeSlopeDir);
                // R.BridgeEdge.D1.B0.P0.O_D1.O_BE0.0
                return (adjacent == Adjacent.Left ? "L" : "R")
                    + ".BridgeEdge"
                    + "." + slope + bridgeElevation
                    + ".B" + bridgeVersion
                    + ".P" + planeVersion
                    + ".O_" + otherSlope + otherBridgeElevation
                    + ".O_BE" + otherBridgeVersion
                    + "." + version;
            }

            public override string ToString()
            {
                return "{ "
                    + "\n Adjacent: " + Adjacent
                    + ",\n SlopeDir: " + SlopeDir
                    + ",\n Elevation: " + Elevation
                    + ",\n BridgeVersion: " + BridgeVersion
                    + ",\n OtherEdge_SlopeDir: " + OtherEdge_SlopeDir
                    + ",\n OtherEdge_Elevation: " + OtherEdge_Elevation
                    + ",\n OtherBridgeEdge_Version: " + OtherBridgeEdge_Version
                    + ",\n PlaneVersion: " + PlaneVersion
                    + ",\n Version: " + Version
                    + "}";
            }

            public static SlopeDir GetSlopeFromString(string s, out int elevation)
            {
                switch (s[0])
                {
                    case 'A':
                        s = s.Substring(1);
                        elevation = int.Parse(s);
                        return SlopeDir.ASC;
                    case 'D':
                        s = s.Substring(1);
                        elevation = int.Parse(s);
                        return SlopeDir.DESC;
                    default:
                        elevation = 0;
                        return SlopeDir.LEVEL;
                }
            }

            public static int GetBridgeVersionFromString(string s)
            {
                return GetNumberFromString(s, 1);
            }

            public static int GetPlaneVersionFromString(string s)
            {
                return GetNumberFromString(s, 1);
            }

            public static SlopeDir GetOtherSlopeFromString(string s, out int elevation)
            {
                switch (s[2])
                {
                    case 'A':
                        s = s.Substring(3);
                        elevation = int.Parse(s);
                        return SlopeDir.ASC;
                    case 'D':
                        s = s.Substring(3);
                        elevation = int.Parse(s);
                        return SlopeDir.DESC;
                    default:
                        elevation = 0;
                        return SlopeDir.LEVEL;
                }
            }

            public static int GetOtherBridgeEdgeVersionFromString(string s)
            {
                return GetNumberFromString(s, 4);
            }

            private static int GetNumberFromString(string s, int offset = 0)
            {
                return int.Parse((offset > 0 ? s.Substring(offset) : s));
            }

            private static string GetStringFromSlope(SlopeDir slopeDir) {
                return slopeDir == SlopeDir.LEVEL
                    ? "L"
                    : slopeDir == SlopeDir.DESC ? "D" : "A";
            }
        }
    }
}
