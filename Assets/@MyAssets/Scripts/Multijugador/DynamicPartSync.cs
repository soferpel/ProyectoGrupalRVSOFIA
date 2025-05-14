using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DynamicPartSync : NetworkBehaviour
{
    public GameObject part;
    public Material bloodMaterial;
    public void setPart(GameObject part, string tag)
    {
        this.part = part;
        NetworkObjectReference containerRef = gameObject.GetComponent<NetworkObject>();
        byte[] meshData = YourMeshUtility.SerializeMesh(part.GetComponent<MeshFilter>().sharedMesh);
        MaterialData matData = MaterialData.FromMaterial(part.GetComponent<Renderer>().sharedMaterial);
        SpawnCutPieceClientRpc(containerRef, meshData, matData, part.transform.localPosition, part.transform.localRotation,tag);
    }

    [ClientRpc]
    private void SpawnCutPieceClientRpc(NetworkObjectReference containerRef, byte[] meshData, MaterialData matData, Vector3 partPosition, Quaternion partRotation,string tag)
    {
        if (containerRef.TryGet(out NetworkObject containerNetObj))
        {
            GameObject container = containerNetObj.gameObject;

            // Deserializás el mesh
            Mesh mesh = YourMeshUtility.DeserializeMesh(meshData);

            // Buscás el material (asegurate de tenerlo cargado en los clientes)
            Material mat = matData.ToMaterial();

            // Ahora creás la pieza dinámica
            GameObject part = new GameObject("Part");
            part.transform.SetParent(container.transform);
            part.transform.localPosition = partPosition;
            part.transform.localRotation = partRotation;
            part.transform.localScale = Vector3.one;
            part.tag = tag;
            // Le ponés componentes
            var filter = part.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            var collider = part.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
            collider.convex = true;

            var renderer = part.AddComponent<MeshRenderer>();
            if (mesh.subMeshCount > 1)
            {
                renderer.materials = new Material[] { mat, bloodMaterial };
            }
            else
            {
                renderer.material = mat;
            }

            container.layer = LayerMask.NameToLayer("BodyParts");

            XRGrabInteractable grabInteractable = container.GetComponent<XRGrabInteractable>();
            if (grabInteractable == null)
            {
                Debug.Log("Added XRGrabInteractable component.");
            }

            grabInteractable.enabled = false;
            grabInteractable.colliders.Add(collider);
            grabInteractable.enabled = true;

            if (part == null) return;
            foreach (Transform child in this.part.transform)
            {
                bool needsCollider = false;
                if (child.name.Equals("meshCloth"))
                {
                    needsCollider = true;
                }

                Vector3 childPosition = child.transform.localPosition;
                Quaternion childRotation = child.transform.localRotation;
                byte[] childMeshData = YourMeshUtility.SerializeMesh(child.GetComponent<MeshFilter>().sharedMesh);

                MaterialData childMatData = MaterialData.FromMaterial(child.GetComponent<Renderer>().sharedMaterial);

                SpawnCutPieceClothesClientRpc(containerRef, childMeshData, childMatData, needsCollider, childPosition, childRotation,tag);
            }
        }
    }
    [ClientRpc]
    private void SpawnCutPieceClothesClientRpc(NetworkObjectReference containerRef, byte[] meshData, MaterialData matData, bool needsCollider, Vector3 position, Quaternion rotation, string tag)
    {
        Debug.Log("LLAMA");
        if (containerRef.TryGet(out NetworkObject containerNetObj))
        {

            GameObject container = containerNetObj.gameObject.transform.GetChild(0).gameObject;

            // Deserializás el mesh
            Mesh mesh = YourMeshUtility.DeserializeMesh(meshData);

            // Buscás el material (asegurate de tenerlo cargado en los clientes)
            Material mat = matData.ToMaterial();

            // Ahora creás la pieza dinámica
            GameObject part = new GameObject("Clothes");
            part.transform.SetParent(container.transform);
            part.transform.localPosition = position;
            part.transform.localRotation = rotation;
            part.transform.localScale = Vector3.one;
            part.tag = tag;
            container.layer = LayerMask.NameToLayer("BodyParts");
            // Le ponés componentes
            var filter = part.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            var renderer = part.AddComponent<MeshRenderer>();
            if (mesh.subMeshCount > 1)
            {
                renderer.materials = new Material[] { mat, bloodMaterial };
            }
            else
            {
                renderer.material = mat;
            }
            if (needsCollider)
            {
                var collider = part.AddComponent<MeshCollider>();
                collider.sharedMesh = mesh;
                collider.convex = true;

                XRGrabInteractable grabInteractable = containerNetObj.gameObject.GetComponent<XRGrabInteractable>();
                grabInteractable.enabled = false;
                grabInteractable.colliders.Add(collider);
                grabInteractable.enabled = true;
            }
        }
    }
}
