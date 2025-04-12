//using System;
//using Unity.Collections;
//using Unity.Netcode;

//public struct MeshData : INetworkSerializable, IEquatable<MeshData>
//{
//    public FixedString512Bytes jsonMesh;  // JSON que contiene la malla serializada.
//    public int materialId;  // ID del material.

//    public MeshData(string json, int matId)
//    {
//        jsonMesh = json;
//        materialId = matId;
//    }

//    public bool Equals(MeshData other)
//    {
//        return jsonMesh == other.jsonMesh && materialId == other.materialId;
//    }

//    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
//    {
//        serializer.SerializeValue(ref jsonMesh);
//        serializer.SerializeValue(ref materialId);
//    }

//}
