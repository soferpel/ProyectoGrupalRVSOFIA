using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class MeshDataSerializable
{
    public SerializableVector3[] vertices;
    public SerializableVector3[] normals;
    public SerializableVector2[] uv;
    public SerializableColor[] colors;
    public SerializableVector4[] tangents;

    public int subMeshCount;
    public List<int[]> subMeshTriangles = new();

    public static MeshDataSerializable FromMesh(Mesh mesh)
    {
        var data = new MeshDataSerializable
        {
            vertices = Array.ConvertAll(mesh.vertices, SerializableVector3.FromVector3),
            normals = Array.ConvertAll(mesh.normals, SerializableVector3.FromVector3),
            uv = Array.ConvertAll(mesh.uv, SerializableVector2.FromVector2),
            colors = Array.ConvertAll(mesh.colors, SerializableColor.FromColor),
            tangents = Array.ConvertAll(mesh.tangents, SerializableVector4.FromVector4),
            subMeshCount = mesh.subMeshCount,
        };

        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            data.subMeshTriangles.Add(mesh.GetTriangles(i));
        }

        return data;
    }

    public virtual Mesh ToMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = Array.ConvertAll(vertices, v => v.ToVector3()),
            normals = Array.ConvertAll(normals, v => v.ToVector3()),
            uv = Array.ConvertAll(uv, v => v.ToVector2()),
            colors = Array.ConvertAll(colors, c => c.ToColor()),
            tangents = Array.ConvertAll(tangents, v => v.ToVector4()),
            subMeshCount = subMeshCount
        };

        for (int i = 0; i < subMeshCount; i++)
        {
            mesh.SetTriangles(subMeshTriangles[i], i);
        }

        mesh.RecalculateBounds();
        return mesh;
    }

    [Serializable]
    public struct SerializableVector3
    {
        public float x, y, z;
        public SerializableVector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static SerializableVector3 FromVector3(Vector3 v) => new(v.x, v.y, v.z);
        public Vector3 ToVector3() => new(x, y, z);
    }

    [Serializable]
    public struct SerializableVector2
    {
        public float x, y;
        public SerializableVector2(float x, float y) { this.x = x; this.y = y; }
        public static SerializableVector2 FromVector2(Vector2 v) => new(v.x, v.y);
        public Vector2 ToVector2() => new(x, y);
    }

    [Serializable]
    public struct SerializableColor
    {
        public float r, g, b, a;
        public SerializableColor(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static SerializableColor FromColor(Color c) => new(c.r, c.g, c.b, c.a);
        public Color ToColor() => new(r, g, b, a);
    }

    [Serializable]
    public struct SerializableVector4
    {
        public float x, y, z, w;
        public SerializableVector4(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
        public static SerializableVector4 FromVector4(Vector4 v) => new(v.x, v.y, v.z, v.w);
        public Vector4 ToVector4() => new(x, y, z, w);
    }
}
