using Game.Shared.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMapGenerator : MonoBehaviour
{
    public GameObject HexPrefab;
    public Transform HexMapRootT;

    //public Queue<Coord> _hexUnsetEdge = new Queue<Coord>();
    //public Dictionary<int, Dictionary<int, HexComponent>> _hexes = new Dictionary<int, Dictionary<int, HexComponent>>();

    //private Coord _currentCoord;

    //public static Dictionary<Pole, Vector3> EVEN_POLE_POSITION = new Dictionary<Pole, Vector3>()
    //{
    //    { Pole.N, new Vector3(0, 0, 1.732f) },
    //    { Pole.NE, new Vector3(1.5f, 0, 0.866f) },
    //    { Pole.SE, new Vector3(1.5f, 0, -0.866f) },
    //    { Pole.S, new Vector3(0, 0, -1.732f) },
    //    { Pole.SV, new Vector3(-1.5f, 0, -0.866f) },
    //    { Pole.NV, new Vector3(-1.5f, 0, 0.866f) }
    //};

    // Start is called before the first frame update
    void Start()
    {
        //_currentCoord = new Coord() { Y = 0, X = 0 };

        //var hex = getNewHex(0, 0, new Vector3(0, 0.1f, 0));
        //addToHexes(hex);

        //AddEdgeHex();
        //AddEdgeHex();
        //AddEdgeHex();
        //AddEdgeHex();
        //AddEdgeHex();
        //AddEdgeHex();
        //AddEdgeHex();
        //AddEdgeHex();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.E))
        {
            AddEdgeHex();
        }
    }

    internal void AddEdgeHex()
    {
        //var currentHex = _hexes[_currentCoord.Y][_currentCoord.X];
        //int y, x;
        //var coord = getUnsetNeighborCoords(ref currentHex);

        //Debug.Log("currentHex[" + currentHex.Y + ", " + currentHex.X + "] ");
        //y = _currentCoord.Y + coord.Y;
        //x = _currentCoord.X + coord.X;
        //Debug.Log("nextHex[" + y + ", " + x + "] ");

        //var posAddition = getNewPositionAddition(coord);
        //var pos = new Vector3(currentHex.transform.position.x + posAddition.x, 0.1f, currentHex.transform.position.z + posAddition.z);

        //var hex = getNewHex(y, x, pos);

        //hex.SetNeighborsTerrain(_hexes);
        ////hex.SetNeighborTerrain(Hex.MIRROR_POLE[coord.Pole], currentHex.HexTerrain);
        //currentHex.Neighbors[(int)coord.Pole] = HexTerrain.Flat;

        //_hexUnsetEdge.Enqueue(new Coord() { Y = y, X = x });
        //addToHexes(hex);
    }

    private void addToHexes(IHexComponent hex)
    {
        //if (_hexes.ContainsKey(hex.Y) == false)
        //{
        //    _hexes.Add(hex.Y, new Dictionary<int, HexComponent>());
        //}

        //_hexes[hex.Y].Add(hex.X, hex);
    }

    //private Vector3 getNewPositionAddition(Coord coord)
    //{
    //    return EVEN_POLE_POSITION[coord.Pole];
    //}

    //private IHexComponent getNewHex(int y, int x, Vector3 pos)
    //{
    //    var go = Instantiate(HexPrefab, HexMapRootT);

    //    go.transform.position = pos;
    //    go.name = y + "_" + x;
    //    var hex = go.GetComponent<HexComponent>();

    //    //hex.Init(y, x);
    //    //hex.SetHexTerrain(HexTerrain.Flat); // TODO: determine this

    //    return hex;
    //}

    // TODO: this is broken now, fix it !!!!
    //private Coord getUnsetNeighborCoords(ref HexComponent currentHex)
    //{
    //    var coord = currentHex.GetUnsetNeighborCoords();

    //    if (coord.HasValue == false)
    //    {
    //        //_currentCoord = _hexUnsetEdge.Dequeue();
    //        //currentHex = _hexes[_currentCoord.Y][_currentCoord.X];

    //        return getUnsetNeighborCoords(ref currentHex);
    //    }
    //    //if (coord.HasValue == false)
    //    //{
    //    //    _currentCoord = _hexUnsetEdge.Dequeue();
    //    //    currentHex = _hexes[_currentCoord.Y][_currentCoord.X];
    //    //    coord = currentHex.GetUnsetNeighborCoords();

    //    //    coord = this.findCoordsIfTheyDontExist(coord, ref currentHex);
    //    //}
    //    //else
    //    //{
    //    //    bool exists = isHexAlreadyCreated(_currentCoord.Y + coord.Value.Y, _currentCoord.X + coord.Value.X);
    //    //    if (exists)
    //    //    {
    //    //        coord = this.findCoordsIfTheyDontExist(null, ref currentHex);
    //    //    }
    //    //}

    //    return coord.Value;
    //}

    //private Coord findCoordsIfTheyDontExist(Coord? coord, ref HexComponent currentHex)
    //{
    //    while (coord.HasValue == false)
    //    {
    //        _currentCoord = _hexUnsetEdge.Dequeue();
    //        currentHex = _hexes[_currentCoord.Y][_currentCoord.X];
    //        coord = currentHex.GetUnsetNeighborCoords();

    //        if (coord.HasValue == false) { continue; }

    //        bool exists = HexMapGenerator.DoesHexExist(_hexes, _currentCoord.Y + coord.Value.Y, _currentCoord.X + coord.Value.X);
    //        if (exists)
    //        {
    //            coord = null;
    //        }
    //    }
    //    return coord.Value;
    //}

    internal static bool DoesHexExist(Dictionary<int, Dictionary<int, IHexComponent>> hexes, int y, int x)
    {
        return hexes.ContainsKey(y) && hexes[y].ContainsKey(x);
    }
}
