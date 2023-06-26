using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Shared.Enums
{
    public static class WorldBuilderEnums
    {
    }

    public enum EdgeCoordDir
    {
        N, NE, SE, S, SW, NW
    }

    public enum SlopeDir { ASC, DESC, LEVEL }
    public enum EdgeSide { LEFT, RIGHT }

    //public enum HexProp { NAME, SLOPE_ELEVATION, SIDE, COORD, VARIATION }

}