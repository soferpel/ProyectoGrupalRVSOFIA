using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class YourMeshUtility
{



    public static byte[] SerializeMesh(Mesh mesh)
    {
        MeshDataSerializable data = MeshDataSerializable.FromMesh(mesh);
        BinaryFormatter bf = new BinaryFormatter();

        using (MemoryStream ms = new())
        {
            bf.Serialize(ms, data);
            return ms.ToArray();
        }
    }

    public static Mesh DeserializeMesh(byte[] data)
    {
        BinaryFormatter bf = new();
        using (MemoryStream ms = new(data))
        {
            MeshDataSerializable m = (MeshDataSerializable)bf.Deserialize(ms);
            return m.ToMesh();
        }
    }
    public static byte[] SerializeSkinnedMesh(Mesh mesh)
    {
        SkinnedMeshDataSerializable data = SkinnedMeshDataSerializable.FromMesh(mesh);
        BinaryFormatter bf = new BinaryFormatter();

        using (MemoryStream ms = new())
        {
            bf.Serialize(ms, data);
            return ms.ToArray();
        }
    }

    public static Mesh DeserializeSkinnedMesh(byte[] data)
    {
        BinaryFormatter bf = new();
        using (MemoryStream ms = new(data))
        {
            SkinnedMeshDataSerializable m = (SkinnedMeshDataSerializable)bf.Deserialize(ms);
            return m.ToMesh();
        }
    }

}
