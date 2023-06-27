using Game.Shared.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.WorldBuilder
{
    [System.Serializable]
    public struct StitchElevation
    {
        public int Me;
        public int Left;
        public int Right;

        public StitchElevation(int m, int l, int r) { Me = m; Left = l; Right = r; }
        public StitchElevation(int[] elvs) { Me = elvs[0]; Left = elvs[1]; Right = elvs[2]; }

        public override string ToString()
        {
            return "[ " + Me + "," + Left + "," + Right + " ]";
        }

        public string ToSaveString()
        {
            return Me + "" + Left + "" + Right;
        }

        internal bool AreEqual(StitchElevation stitchElevation)
        {
            return Me == stitchElevation.Me && Left == stitchElevation.Left && Right == stitchElevation.Right;
        }

        public static bool AreCombinationsEqual(int[] comb1, int[] comb2)
        {
            return comb1[0] == comb2[0] && comb1[1] == comb2[1] && comb1[2] == comb2[2];
        }

        internal static int GetRotation(int[] combination, int[] default_combination)
        {
            if (AreCombinationsEqual(combination, default_combination)) { return 0; }

            int r;
            for (r = 0; r < combination.Length; r++)
            {
                var aux0 = combination[0];
                combination[0] = combination[2];
                var aux1 = combination[1];
                combination[1] = aux0;
                combination[2] = aux1;

                if (AreCombinationsEqual(combination, default_combination))
                {
                    break;
                }
            }
            return r + 1;
        }

        internal static DirWay GetMinIndex(int[] arr)
        {
            int minIndex = 0;
            int minValue = arr[0];

            for (int i = 1; i < arr.Length; i++)
            {
                if (arr[i] < minValue)
                {
                    minValue = arr[i];
                    minIndex = i;
                }
            }
            return (DirWay)minIndex;
        }

        internal static int[] GetDefaultCombination(int[] combination)
        {
            foreach (var def_comb in DEFAULT_COMBINATIONS)
            {
                if (
                   (def_comb[0] == combination[0] && def_comb[1] == combination[1] && def_comb[2] == combination[2])
                || (def_comb[0] == combination[2] && def_comb[1] == combination[0] && def_comb[2] == combination[1])
                || (def_comb[0] == combination[1] && def_comb[1] == combination[2] && def_comb[2] == combination[0])
                )
                {
                    return def_comb;
                }
            }
            return null;
        }

        public static readonly List<int[]> DEFAULT_COMBINATIONS = new List<int[]>()
        {
            new int[3] { 0, 0, 0 }, // ☑️
            new int[3] { 0, 0, 1 }, // ☑️
            new int[3] { 0, 0, 2 }, // 
            new int[3] { 0, 0, 3 },
            new int[3] { 0, 0, 4 },
            new int[3] { 0, 1, 1 }, // ☑️
            new int[3] { 0, 1, 2 }, // 
            new int[3] { 0, 1, 3 },
            new int[3] { 0, 1, 4 },
            new int[3] { 0, 2, 1 }, // 
            new int[3] { 0, 2, 2 }, // 
            new int[3] { 0, 2, 3 },
            new int[3] { 0, 2, 4 },
            new int[3] { 0, 3, 1 },
            new int[3] { 0, 3, 2 },
            new int[3] { 0, 3, 3 },
            new int[3] { 0, 3, 4 },
            new int[3] { 0, 4, 1 },
            new int[3] { 0, 4, 2 },
            new int[3] { 0, 4, 3 },
            new int[3] { 0, 4, 4 }
        };
    }

    [System.Serializable]
    public struct EdgeStitch
    {
        public StitchElevation StitchElevation;
        public int PlaneVersion;
        public int LeftBridgeVersion;
        public int LeftPlaneVersion;
        public int OppositeBridgeVersion;
        public int RightPlaneVersion;
        public int RightBridgeVersion;
        public int Version;

        public GameObject Model;

        public EdgeStitch(GameObject model)
        {
            var props = model.name.Split('.');

            StitchElevation = GetStitchElevationFromString(props[(int)PART.COMBINATION]);
            PlaneVersion = GetNumberFromString(props[(int)PART.PLANE_VERSION], 1);
            LeftBridgeVersion = GetNumberFromString(props[(int)PART.LEFT_BRIDGE_VERSION], 3);
            LeftPlaneVersion = GetNumberFromString(props[(int)PART.LEFT_PLANE_VERSION], 3);
            OppositeBridgeVersion = GetNumberFromString(props[(int)PART.OPPOSITE_BRIDGE_VERSION], 3);
            RightPlaneVersion = GetNumberFromString(props[(int)PART.RIGHT_PLANE_VERSION], 3);
            RightBridgeVersion = GetNumberFromString(props[(int)PART.RIGHT_BRIDGE_VERSION], 3);
            Version = GetNumberFromString(props[(int)PART.VERSION]);

            Model = model;
        }

        public static string CreateEdgeStitchName(EdgeStitch edgeStitch)
        {
            return CreateEdgeStitchName(edgeStitch.StitchElevation, edgeStitch.PlaneVersion, edgeStitch.LeftBridgeVersion, edgeStitch.LeftPlaneVersion,
                edgeStitch.OppositeBridgeVersion, edgeStitch.RightPlaneVersion, edgeStitch.RightBridgeVersion, edgeStitch.Version);
        }

        public static string CreateEdgeStitchName(StitchElevation stitchElevation, int planeVersion, int leftBridgeVersion, int leftPlaneVersion,
            int oppositeBridgeVersion, int rightPlaneVersion, int rightBridgeVersion, int version)
        {
            return stitchElevation.ToSaveString()
                + ".EdgeStitch"
                + ".P" + planeVersion
                + ".L_B" + leftBridgeVersion
                + ".L_P" + leftPlaneVersion
                + ".O_B" + oppositeBridgeVersion
                + ".R_P" + rightPlaneVersion
                + ".R_B" + rightBridgeVersion
                + "." + version;
        }

        private static StitchElevation GetStitchElevationFromString(string s)
        {
            var m = int.Parse(s[0].ToString());
            var l = int.Parse(s[1].ToString());
            var r = int.Parse(s[2].ToString());
            return new StitchElevation(m, l, r);
        }

        public override string ToString()
        {
            return "{ "
                + "\n StitchElevation: " + StitchElevation
                + ",\n PlaneVersion: " + PlaneVersion
                + ",\n LeftBridgeVersion: " + LeftBridgeVersion
                + ",\n LeftPlaneVersion: " + LeftPlaneVersion
                + ",\n OppositeBridgeVersion: " + OppositeBridgeVersion
                + ",\n RightPlaneVersion: " + RightPlaneVersion
                + ",\n RightBridgeVersion: " + RightBridgeVersion
                + ",\n Version: " + Version
                + "}";
        }

        private static int GetNumberFromString(string s, int offset = 0)
        {
            return int.Parse((offset > 0 ? s.Substring(offset) : s));
        }

        // 000.EdgeStitch.P0.L_B0.L_P0.O_B0.R_P0 .R_B0.0
        enum PART
        {
            COMBINATION = 0,
            NAME = 1,
            PLANE_VERSION = 2,
            LEFT_BRIDGE_VERSION = 3,
            LEFT_PLANE_VERSION = 4,
            OPPOSITE_BRIDGE_VERSION = 5,
            RIGHT_PLANE_VERSION = 6,
            RIGHT_BRIDGE_VERSION = 7,
            VERSION = 8
        }
    }
}