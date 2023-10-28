using Game.Shared.Interfaces;
using Game.Shared.Structs;
using Game.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.FogOfWar
{
    public class FogOfWarController : MonoBehaviour
    {
        public Action<List<ICoord>> OnNewEdgesDiscovered;

        [SerializeField]
        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;

        public const float FOG_Y_RAYCAST_POS = 10f;

        public void PreSetup()
        {
            OnNewEdgesDiscovered += (List<ICoord> justAddedCoords) =>
            {
                //onNewEdgesDiscovered(justAddedCoords);
            };

            _meshCollider = _meshFilter.GetComponent<MeshCollider>();
        }

        public void Init(ICoord middleCoord)
        {
            _meshFilter.mesh.Clear();
            _meshFilter.mesh.name = "fog_of_war_mesh";

            var coords = getVisionRangeCoords(middleCoord, 6);
            //onNewEdgesDiscovered(new List<ICoord>() { middleCoord });
            onNewEdgesDiscovered(coords);

            _meshFilter.gameObject.transform.position = new Vector3(
                _meshFilter.gameObject.transform.position.x,
                1.5f,
                _meshFilter.gameObject.transform.position.z
            );
        }

        private void onNewEdgesDiscovered(List<ICoord> justAddedCoords)
        {
            foreach (var hexCoord in justAddedCoords)
            {
                var pos = HexUtils.GetHexPosition(hexCoord);
                var points = new Vector3[6];
                for (int i = 0; i < HexUtils.EDGE_DIRECTIONS.Length; i++)
                {
                    var edgeDir = HexUtils.EDGE_DIRECTIONS[i];
                    var posOffset = HexUtils.EDGE_STITCH_POSITION[edgeDir];
                    points[i] = pos + posOffset;
                }

                createHexFog(points);
            }

            _meshCollider.sharedMesh = _meshFilter.mesh;
        }

        public void RemoveFog(ICoord unitCoord, int visionRange)
        {
            var positions = getCoordVisionRangePositions(unitCoord, visionRange);
            var triangleIndexes = new List<int>();
            foreach (var pos in positions)
            {
                var posTriangleIndexes = getTrianglesIndexByRaycast(pos);
                triangleIndexes.AddRange(posTriangleIndexes);
            }
            triangleIndexes.Reverse();

            var triangles = _meshFilter.mesh.triangles.ToList();
            foreach (int index in triangleIndexes)
            {
                triangles.RemoveRange(index * 3, 3);
            }

            _meshFilter.mesh.triangles = triangles.ToArray();
            if (_meshFilter.mesh.triangles.Length > 0)
            {
                _meshCollider.sharedMesh = _meshFilter.mesh;
            }
            else
            {
                _meshCollider.sharedMesh.Clear();
            }
        }

        private void createHexFog(Vector3[] points)
        {
            var newMesh = new Mesh();

            var startIndex = _meshFilter.mesh.vertices.Length;
            var previousVertices = _meshFilter.mesh.vertices.ToList();
            previousVertices.AddRange(points);
            newMesh.vertices = previousVertices.ToArray();

            var newHexTriangles = getHexTriangles(startIndex);
            var previousTriangles = _meshFilter.mesh.triangles.ToList();
            previousTriangles.AddRange(newHexTriangles);
            newMesh.triangles = previousTriangles.ToArray();

            _meshFilter.mesh = newMesh;

            _meshFilter.mesh.RecalculateNormals();
            _meshFilter.mesh.RecalculateBounds();
        }

        private int[] getHexTriangles(int startIndex)
        {
            var triangles = new int[12] {
                startIndex + 0, startIndex + 4, startIndex + 5,
                startIndex + 0, startIndex + 1, startIndex + 4,
                startIndex + 1, startIndex + 2, startIndex + 3,
                startIndex + 1, startIndex + 3, startIndex + 4,
            };
            return triangles;
        }

        private List<int> getTrianglesIndexByRaycast(Vector3 pos)
        {
            var triangles = new List<int>();
            for (int i = 0; i < HexUtils.RAY_FOG_OFFSETS.Count; i++)
            {
                var toPos = pos + HexUtils.RAY_FOG_OFFSETS[i];
                var rayPos = toPos + (Vector3.up * FOG_Y_RAYCAST_POS);
                var rayDir = (toPos - rayPos).normalized;
                RaycastHit hit;
                if (Physics.Raycast(rayPos, rayDir, out hit, Mathf.Infinity))
                {
                    Debug.DrawRay(rayPos, rayDir * 15f, Color.yellow, 5f);
                    triangles.Add(hit.triangleIndex);
                }
                else
                {
                    Debug.DrawRay(rayPos, rayDir * 15f, Color.red, 5f);
                }
            }
            return triangles;
        }

        private List<Vector3> getCoordVisionRangePositions(ICoord unitCoord, int visionRange)
        {
            var visionCoords = getVisionRangeCoords(unitCoord, visionRange);
            var positions = new List<Vector3>();
            foreach (var visionCoord in visionCoords)
            {
                positions.Add(HexUtils.GetHexPosition(visionCoord));
            }

            return positions;
        }

        private List<ICoord> getVisionRangeCoords(ICoord startCoord, int visionRange)
        {
            int hexCount = HexUtils.GetHexCountByRange(visionRange);
            ICoord curCoord;
            var visionCoords = new List<ICoord>() { startCoord };
            var onEdge = new Queue<ICoord>() { };
            onEdge.Enqueue(startCoord);

            for (int i = 0; i < hexCount - 1; i++)
            {
                curCoord = onEdge.Dequeue();

                foreach (var dir in HexUtils.DIRECTIONS)
                {
                    var isOdd = HexUtils.IsOddRow(curCoord.Y);
                    var coordOffset = isOdd ? HexUtils.COORD_ODD__OFFSET[dir] : HexUtils.COORD_EVEN_OFFSET[dir];
                    var neighborCoord = Coord.AddTogheter(curCoord, coordOffset);

                    var checkedAlready = visionCoords.Contains(neighborCoord);
                    if (checkedAlready) { continue; }

                    i++;
                    visionCoords.Add(neighborCoord);
                    onEdge.Enqueue(neighborCoord);
                }
                i--;
            }
            return visionCoords;
        }
    }
}