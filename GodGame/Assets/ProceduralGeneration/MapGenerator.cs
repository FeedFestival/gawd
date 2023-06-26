using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public const int MAP_CHUNK_SIZE = 241;
    [SerializeField]
    [Range(0, 6)]
    private int _editorPreviewLOD;
    [SerializeField]
    private int _seed;
    [SerializeField]
    private float _noiseScale;
    [SerializeField]
    private int _octaves;
    [SerializeField]
    [Range(0, 1)]
    private float _persistance;
    [SerializeField]
    private float _lacunarity;
    [SerializeField]
    private Vector2 _offset = Vector2.zero;
    [SerializeField]
    private float _meshHeightMultiplier;
    [SerializeField]
    private AnimationCurve _meshHeightCurve;

    public TerrainType[] Regions;

    [Header("Generation")]
    public DrawMode DrawMode;
    public Noise.NormalizeMode NormalizeMode;
    public bool useFalloff;
    public bool AutoUpdate;

    private Queue<MapThreadInfo<MapData>> _mapDataThreadingInfoQueue
        = new Queue<MapThreadInfo<MapData>>();
    private Queue<MapThreadInfo<MeshData>> _meshDataThreadingInfoQueue
        = new Queue<MapThreadInfo<MeshData>>();
    private float[,] _falloffMap;

    private void Awake()
    {
        //_falloffMap = FalloffGenerator.GenerateFalloffMap(MAP_CHUNK_SIZE);
    }

    private void Update()
    {
        if (_mapDataThreadingInfoQueue.Count > 0)
        {
            for (int i = 0; i < _mapDataThreadingInfoQueue.Count; i++)
            {
                var threadInfo = _mapDataThreadingInfoQueue.Dequeue();
                threadInfo.Callback(threadInfo.Parameter);
            }
        }

        if (_meshDataThreadingInfoQueue.Count > 0)
        {
            for (int i = 0; i < _meshDataThreadingInfoQueue.Count; i++)
            {
                var threadInfo = _meshDataThreadingInfoQueue.Dequeue();
                threadInfo.Callback(threadInfo.Parameter);
            }
        }
    }

    public void DrawMapInEditor()
    {
        if (useFalloff) {
            _falloffMap = FalloffGenerator.GenerateFalloffMap(MAP_CHUNK_SIZE);
        }
        var mapData = generateMapData(Vector2.zero);

        var mapDisplay = FindObjectOfType<MapDisplay>();
        if (DrawMode == DrawMode.NoiseMap)
        {
            var texture = TextureGenerator.TextureFromHeightMap(mapData.HeightMap);
            mapDisplay.DrawTexture(texture);
        }
        else if (DrawMode == DrawMode.ColorMap)
        {
            var texture = TextureGenerator.TextureFromColorMap(mapData.ColorMap, MAP_CHUNK_SIZE, MAP_CHUNK_SIZE);
            mapDisplay.DrawTexture(texture);
        }
        else if (DrawMode == DrawMode.Mesh)
        {
            var texture = TextureGenerator.TextureFromColorMap(mapData.ColorMap, MAP_CHUNK_SIZE, MAP_CHUNK_SIZE);
            var terrainMesh = MeshGenerator.GenerateTerrainMesh(
                mapData.HeightMap,
                _meshHeightMultiplier,
                _meshHeightCurve,
                _editorPreviewLOD
            );
            mapDisplay.DrawMesh(terrainMesh, texture);
        }
        else if (DrawMode == DrawMode.FalloffMap)
        {
            var falloffMap = FalloffGenerator.GenerateFalloffMap(MAP_CHUNK_SIZE);
            var texture = TextureGenerator.TextureFromHeightMap(falloffMap);
            mapDisplay.DrawTexture(texture);
        }
    }

    public void RequestMapData(Vector2 center, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate { MapDataThread(center, callback); };

        new Thread(threadStart).Start();
    }

    private void MapDataThread(Vector2 center, Action<MapData> callback)
    {
        var mapData = generateMapData(center);
        lock (_mapDataThreadingInfoQueue)
        {
            _mapDataThreadingInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate { MeshDataThread(mapData, lod, callback); };

        new Thread(threadStart).Start();
    }

    private void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        var meshData = MeshGenerator.GenerateTerrainMesh(
            mapData.HeightMap,
            _meshHeightMultiplier,
            _meshHeightCurve,
            lod
        );
        lock (_meshDataThreadingInfoQueue)
        {
            _meshDataThreadingInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private MapData generateMapData(Vector2 center)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(
                MAP_CHUNK_SIZE,
                MAP_CHUNK_SIZE,
                _seed,
                _noiseScale,
                _octaves,
                _persistance,
                _lacunarity,
                center + _offset,
                NormalizeMode
            );

        var colorMap = new Color[MAP_CHUNK_SIZE * MAP_CHUNK_SIZE];
        for (int y = 0; y < MAP_CHUNK_SIZE; y++)
        {
            for (int x = 0; x < MAP_CHUNK_SIZE; x++)
            {
                if (useFalloff)
                {
                    noiseMap[x, y] = noiseMap[x, y] - _falloffMap[x, y];
                }
                float currentHeight = noiseMap[x, y];
                colorMap[y * MAP_CHUNK_SIZE + x] = getRegionColor(currentHeight);
            }
        }

        return new MapData(noiseMap, colorMap);
    }

    private Color getRegionColor(float height)
    {
        for (int i = 0; i < Regions.Length; i++)
        {
            if (height <= Regions[i].Height)
            {
                return Regions[i].Color;
            }
        }
        return Color.white;
    }

    private void OnValidate()
    {
        if (_lacunarity < 1)
        {
            _lacunarity = 1;
        }
        if (_octaves < 0)
        {
            _octaves = 0;
        }
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> Callback;
        public readonly T Parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            Callback = callback;
            Parameter = parameter;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string Name;
    public float Height;
    public Color Color;
}

public struct MapData
{
    public readonly float[,] HeightMap;
    public readonly Color[] ColorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        HeightMap = heightMap;
        ColorMap = colorMap;
    }
}

public enum DrawMode { NoiseMap, ColorMap, Mesh, FalloffMap };
