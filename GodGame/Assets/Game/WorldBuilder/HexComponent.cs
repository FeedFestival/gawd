using UnityEngine;
using Game.Shared.Interfaces;
using Game.Shared.Enums;
using System.Collections.Generic;
using static Game.Shared.Structs.HexPartsStructs;

namespace Game.WorldBuilder
{
    public enum HexTerrain { Unset, DeepSea, Sea, Flat, Highland, Valley, Mountain };

    public class HexComponent : MonoBehaviour, IHexComponent
    {
        public int Y;
        public int X;

        public int Elevation;
        public int Version;

        public Transform Transform { get => transform; }

        [SerializeField]
        private HexDebug _hexDebug;

        public Dictionary<Dir, Bridge> Bridges;
        public Dictionary<EdgeDir, bool> EdgeStitches;

        public void Init(IHex hex)
        {
            _hexDebug?.SetId(hex.ID);
            _hexDebug?.SetCoord(hex.Y, hex.X);

            Elevation = hex.Elevation;
            Version = 0;

            Bridges = new Dictionary<Dir, Bridge>();
            EdgeStitches = new Dictionary<EdgeDir, bool>();
            //BridgeSettings = new Dictionary<CoordDir, RenderSettings>();
            //EdgeSettings = new Dictionary<EdgeCoordDir, Dictionary<EdgeSide, RenderSettings>>();
        }

        public void AddNeighbor(Dir dir, ICoord newNeighborCoord, int id)
        {
            _hexDebug.SetNeighbor(dir, id);

            // make bridge
        }

        //public HexTerrain HexTerrain;
        //public TMP_Text Text;

        //public HexTerrain[] Neighbors = new HexTerrain[6];

        //internal void Init(int y, int x)
        //{
        //    Y = y;
        //    X = x;
        //}

        //internal Coord? GetUnsetNeighborCoords()
        //{
        //    int index = GetUnsetIndex();

        //    if (index == -1) { return null; }

        //    var pole = (Dir)index;
        //    var coords = IsEven() ? HexUtils.COORD_EVEN_OFFSET[pole] : HexUtils.COORD_ODD__OFFSET[pole];

        //    Debug.Log("[" + pole + "] coords: " + coords);

        //    return coords;
        //}

        //internal void SetNeighborsTerrain(Dictionary<int, Dictionary<int, HexComponent>> hexes)
        //{

        //    if (IsEven())
        //    {
        //        setNeighborsTerrain(hexes, HexUtils.COORD_EVEN_OFFSET);
        //    }
        //    else
        //    {
        //        setNeighborsTerrain(hexes, HexUtils.COORD_ODD__OFFSET);
        //    }
        //}

        //internal void SetHexTerrainOfNeighbor(Dir dir, HexTerrain hexTerrain)
        //{
        //    Neighbors[(int)dir] = hexTerrain;
        //}

        //private int GetUnsetIndex()
        //{
        //    for (int i = 0; i < Neighbors.Length; i++)
        //    {
        //        if (Neighbors[i] == HexTerrain.Unset) { return i; }
        //    }
        //    return -1;
        //}

        //internal void SetHexTerrain(HexTerrain hexTerrain)
        //{
        //    HexTerrain = hexTerrain;
        //    Text.text = gameObject.name;
        //    gameObject.name = gameObject.name + " " + hexTerrain;
        //}

        //internal bool IsEven()
        //{
        //    return X % 2 == 0;
        //}

        //private void setNeighborsTerrain(Dictionary<int, Dictionary<int, HexComponent>> hexes, Dictionary<Dir, Coord> poleCoords)
        //{
        //    int y, x;
        //    foreach (var kvp in poleCoords)
        //    {
        //        y = Y + kvp.Value.Y;
        //        x = X + kvp.Value.X;
        //        bool exists = HexMapGenerator.DoesHexExist(hexes, y, x);
        //        if (exists)
        //        {
        //            SetHexTerrainOfNeighbor(kvp.Key, hexes[y][x].HexTerrain);
        //            hexes[y][x].SetHexTerrainOfNeighbor(HexUtils.OPPOSITE_DIR[kvp.Key], HexTerrain);
        //        }
        //    }
        //}
    }
}
