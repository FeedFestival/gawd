using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TriangleNet.Geometry;
using TriangleNet.Meshing;

public class Triangulator
{
    public static Mesh CreateConcaveHullMesh(List<Vector3> points)
    {
        Polygon polygon = new Polygon();

        // Keep track of the added vertices
        List<Vertex> addedVertices = new List<Vertex>();

        // Add the points to the polygon.
        for (int i = 0; i < points.Count; i++)
        {
            var vertex = new Vertex(points[i].x, points[i].z);
            polygon.Add(vertex);
            addedVertices.Add(vertex);

            if (i > 0)
            {
                // Use the added vertices to create the segment
                polygon.Add(new Segment(addedVertices[i - 1], addedVertices[i]));
            }
        }

        // Close the polygon.
        polygon.Add(new Segment(addedVertices[points.Count - 1], addedVertices[0]));

        // Create a constraint options
        ConstraintOptions options = new ConstraintOptions() { ConformingDelaunay = true };
        QualityOptions quality = new QualityOptions() { SteinerPoints = 100 };
        // Generate the constrained Delaunay triangulation.
        var mesh = polygon.Triangulate(options, quality);

        // Convert the result to a Unity Mesh.
        return ConvertMesh(mesh);
    }

    public static Mesh CreateHole(Mesh mesh, int triangleIndex)
    {
        // Ensure the triangle index is within the range of the mesh's triangles.
        if (triangleIndex < 0 || triangleIndex >= mesh.triangles.Length / 3)
        {
            throw new System.ArgumentOutOfRangeException(nameof(triangleIndex));
        }

        // Get a copy of the current triangles.
        var triangles = mesh.triangles;

        // Replace the triangle with the last triangle in the list.
        triangles[triangleIndex * 3 + 0] = triangles[triangles.Length - 3];
        triangles[triangleIndex * 3 + 1] = triangles[triangles.Length - 2];
        triangles[triangleIndex * 3 + 2] = triangles[triangles.Length - 1];

        // Create a new list of triangles without the last one.
        var newTriangles = new int[triangles.Length - 3];
        System.Array.Copy(triangles, newTriangles, newTriangles.Length);

        // Create a new mesh with the new list of triangles.
        var newMesh = new Mesh();
        newMesh.vertices = mesh.vertices;
        newMesh.triangles = newTriangles;
        newMesh.normals = mesh.normals; // You might need to recalculate this.
        newMesh.uv = mesh.uv; // You might need to recalculate this.

        return newMesh;
    }

    //public static Mesh CreateMeshWithHole(List<Vector3> mainPoints, List<Vector3> holePoints)
    //{
    //    Polygon polygon = new Polygon();

    //    // Keep track of the added vertices
    //    List<Vertex> addedVertices = new List<Vertex>();

    //    // Add the main points to the polygon.
    //    for (int i = 0; i < mainPoints.Count; i++)
    //    {
    //        var vertex = new Vertex(mainPoints[i].x, mainPoints[i].z);
    //        polygon.Add(vertex);
    //        addedVertices.Add(vertex);

    //        if (i > 0)
    //        {
    //            // Use the added vertices to create the segment
    //            polygon.Add(new Segment(addedVertices[i - 1], addedVertices[i]));
    //        }
    //    }

    //    // Close the polygon.
    //    polygon.Add(new Segment(addedVertices[mainPoints.Count - 1], addedVertices[0]));

    //    // Add the hole to the polygon
    //    polygon.AddHole(holePoints.Select(p => new Vertex(p.x, p.z)).ToList());

    //    // Create a constraint options
    //    ConstraintOptions options = new ConstraintOptions() { ConformingDelaunay = true };

    //    // Generate the constrained Delaunay triangulation.
    //    var mesh = polygon.Triangulate(options);

    //    // Convert the result to a Unity Mesh.
    //    return ConvertMesh(mesh);
    //}

    private static Mesh ConvertMesh(IMesh inputMesh)
    {
        // Convert the vertices and triangles for Unity.
        Vector3[] vertices = new Vector3[inputMesh.Vertices.Count];
        int[] triangles = new int[inputMesh.Triangles.Count * 3];

        int i = 0;
        foreach (var vertex in inputMesh.Vertices)
        {
            vertices[i] = new Vector3((float)vertex.X, 0, (float)vertex.Y);
            i++;
        }

        i = 0;
        foreach (var triangle in inputMesh.Triangles)
        {
            triangles[i * 3] = triangle.GetVertexID(0);
            triangles[i * 3 + 1] = triangle.GetVertexID(2);
            triangles[i * 3 + 2] = triangle.GetVertexID(1); // Reversed for correct orientation.
            i++;
        }

        // Create the Unity Mesh.
        Mesh unityMesh = new Mesh
        {
            vertices = vertices,
            triangles = triangles
        };

        unityMesh.RecalculateNormals();
        unityMesh.RecalculateBounds();

        return unityMesh;
    }
}
