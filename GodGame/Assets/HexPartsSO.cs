using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/HexParts", order = 1)]
public class HexPartsSO : ScriptableObject
{
    public GameObject BasicHex;

    [System.Serializable]
    public struct HexPlane
    {
        public int Elevation;
        public int Version;
        public GameObject PlaneModel;
    }
    public List<HexPlane> HexPlanes;

    [System.Serializable]
    public struct Bridge
    {
        public SlopeDir SlopeDir;
        public int Elevation;
        public int Version;
        public GameObject BridgeModel;
    }
    public List<Bridge> Bridges;

    [System.Serializable]
    public struct EdgeSide
    {
        public SlopeDir SlopeDir;
        public int Elevation;
        public int Version;
    }

    [System.Serializable]
    public struct BridgeEdge
    {
        public EdgeSide LeftSide;
        public EdgeSide RightSide;
        public GameObject BridgeEdgeModel;
    }
    public List<BridgeEdge> BridgeEdges;

    public Bridge GetBridge(SlopeDir slopeDir, int elevation, int version)
    {
        return Bridges.Find(b => b.SlopeDir == slopeDir && b.Elevation == elevation && b.Version == version);
    }

    public Bridge GetBridge(HexRenderer.RenderSettings settings)
    {
        return GetBridge(settings.SlopeDir, settings.BridgeToElevation, settings.Version);
    }

    public BridgeEdge GetBridgeEdge(
        SlopeDir leftSlopeDir, int leftElevation, int leftVersion,
        SlopeDir rightSlopeDir, int rightElevation, int rightVersion
    )
    {
        return BridgeEdges.Find(be =>
            be.LeftSide.SlopeDir == leftSlopeDir && be.LeftSide.Elevation == leftElevation && be.LeftSide.Version == leftVersion
            && be.RightSide.SlopeDir == rightSlopeDir && be.RightSide.Elevation == rightElevation && be.RightSide.Version == rightVersion
        );
    }

    public HexPlane GetHexPlanes(int elevation, int version)
    {
        return HexPlanes.Find(b => b.Elevation == elevation && b.Version == version);
    }
}
