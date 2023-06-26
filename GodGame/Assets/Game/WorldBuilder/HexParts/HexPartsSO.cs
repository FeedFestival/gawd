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
        public List<GameObject> BridgeEdgesGos;
        [HideInInspector]
        public List<BridgeEdge> BridgeEdges;

        public void Init()
        {
            BridgeEdges = new List<BridgeEdge>();

            foreach (var beGo in BridgeEdgesGos)
            {
                Debug.Log("beGo.name: " + beGo.name);

                var bridgeEdge = new BridgeEdge(beGo);
                Debug.Log("bridgeEdge: " + bridgeEdge.ToString());
                BridgeEdges.Add(bridgeEdge);
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

        public BridgeEdge GetBridgeEdge(
            Adjacent adjacent,
            SlopeDir slopeDir, int elevation,
            int bridgeVersion,
            int planeVersion,
            SlopeDir otherEdge_SlopeDir, int otherEdge_Elevation,
            int otherBridgeEdge_Version
        )
        {
            return BridgeEdges.Find(be =>
                be.Adjacent == adjacent
                && be.SlopeDir == slopeDir && be.Elevation == elevation
                && be.BridgeVersion == bridgeVersion
                && be.PlaneVersion == planeVersion
                && be.OtherEdge_SlopeDir == otherEdge_SlopeDir && be.OtherEdge_Elevation == otherEdge_Elevation
                && be.OtherBridgeEdge_Version == otherBridgeEdge_Version
                && be.Version == 0
            );
        }
    }
}
