using System;
using UnityEngine;
using Unity.Netcode;

[Serializable]
public class MaterialData : INetworkSerializable
{
    public string shaderName;
    public Color color;
    public float metallic;
    public float smoothness;
    public byte[] textureBytes; // Textura como byte array

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref shaderName);
        serializer.SerializeValue(ref color);
        serializer.SerializeValue(ref metallic);
        serializer.SerializeValue(ref smoothness);

        // Serializar tamaño y contenido del array
        if (serializer.IsWriter)
        {
            int length = textureBytes != null ? textureBytes.Length : 0;
            serializer.SerializeValue(ref length);
            if (length > 0)
            {
                for (int i = 0; i < length; i++)
                    serializer.SerializeValue(ref textureBytes[i]);
            }
        }
        else
        {
            int length = 0;
            serializer.SerializeValue(ref length);
            textureBytes = new byte[length];
            for (int i = 0; i < length; i++)
                serializer.SerializeValue(ref textureBytes[i]);
        }
    }

    public static MaterialData FromMaterial(Material mat)
    {
        var data = new MaterialData
        {
            shaderName = mat.shader?.name ?? "Standard",
            color = mat.HasProperty("_Color") ? mat.color : Color.white,
            metallic = mat.HasProperty("_Metallic") ? mat.GetFloat("_Metallic") : 0f,
            smoothness = mat.HasProperty("_Glossiness") ? mat.GetFloat("_Glossiness") : 0f
        };

        if (mat.HasProperty("_MainTex") && mat.mainTexture is Texture2D tex)
        {
            data.textureBytes = tex.EncodeToPNG(); // podés usar EncodeToJPG si querés más liviano
        }

        return data;
    }

    public Material ToMaterial()
    {
        Shader shader = Shader.Find(shaderName) ?? Shader.Find("Standard");
        Material mat = new Material(shader)
        {
            color = color
        };
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);

        if (textureBytes != null && textureBytes.Length > 0)
        {
            Texture2D tex = new Texture2D(2, 2); // tamaño temporal
            tex.LoadImage(textureBytes); // reconstruye la imagen
            mat.mainTexture = tex;
        }

        return mat;
    }
}
