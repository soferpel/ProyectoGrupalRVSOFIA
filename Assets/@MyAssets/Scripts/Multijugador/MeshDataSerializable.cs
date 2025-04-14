using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MeshDataSerializable
{
    public float[] vertices;
    public float[] normals;
    public int subMeshCount;
    public List<int[]> subMeshTriangles; // Lista de triángulos por submesh
    public float[] uvs; // <-- NUEVO: Para UVs

    public static MeshDataSerializable FromMesh(Mesh mesh)
    {
        var data = new MeshDataSerializable
        {
            subMeshCount = mesh.subMeshCount,
            subMeshTriangles = new List<int[]>()
        };

        // Vértices
        Vector3[] verts = mesh.vertices;
        data.vertices = new float[verts.Length * 3];
        for (int i = 0; i < verts.Length; i++)
        {
            data.vertices[i * 3 + 0] = verts[i].x;
            data.vertices[i * 3 + 1] = verts[i].y;
            data.vertices[i * 3 + 2] = verts[i].z;
        }

        // Normales
        Vector3[] norms = mesh.normals;
        data.normals = new float[norms.Length * 3];
        for (int i = 0; i < norms.Length; i++)
        {
            data.normals[i * 3 + 0] = norms[i].x;
            data.normals[i * 3 + 1] = norms[i].y;
            data.normals[i * 3 + 2] = norms[i].z;
        }

        // Triángulos por submesh
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            data.subMeshTriangles.Add(mesh.GetTriangles(i));
        }

        // UVs
        Vector2[] uvData = mesh.uv;
        data.uvs = new float[uvData.Length * 2]; // Cada UV tiene 2 valores (x, y)
        for (int i = 0; i < uvData.Length; i++)
        {
            data.uvs[i * 2] = uvData[i].x;
            data.uvs[i * 2 + 1] = uvData[i].y;
        }

        return data;
    }

    public Mesh ToMesh()
    {
        Mesh mesh = new Mesh();

        // Restaurar vértices
        Vector3[] verts = new Vector3[vertices.Length / 3];
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = new Vector3(vertices[i * 3], vertices[i * 3 + 1], vertices[i * 3 + 2]);
        }
        mesh.vertices = verts;

        // Restaurar normales
        Vector3[] norms = new Vector3[normals.Length / 3];
        for (int i = 0; i < norms.Length; i++)
        {
            norms[i] = new Vector3(normals[i * 3], normals[i * 3 + 1], normals[i * 3 + 2]);
        }
        mesh.normals = norms;

        // Restaurar triángulos por submesh
        mesh.subMeshCount = subMeshCount;
        for (int i = 0; i < subMeshCount; i++)
        {
            mesh.SetTriangles(subMeshTriangles[i], i);
        }

        // Restaurar UVs
        Vector2[] uvData = new Vector2[uvs.Length / 2];
        for (int i = 0; i < uvData.Length; i++)
        {
            uvData[i] = new Vector2(uvs[i * 2], uvs[i * 2 + 1]);
        }
        mesh.uv = uvData;

        mesh.RecalculateBounds();
        return mesh;
    }
}
