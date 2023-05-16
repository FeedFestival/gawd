using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public enum HexTerrain { Unset, DeepSea, Sea, Flat, Highland, Valley, Mountain };
public enum Pole { N, NE, SE, S, SV, NV };

public struct Coord { public int Y; public int X; public Pole Pole; }

public class Hex : MonoBehaviour
{
    public int Y;
    public int X;
    public HexTerrain HexTerrain;
    public TMP_Text Text;

    public HexTerrain[] Neighbors = new HexTerrain[6];

    public static Dictionary<Pole, Pole> MIRROR_POLE = new Dictionary<Pole, Pole>()
    {
        { Pole.N, Pole.S },
        { Pole.NE, Pole.SV },
        { Pole.SE, Pole.NV },
        { Pole.S, Pole.N },
        { Pole.SV, Pole.NE },
        { Pole.NV, Pole.SE }
    };

    public static Dictionary<Pole, Coord> UNEVEN_POLE_COORDS = new Dictionary<Pole, Coord>()
    {
        { Pole.N, new Coord() { Y = 1, X = 0, Pole = Pole.N } },
        { Pole.NE, new Coord() { Y = 0, X = 1, Pole = Pole.NE } },
        { Pole.SE, new Coord() { Y = -1, X = 1, Pole = Pole.SE } },
        { Pole.S, new Coord() { Y = -1, X = 0, Pole = Pole.S } },
        { Pole.SV, new Coord() { Y = -1, X = -1, Pole = Pole.SV } },
        { Pole.NV, new Coord() { Y = 0, X = -1, Pole = Pole.NV } }
    };

    public static Dictionary<Pole, Coord> EVEN_POLE_COORDS = new Dictionary<Pole, Coord>()
    {
        { Pole.N, new Coord() { Y = 1, X = 0, Pole = Pole.N } },
        { Pole.NE, new Coord() { Y = 1, X = 1, Pole = Pole.NE } },
        { Pole.SE, new Coord() { Y = 0, X = 1, Pole = Pole.SE } },
        { Pole.S, new Coord() { Y = -1, X = 0, Pole = Pole.S } },
        { Pole.SV, new Coord() { Y = 0, X = -1, Pole = Pole.SV } },
        { Pole.NV, new Coord() { Y = 1, X = -1, Pole = Pole.NV } }
    };

    internal void Init(int y, int x)
    {
        Y = y;
        X = x;
    }

    internal Coord? GetUnsetNeighborCoords()
    {
        int index = GetUnsetIndex();

        if (index == -1) { return null; }

        var pole = (Pole)index;
        var coords = IsEven() ? EVEN_POLE_COORDS[pole] : UNEVEN_POLE_COORDS[pole];

        Debug.Log("[" + pole + "] coords: " + coords);

        return coords;
    }

    internal void SetNeighborsTerrain(Dictionary<int, Dictionary<int, Hex>> hexes)
    {
        
        if (IsEven())
        {
            setNeighborsTerrain(hexes, EVEN_POLE_COORDS);
        }
        else
        {
            setNeighborsTerrain(hexes, UNEVEN_POLE_COORDS);
        }
    }

    internal void SetHexTerrainOfNeighbor(Pole pole, HexTerrain hexTerrain)
    {
        Neighbors[(int)pole] = hexTerrain;
    }

    private int GetUnsetIndex()
    {
        for (int i = 0; i < Neighbors.Length; i++)
        {
            if (Neighbors[i] == HexTerrain.Unset) { return i; }
        }
        return -1;
    }

    internal void SetHexTerrain(HexTerrain hexTerrain)
    {
        HexTerrain = hexTerrain;
        Text.text = gameObject.name;
        gameObject.name = gameObject.name + " " + hexTerrain;
    }

    internal bool IsEven()
    {
        return X % 2 == 0;
    }

    private void setNeighborsTerrain(Dictionary<int, Dictionary<int, Hex>> hexes, Dictionary<Pole, Coord> poleCoords)
    {
        int y, x;
        foreach (var kvp in poleCoords)
        {
            y = Y + kvp.Value.Y;
            x = X + kvp.Value.X;
            bool exists = HexMapGenerator.DoesHexExist(hexes, y, x);
            if (exists)
            {
                SetHexTerrainOfNeighbor(kvp.Key, hexes[y][x].HexTerrain);
                hexes[y][x].SetHexTerrainOfNeighbor(Hex.MIRROR_POLE[kvp.Key], HexTerrain);
            }
        }
    }
}
