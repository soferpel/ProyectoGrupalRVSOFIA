using System;
using UnityEngine;

[Serializable]
public class SkinnedMeshDataSerializable : MeshDataSerializable
{
    public BoneWeight[] boneWeights;
    public SerializableMatrix4x4[] bindposes;

    public static new SkinnedMeshDataSerializable FromMesh(Mesh mesh)
    {
        var baseData = MeshDataSerializable.FromMesh(mesh);
        var skinnedData = new SkinnedMeshDataSerializable
        {
            vertices = baseData.vertices,
            normals = baseData.normals,
            uv = baseData.uv,
            colors = baseData.colors,
            tangents = baseData.tangents,
            subMeshCount = baseData.subMeshCount,
            subMeshTriangles = baseData.subMeshTriangles,

            boneWeights = mesh.boneWeights,
            bindposes = Array.ConvertAll(mesh.bindposes, SerializableMatrix4x4.FromMatrix)
        };
        return skinnedData;
    }

    public override Mesh ToMesh()
    {
        Mesh mesh = base.ToMesh();
        mesh.boneWeights = boneWeights;
        mesh.bindposes = Array.ConvertAll(bindposes, b => b.ToMatrix());
        return mesh;
    }

}
