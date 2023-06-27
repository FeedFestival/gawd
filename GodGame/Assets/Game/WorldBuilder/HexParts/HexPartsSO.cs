using Game.Shared.Enums;
using Game.Shared.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using static Game.Shared.Structs.HexPartsStructs;

namespace Game.WorldBuilder
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/HexParts", order = 1)]
    public class HexPartsSO : ScriptableObject
    {
        public GameObject BasicHex;

        public List<HexPlane> HexPlanes;
        public List<Bridge> Bridges;

        public List<GameObject> EdgeStitchGos;
        [HideInInspector]
        public List<EdgeStitch> EdgeStitches;

        public void Init()
        {
            EdgeStitches = new List<EdgeStitch>();
            foreach (var beGo in EdgeStitchGos)
            {
                var edgeStitch = new EdgeStitch(beGo);
                Debug.Log(beGo.name + " = edgeStitch: " + edgeStitch.ToString());
                EdgeStitches.Add(edgeStitch);
            }
        }

        public HexPlane GetHexPlanes(int version)
        {
            return HexPlanes.Find(b => b.Version == version);
        }

        public Bridge GetBridge(SlopeDir slopeDir, int version, int elevation = 0)
        {
            return Bridges.Find(b => b.SlopeDir == slopeDir && b.Elevation == elevation && b.Version == version);
        }

        public Bridge GetBridge(Bridge bridge)
        {
            return GetBridge(bridge.SlopeDir, bridge.Version, bridge.Elevation);
        }

        public EdgeStitch GetEdgeStitch(
            StitchElevation stitchElevation,
            int planeVersion,
            int leftBridgeVersion,
            int leftPlaneVersion,
            int oppositeBridgeVersion,
            int rightPlaneVersion,
            int rightBridgeVersion,
            int version
        )
        {
            return EdgeStitches.Find(be =>
                be.StitchElevation.AreEqual(stitchElevation)
                && be.PlaneVersion == planeVersion
                && be.LeftBridgeVersion == leftBridgeVersion
                && be.LeftPlaneVersion == leftPlaneVersion
                && be.OppositeBridgeVersion == oppositeBridgeVersion
                && be.RightPlaneVersion == rightPlaneVersion
                && be.RightBridgeVersion == rightBridgeVersion
                && be.Version == version
            );
        }
    }
}
