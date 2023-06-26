using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
    public static float MAX_VIEW_DISTANCE_REF;
    public static Vector2 VIEWER_POSITION;
    private static MapGenerator MapGenerator_REF; // TODO: refactor this
    const float VIEWER_MOVE_THRESHOLD_FOR_CHUNK_UPDATE = 25f;
    const float SQR_VIEWER_MOVE_THRESHOLD_FOR_CHUNK_UPDATE
        = VIEWER_MOVE_THRESHOLD_FOR_CHUNK_UPDATE * VIEWER_MOVE_THRESHOLD_FOR_CHUNK_UPDATE;

    [SerializeField]
    private LODInfo[] _detailLevels;
    [SerializeField]
    private Transform _viewer;
    [SerializeField]
    private Material _mapMaterial;
    private Vector2 _viewerPositionOld;

    private int _chunkSize;
    private int _chunkVisibleInViewDist;

    Dictionary<Vector2, TerrainChunk> _terrainChunkDict = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> S_LAST_VISIBLE_CHUNKS = new List<TerrainChunk>();

    private void Start()
    {
        MAX_VIEW_DISTANCE_REF = _detailLevels[_detailLevels.Length - 1].VisibleDistThreshold;
        MapGenerator_REF = FindObjectOfType<MapGenerator>();
        _chunkSize = MapGenerator.MAP_CHUNK_SIZE - 1;
        _chunkVisibleInViewDist = Mathf.RoundToInt(MAX_VIEW_DISTANCE_REF / _chunkSize);

        updateVisibleChunks();
    }

    private void Update()
    {
        VIEWER_POSITION = new Vector2(_viewer.position.x, _viewer.position.z);

        if ((_viewerPositionOld - VIEWER_POSITION).sqrMagnitude > SQR_VIEWER_MOVE_THRESHOLD_FOR_CHUNK_UPDATE)
        {
            _viewerPositionOld = VIEWER_POSITION;
            updateVisibleChunks();
        }
    }

    private void updateVisibleChunks()
    {
        for (int i = 0; i < S_LAST_VISIBLE_CHUNKS.Count; i++)
        {
            S_LAST_VISIBLE_CHUNKS[i].SetVisible(false);
        }
        S_LAST_VISIBLE_CHUNKS.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(VIEWER_POSITION.x / _chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(VIEWER_POSITION.y / _chunkSize);

        for (int yOffset = -_chunkVisibleInViewDist; yOffset <= _chunkVisibleInViewDist; yOffset++)
        {
            for (int xOffset = -_chunkVisibleInViewDist; xOffset <= _chunkVisibleInViewDist; xOffset++)
            {
                var viewedChunkCoord = new Vector2(currentChunkCoordX - xOffset, currentChunkCoordY - yOffset);

                if (_terrainChunkDict.ContainsKey(viewedChunkCoord))
                {
                    _terrainChunkDict[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    _terrainChunkDict.Add(
                        viewedChunkCoord,
                        new TerrainChunk(viewedChunkCoord, _chunkSize, _detailLevels, transform, _mapMaterial)
                    );
                }
            }
        }
    }

    public class TerrainChunk
    {
        private GameObject _meshObject;
        private Vector2 _position;
        private Bounds _bounds;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private LODInfo[] _detailLevels;
        private LODMesh[] _lodMeshes;

        private MapData _mapData;
        private bool _mapDataRecived;
        private int _previousLODIndex = -1;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parentT, Material material)
        {
            _detailLevels = detailLevels;

            _position = coord * size;
            _bounds = new Bounds(_position, Vector2.one * size);
            var positionV3 = new Vector3(_position.x, 0, _position.y);

            _meshObject = new GameObject("TerrainChunk");

            _meshFilter = _meshObject.AddComponent<MeshFilter>();
            _meshRenderer = _meshObject.AddComponent<MeshRenderer>();
            _meshRenderer.material = material;

            _meshObject.transform.position = positionV3;
            _meshObject.transform.SetParent(parentT);

            SetVisible(false);


            _lodMeshes = new LODMesh[_detailLevels.Length];
            for (int i = 0; i < _detailLevels.Length; i++)
            {
                _lodMeshes[i] = new LODMesh(detailLevels[i].Lod, UpdateTerrainChunk);
            }

            MapGenerator_REF.RequestMapData(_position, onMapDataRecieved);
        }

        private void onMapDataRecieved(MapData mapData)
        {
            _mapData = mapData;
            _mapDataRecived = true;

            var texture = TextureGenerator
                .TextureFromColorMap(mapData.ColorMap, MapGenerator.MAP_CHUNK_SIZE, MapGenerator.MAP_CHUNK_SIZE);
            _meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            if (_mapDataRecived == false) { return; }

            float viewerDstFromNearestEdge = Mathf.Sqrt(_bounds.SqrDistance(VIEWER_POSITION));
            bool visible = viewerDstFromNearestEdge <= MAX_VIEW_DISTANCE_REF;

            if (visible)
            {
                int lodIndex = 0;
                for (int i = 0; i < _detailLevels.Length - 1; i++)
                {
                    if (viewerDstFromNearestEdge > _detailLevels[i].VisibleDistThreshold)
                    {
                        lodIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                if (lodIndex != _previousLODIndex)
                {
                    var lodMesh = _lodMeshes[lodIndex];
                    if (lodMesh.HasMesh)
                    {
                        _previousLODIndex = lodIndex;
                        _meshFilter.mesh = lodMesh.Mesh;
                    }
                    else if (lodMesh.HasRequestedMesh == false)
                    {
                        lodMesh.RequestMesh(_mapData);
                    }
                }

                S_LAST_VISIBLE_CHUNKS.Add(this);
            }

            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            _meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return _meshObject.activeSelf;
        }
    }

    class LODMesh
    {
        public Mesh Mesh;
        public bool HasRequestedMesh;
        public bool HasMesh;
        private int _lod;
        private Action _updateCallback;

        public LODMesh(int lod, Action updateCallback)
        {
            _lod = lod;
            _updateCallback = updateCallback;
        }

        public void RequestMesh(MapData mapData)
        {
            HasRequestedMesh = true;
            MapGenerator_REF.RequestMeshData(mapData, _lod, onMeshDataRecieved);
        }

        private void onMeshDataRecieved(MeshData meshData)
        {
            Mesh = meshData.CreateMesh();
            HasMesh = true;

            _updateCallback();
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int Lod;
        public float VisibleDistThreshold;
    }
}
