using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Netcode;
using UnityEngine;

public class CutPieceNetworkSync : NetworkBehaviour
{
    public byte[] meshData;
    public int materialId;
    public Material materialPrueba;
    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsOwner)
        {
           // Debug.Log("llamado en ONNETW");
           // BuildCutPiece();
        }
    }

    public void Init(byte[] meshData, int materialId)
    {
        this.meshData = meshData;
        this.materialId = materialId;
        Debug.Log("Init");
        if (IsServer || IsHost)
        {
            Debug.Log("InitIf INIT");
            BuildCutPiece();
        }
        if (IsClient && !IsOwner)
        {
            // Debug.Log("llamado en ONNETW");
            // BuildCutPiece();
        }
    }

    private void BuildCutPiece()
    {
        Mesh mesh = DeserializeMesh(meshData);
        Material mat = materialPrueba;

        GameObject child = new GameObject("Part2");
        Debug.Log("CREADO");
        child.transform.SetParent(transform);
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;

        var mf = child.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        var mr = child.AddComponent<MeshRenderer>();
        mr.material = mat;

        var mc = child.AddComponent<MeshCollider>();
        mc.sharedMesh = mesh;
        mc.convex = true;
    }

    public static byte[] SerializeMesh(Mesh mesh)
    {
        MeshDataSerializable data = MeshDataSerializable.FromMesh(mesh);
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream())
        {
            bf.Serialize(ms, data);
            return ms.ToArray();
        }
    }

    public static Mesh DeserializeMesh(byte[] data)
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream(data))
        {
            MeshDataSerializable m = (MeshDataSerializable)bf.Deserialize(ms);
            return m.ToMesh();
        }
    }
}
