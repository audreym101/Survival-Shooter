using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlane))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CustomPlaneVisualizer : MonoBehaviour
{
    ARPlane _plane;
    Mesh _mesh;
    MeshFilter _meshFilter;

    void Awake()
    {
        _plane = GetComponent<ARPlane>();
        _meshFilter = GetComponent<MeshFilter>();
        _mesh = new Mesh();
        _meshFilter.mesh = _mesh;
    }

    void OnEnable() => _plane.boundaryChanged += OnBoundaryChanged;
    void OnDisable() => _plane.boundaryChanged -= OnBoundaryChanged;

    void OnBoundaryChanged(ARPlaneBoundaryChangedEventArgs args)
    {
        var boundary = _plane.boundary;
        if (boundary.Length < 3) return;

        var vertices = new Vector3[boundary.Length];
        var uvs = new Vector2[boundary.Length];

        // Find bounds for UV mapping
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        for (int i = 0; i < boundary.Length; i++)
        {
            vertices[i] = new Vector3(boundary[i].x, 0, boundary[i].y);
            if (boundary[i].x < minX) minX = boundary[i].x;
            if (boundary[i].x > maxX) maxX = boundary[i].x;
            if (boundary[i].y < minZ) minZ = boundary[i].y;
            if (boundary[i].y > maxZ) maxZ = boundary[i].y;
        }

        float width = maxX - minX;
        float height = maxZ - minZ;

        for (int i = 0; i < boundary.Length; i++)
            uvs[i] = new Vector2(
                (boundary[i].x - minX) / width,
                (boundary[i].y - minZ) / height
            );

        // Triangulate
        var indices = new List<int>();
        for (int i = 1; i < boundary.Length - 1; i++)
        {
            indices.Add(0);
            indices.Add(i);
            indices.Add(i + 1);
        }

        _mesh.Clear();
        _mesh.vertices = vertices;
        _mesh.uv = uvs;
        _mesh.triangles = indices.ToArray();
        _mesh.RecalculateNormals();
    }
}
