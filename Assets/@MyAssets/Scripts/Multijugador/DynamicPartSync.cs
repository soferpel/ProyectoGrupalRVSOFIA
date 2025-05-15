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
        byte[] meshData = null;
        MaterialData matData = null;

        if (part.TryGetComponent<MeshFilter>(out MeshFilter mf))
        {
            meshData = YourMeshUtility.SerializeMesh(mf.sharedMesh);
            matData = MaterialData.FromMaterial(part.GetComponent<Renderer>().sharedMaterial);
            SpawnCutPieceClientRpc(containerRef, meshData, matData, part.transform.localPosition, part.transform.localRotation,tag, false);
        }
        else
        {
            Transform humanMesh = part.transform.Find("human_mesh");
            Debug.Log("INTENTANDO CORTAR CUERPo");
            if(IsServer && humanMesh != null && humanMesh.TryGetComponent<SkinnedMeshRenderer>(out SkinnedMeshRenderer childMF))
            {
                Debug.Log("INTENTANDO CORTAR CUERPO DENTRO DE IF");
                meshData = YourMeshUtility.SerializeMesh(childMF.sharedMesh);
                matData = MaterialData.FromMaterial(humanMesh.GetComponent<Renderer>().sharedMaterial);
                SpawnCutPieceClientRpc(containerRef, meshData, matData, part.transform.localPosition, part.transform.localRotation,tag, true);
            }
        }

    }

    [ClientRpc]
    private void SpawnCutPieceClientRpc(NetworkObjectReference containerRef, byte[] meshData, MaterialData matData, Vector3 partPosition, Quaternion partRotation,string tag, bool isBody)
    {
        if (containerRef.TryGet(out NetworkObject containerNetObj))
        {
            GameObject container = containerNetObj.gameObject;

            // Deserializás el mesh
            Mesh mesh = YourMeshUtility.DeserializeMesh(meshData);

            // Buscás el material (asegurate de tenerlo cargado en los clientes)
            Material mat = matData.ToMaterial();

            // Ahora creás la pieza dinámica
            GameObject part = new GameObject("Part"+ IsServer+" spawn");
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
            if (!isBody)
            {
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
            else
            {
                part.transform.localScale = new Vector3(100,100,100);
                part.transform.localPosition = new Vector3(0,0,0);
                foreach (Transform child in this.part.transform)
                {
                    Debug.Log("Hijo: " + child.name);
                }
                Transform humanMesh = this.part.transform.Find("human_mesh");
                if (humanMesh != null)
                {
                    Debug.Log("ENCONTRADO HUMAN MESH");
                    Transform cloth = humanMesh.transform.Find("cloth");
                    if (cloth != null)
                    {
                    Debug.Log("ENCONTRADO CLOTH");
                        foreach(Transform child2 in cloth.transform)
                        {
                    Debug.Log("CHILD2: "+child2.name);
                            if (child2.gameObject.activeSelf)
                            {
                                Debug.Log("ESTA ACTIVO");
                                if (child2.TryGetComponent<SkinnedMeshRenderer>(out SkinnedMeshRenderer childMF))
                                {
                                Debug.Log("TIENE SKINNED");

                                    bool needsCollider = false;
                                    Vector3 childPosition = child2.transform.localPosition;
                                    Quaternion childRotation = child2.transform.localRotation;
                                    byte[] meshd = YourMeshUtility.SerializeMesh((childMF.sharedMesh));
                                    MaterialData matd = MaterialData.FromMaterial(child2.GetComponent<Renderer>().sharedMaterial);
                                    SpawnCutPieceClothesClientRpc(containerRef, meshd, matd, needsCollider, childPosition, childRotation, tag);
                                }
                                else
                                {
                                Debug.Log("NOOOO TIENE SKINNED");
                                    foreach (Transform child3 in child2.transform)
                                    {
                    Debug.Log("CHILD3: "+child3.name);
                                        if (child3.gameObject.activeSelf && child3.TryGetComponent<SkinnedMeshRenderer>(out SkinnedMeshRenderer child3MF))
                                        {
                                Debug.Log("CHILD3 TIENE SKINNED");
                                            bool needsCollider = false;
                                            Vector3 childPosition = child3.transform.localPosition;
                                            Quaternion childRotation = child3.transform.localRotation;
                                            byte[] meshd = YourMeshUtility.SerializeMesh((child3MF.sharedMesh));
                                            MaterialData matd = MaterialData.FromMaterial(child3.GetComponent<Renderer>().sharedMaterial);


                                            if (containerRef.TryGet(out NetworkObject containerNetObj2))
                                            {

                                                GameObject container2 = containerNetObj2.gameObject.transform.GetChild(0).gameObject;

                                                // Deserializás el mesh
                                                Mesh mesh2 = YourMeshUtility.DeserializeMesh(meshd);

                                                // Buscás el material (asegurate de tenerlo cargado en los clientes)
                                                Material mat2 = matd.ToMaterial();

                                                // Ahora creás la pieza dinámica
                                                GameObject part2 = new GameObject("Clothes" + IsServer + " spawn");
                                                part2.transform.SetParent(container2.transform.parent);
                                                part2.transform.localPosition = new Vector3(childPosition.y,-childPosition.x,childPosition.z);
                                                part2.transform.localRotation = Quaternion.Euler(0, 90, -90);
                                                part2.transform.localScale = new Vector3(100,100,100);
                                                part2.transform.SetParent(container2.transform);
                                                part2.tag = tag;
                                                container2.layer = LayerMask.NameToLayer("BodyParts");
                                                // Le ponés componentes
                                                var filter2 = part2.AddComponent<MeshFilter>();
                                                filter2.sharedMesh = mesh2;

                                                var renderer2 = part2.AddComponent<MeshRenderer>();
                                                if (mesh2.subMeshCount > 1)
                                                {
                                                    renderer2.materials = new Material[] { mat2, bloodMaterial };
                                                }
                                                else
                                                {
                                                    renderer2.material = mat2;
                                                }
                                                if (needsCollider)
                                                {
                                                    var collider2 = part2.AddComponent<MeshCollider>();
                                                    collider2.sharedMesh = mesh2;
                                                    collider2.convex = true;

                                                    XRGrabInteractable grabInteractable2 = containerNetObj2.gameObject.GetComponent<XRGrabInteractable>();
                                                    grabInteractable2.enabled = false;
                                                    grabInteractable2.colliders.Add(collider2);
                                                    grabInteractable2.enabled = true;
                                                }
                                            }
                                            //SpawnCutPieceClothesClientRpc(containerRef, meshd, matd, needsCollider, childPosition, childRotation, tag);
                                        }
                                    }

                                }
                            }
                        }

                    }
                }
                part.transform.localPosition = new Vector3(-1.043f, 0, 0);

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
            GameObject part = new GameObject("Clothes" + IsServer + " spawns");
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
