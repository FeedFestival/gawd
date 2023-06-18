using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexRenderer : MonoBehaviour
{
    public GameObject HexBasePlane;
    [SerializeField]
    internal HexDebug HexDebug;

    public int Elevation;
    public struct RenderSettings
    {
        public SlopeDir SlopeDir;
        public int BridgeToElevation;
        public int Version;

        public RenderSettings(SlopeDir slopeDir, int bridgeToElevation, int version)
        {
            SlopeDir = slopeDir;
            BridgeToElevation = bridgeToElevation;
            Version = version;
        }
    }

    public Dictionary<CoordDir, RenderSettings> BridgeSettings;
    public Dictionary<EdgeCoordDir, Dictionary<EdgeSide, RenderSettings>> EdgeSettings;

    internal void InitSettings(Hex hex)
    {
        HexDebug?.SetId(hex.ID);
        HexDebug?.SetCoord(hex.Y, hex.X);

        Elevation = hex.Elevation;
        BridgeSettings = new Dictionary<CoordDir, RenderSettings>();
        EdgeSettings = new Dictionary<EdgeCoordDir, Dictionary<EdgeSide, RenderSettings>>();
    }

    internal void SetHexVersion(int elevation, int version, ref HexPartsSO hexParts)
    {
        Destroy(HexBasePlane);
        var go = Instantiate(hexParts.GetHexPlanes(elevation, version).PlaneModel, transform);
        HexBasePlane = go;
    }

    internal void BuildBridges(CoordDir coordDir, Hex hexNeighbor, ref HexPartsSO hexParts)
    {
        if (BridgeSettings.ContainsKey(coordDir)) { return; }

        var slopeDir = GetSlopeDir(Elevation, hexNeighbor.Elevation);
        var bridgeToElevation = (slopeDir == SlopeDir.LEVEL)
            ? 0
            : (slopeDir == SlopeDir.ASC)
                ? hexNeighbor.Elevation - Elevation
                : Elevation - hexNeighbor.Elevation;
        // TODO: determine version somehow
        var version = 1;

        var settings = new RenderSettings(slopeDir, bridgeToElevation, version);
        BridgeSettings.Add(coordDir, settings);

        var hexBridge = hexParts.GetBridge(settings);
        var go = Instantiate(hexBridge.BridgeModel, transform);
        var rotation = GetBridgeRotationByCoord(coordDir);

        go.transform.eulerAngles = rotation;
    }

    public static SlopeDir GetSlopeDir(int elevation, int neighborElevation)
    {
        if (elevation > neighborElevation)
        {
            return SlopeDir.DESC;
        }
        else if (elevation < neighborElevation)
        {
            return SlopeDir.ASC;
        }
        else
        {
            return SlopeDir.LEVEL;
        }
    }

    public static Vector3 GetBridgeRotationByCoord(CoordDir coord)
    {
        return new Vector3(0, ((int)coord + 1) * 60f, 0);
    }

    public void buildEdge(EdgeSide edgeSide, CoordDir coordDir, EdgeCoordDir edgeCoordDirs, ref HexPartsSO hexParts)
    {
        if (EdgeSettings.ContainsKey(edgeCoordDirs) && EdgeSettings[edgeCoordDirs].ContainsKey(edgeSide)) { return; }

        var edgeCoordDir = edgeSide == EdgeSide.LEFT ? GetLeftCoordDir(coordDir) : GetRightCoordDir(coordDir);
        var exist = BridgeSettings.ContainsKey(edgeCoordDir);
        if (!exist) { return; }

        var sideBridgeSettings = BridgeSettings[edgeCoordDir];
        var currBridgeSettings = BridgeSettings[coordDir];
        var edgeBridge = edgeSide == EdgeSide.LEFT
            ? hexParts.GetBridgeEdge(
                sideBridgeSettings.SlopeDir, sideBridgeSettings.BridgeToElevation, sideBridgeSettings.Version,
                currBridgeSettings.SlopeDir, currBridgeSettings.BridgeToElevation, currBridgeSettings.Version
            )
            : hexParts.GetBridgeEdge(
                currBridgeSettings.SlopeDir, currBridgeSettings.BridgeToElevation, currBridgeSettings.Version,
                sideBridgeSettings.SlopeDir, sideBridgeSettings.BridgeToElevation, sideBridgeSettings.Version
            );

        GameObject go = null;
        try
        {
            go = Instantiate(edgeBridge.BridgeEdgeModel, transform);
        }
        catch (Exception e)
        {
            Debug.LogError(edgeSide == EdgeSide.LEFT
                ? "(" + HexDebug?.GetId() + ") -> LEFT[" + edgeCoordDir + "]: slopeDir: " + sideBridgeSettings.SlopeDir + ", bridgeToElevation: " + sideBridgeSettings.BridgeToElevation + ", version: " + sideBridgeSettings.Version
                    + ";[" + coordDir + "] slopeDir: " + currBridgeSettings.SlopeDir + ", bridgeToElevation: " + currBridgeSettings.BridgeToElevation + ", version: " + currBridgeSettings.Version
                : "(" + HexDebug?.GetId() + ") -> RIGHT: slopeDir: " + currBridgeSettings.SlopeDir + ", bridgeToElevation: " + currBridgeSettings.BridgeToElevation + ", version: " + currBridgeSettings.Version
                    + ", slopeDir: " + sideBridgeSettings.SlopeDir + ", bridgeToElevation: " + sideBridgeSettings.BridgeToElevation + ", version: " + sideBridgeSettings.Version
            );
            throw e;
        }
        go.transform.eulerAngles = GetBridgeEdgeRotationByEdgeCoord(edgeCoordDirs);

        if (!EdgeSettings.ContainsKey(edgeCoordDirs))
        {
            EdgeSettings.Add(edgeCoordDirs, new Dictionary<EdgeSide, RenderSettings>());
        }

        EdgeSettings[edgeCoordDirs].Add(edgeSide, sideBridgeSettings);
    }

    public static Vector3 GetBridgeEdgeRotationByEdgeCoord(EdgeCoordDir coord)
    {
        switch (coord)
        {
            case EdgeCoordDir.N:
                return Vector3.zero;
            case EdgeCoordDir.NE:
                return new Vector3(0, 60, 0);
            case EdgeCoordDir.SE:
                return new Vector3(0, 120, 0);
            case EdgeCoordDir.S:
                return new Vector3(0, 180, 0);
            case EdgeCoordDir.SW:
                return new Vector3(0, 240, 0);
            case EdgeCoordDir.NW:
            default:
                return new Vector3(0, 300, 0);
        }
    }

    public static EdgeCoordDir[] GetEdgeCoordDirsOfCoordDir(CoordDir coordDir)
    {
        switch (coordDir)
        {
            case CoordDir.NE:
                return new EdgeCoordDir[] { EdgeCoordDir.N, EdgeCoordDir.NE };
            case CoordDir.E:
                return new EdgeCoordDir[] { EdgeCoordDir.NE, EdgeCoordDir.SE };
            case CoordDir.SE:
                return new EdgeCoordDir[] { EdgeCoordDir.SE, EdgeCoordDir.S };
            case CoordDir.SW:
                return new EdgeCoordDir[] { EdgeCoordDir.S, EdgeCoordDir.SW };
            case CoordDir.W:
                return new EdgeCoordDir[] { EdgeCoordDir.SW, EdgeCoordDir.NW };
            case CoordDir.NW:
            default:
                return new EdgeCoordDir[] { EdgeCoordDir.NW, EdgeCoordDir.N };
        }
    }

    public static CoordDir GetLeftCoordDir(CoordDir coordDir)
    {
        int nr = (int)coordDir - 1;
        nr = nr < 0 ? 5 : nr;
        return (CoordDir)nr;
    }

    public static CoordDir GetRightCoordDir(CoordDir coordDir)
    {
        int nr = (int)coordDir + 1;
        nr = nr > 5 ? 0 : nr;
        return (CoordDir)nr;
    }
}
