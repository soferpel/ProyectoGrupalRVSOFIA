using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using Unity.Netcode;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public class SliceObjectMP : NetworkBehaviour
{
    public Transform startSlicePoint;
    public Transform endSlicePoint;
    public VelocityEstimator velocityEstimator;
    public float cutForce;
    public LayerMask sliceableLayer;
    public Material bloodMaterial;

    private GameObject collisionCut;
    private SliceablePartController collisionCutComponents;
    public float umbralDistancia = 0.1f;
    public bool hasCut = false;
    public UnityEvent OnCutMade;
    public AudioSource audioSource;


    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enabled)
        {
            collisionCut = other.gameObject;
            if (other.gameObject.layer == LayerMask.NameToLayer("Sliceable"))
            {
                Debug.Log("trigger enter");
                Debug.Log("ha cortado" + collisionCut.name);
                if (collisionCut.TryGetComponent(out SliceablePartController partController) && partController.target != null)
                {
                    collisionCutComponents = partController;
                    Slice(partController.target, partController);
                }
                collisionCutComponents = null;
                collisionCut = null;

            }
        }
    }

    public void Slice(GameObject target, SliceablePartController partController)
    {
        Vector3 velocity = velocityEstimator.GetVelocityEstimate();
        Vector3 planeNormal = Vector3.Cross(endSlicePoint.position - startSlicePoint.position, velocity);
        planeNormal.Normalize();
        planeNormal = gameObject.transform.right;
        float alignment = Vector3.Dot(planeNormal, partController.gameObject.transform.up);

        if (Mathf.Abs(alignment) < 0.85f)
        {
            return;
        }
        audioSource.Play();

        SkinnedMeshRenderer skinnedMeshRenderer = target.GetComponent<SkinnedMeshRenderer>();
        GameObject tempObject = new GameObject("TempSlicingObject");
        tempObject.transform.position = target.transform.position;
        tempObject.transform.rotation = target.transform.rotation;
        tempObject.transform.localScale = new Vector3(1, 1, 1);

        MeshFilter tempMeshFilter = tempObject.AddComponent<MeshFilter>();
        MeshRenderer tempMeshRenderer = tempObject.AddComponent<MeshRenderer>();

        Mesh bakedMesh = new Mesh();
        if (skinnedMeshRenderer != null)
        {
            skinnedMeshRenderer.BakeMesh(bakedMesh);
            tempMeshFilter.mesh = bakedMesh;
            tempMeshRenderer.materials = skinnedMeshRenderer.sharedMaterials;
        }
        else
        {
            tempMeshFilter.mesh = target.GetComponent<MeshFilter>().mesh;
            tempMeshRenderer.materials = target.GetComponent<MeshRenderer>().materials;
        }

        GameObject outBoundPart = GetSkinnedSubMesh(tempMeshFilter.sharedMesh, collisionCutComponents.ConvertCollidersToBounds(), skinnedMeshRenderer, false, tempMeshFilter, partController.boneNames);

        GameObject inBoundMesh = GetSkinnedSubMesh(tempMeshFilter.sharedMesh, collisionCutComponents.ConvertCollidersToBounds(), skinnedMeshRenderer, true, tempMeshFilter, partController.boneNames);

        if (inBoundMesh == null)
        {
            return;
        }

        if (outBoundPart != null)
        {
            SkinnedMeshRenderer outBoundSkinnedMeshRenderer = outBoundPart.GetComponent<SkinnedMeshRenderer>();
            if (outBoundSkinnedMeshRenderer != null && skinnedMeshRenderer != null)
            {
                skinnedMeshRenderer.sharedMesh = outBoundSkinnedMeshRenderer.sharedMesh;
                skinnedMeshRenderer.sharedMaterials = outBoundSkinnedMeshRenderer.sharedMaterials;
            }
        }

        if (outBoundPart != null)
        {
            Destroy(outBoundPart);
        }

        SkinnedMeshRenderer skinnedMeshRenderer2 = inBoundMesh.GetComponent<SkinnedMeshRenderer>();
        GameObject tempObject2 = new GameObject("TempSlicingObject2");
        tempObject2.transform.position = inBoundMesh.transform.position;
        tempObject2.transform.rotation = inBoundMesh.transform.rotation;
        tempObject2.transform.localScale = new Vector3(1, 1, 1);

        MeshFilter tempMeshFilter2 = tempObject2.AddComponent<MeshFilter>();
        MeshRenderer tempMeshRenderer2 = tempObject2.AddComponent<MeshRenderer>();
        Mesh bakedMesh2 = new Mesh();

        skinnedMeshRenderer2.BakeMesh(bakedMesh2);
        tempMeshFilter2.mesh = bakedMesh2;
        tempMeshRenderer2.materials = skinnedMeshRenderer2.sharedMaterials;

        SlicedHull hull = tempObject2.Slice(gameObject.transform.position, gameObject.transform.right.normalized);
        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(tempObject2, bloodMaterial);
            GameObject lowerHull = hull.CreateLowerHull(tempObject2, bloodMaterial);
            if (upperHull != null && lowerHull != null)
            {

                float upperDistance = CalculateAverageDistance(upperHull, collisionCutComponents.attachPoint.transform.position);
                float lowerDistance = CalculateAverageDistance(lowerHull, collisionCutComponents.attachPoint.transform.position);

                GameObject detachPart = (upperDistance < lowerDistance) ? lowerHull : upperHull;
                SetupSlicedComponent(detachPart);

                foreach (GameObject clothes in collisionCutComponents.clothes)
                {
                    foreach (Transform child in clothes.transform)
                    {
                        if (child.gameObject.activeSelf)
                        {
                            SliceClothes(child.gameObject, planeNormal, detachPart);
                        }
                    }
                }
                foreach (GameObject clothes in collisionCutComponents.clothesChild)
                {
                    foreach (Transform child in clothes.transform)
                    {
                        if (child.gameObject.activeSelf)
                        {
                            SkinnedMeshRenderer skinnedMeshRenderer3 = child.GetComponent<SkinnedMeshRenderer>();
                            GameObject tempObject3 = new GameObject("meshCloth");
                            tempObject3.layer = LayerMask.NameToLayer("BodyParts");

                            tempObject3.transform.position = child.transform.position;
                            tempObject3.transform.rotation = child.transform.rotation;
                            tempObject3.transform.localScale = new Vector3(1, 1, 1);

                            MeshFilter tempMeshFilter3 = tempObject3.AddComponent<MeshFilter>();
                            MeshRenderer tempMeshRenderer3 = tempObject3.AddComponent<MeshRenderer>();
                            Mesh bakedMesh3 = new Mesh();

                            skinnedMeshRenderer3.BakeMesh(bakedMesh3);
                            tempMeshFilter3.mesh = bakedMesh3;
                            tempMeshRenderer3.materials = skinnedMeshRenderer3.sharedMaterials;
                            Destroy(child.gameObject);
                            tempObject3.transform.SetParent(detachPart.transform);
                            MeshCollider collider = tempObject3.AddComponent<MeshCollider>();
                            collider.convex = true;
                            /*XRGrabInteractable grabInteractable = detachPart.transform.parent.GetComponent<XRGrabInteractable>();
                            grabInteractable.trackScale = false;
                            grabInteractable.enabled = false;
                            grabInteractable.colliders.Add(collider);
                            grabInteractable.enabled = true;*/
                        }
                    }
                }
                DetachPart(detachPart);

                GameObject connectedPart = (detachPart == lowerHull) ? upperHull : lowerHull;
                AttachToBody(connectedPart);


                foreach (Collider collider in collisionCutComponents.boundsColliders)
                {
                    if (!collider.Equals(collisionCutComponents.gameObject.GetComponent<Collider>()))
                    {
                        collider.gameObject.SetActive(false);
                    }
                    else
                    {
                        Destroy(collider.gameObject.GetComponent<XRGrabInteractable>());
                        Destroy(collider.gameObject.GetComponent<CharacterJoint>());

                        Destroy(collider.attachedRigidbody);
                        Destroy(collider);
                    }
                }
            }
            Destroy(tempObject2);
            Destroy(inBoundMesh);
        }
        else
        {
            SetupSlicedComponent(tempObject2);
            DetachPart(tempObject2);
            Destroy(inBoundMesh);
        }

        Destroy(tempObject);
        collisionCutComponents.client.countBodyParts--;
        if (collisionCutComponents.client.countBodyParts <= 0)
        {
            collisionCutComponents.client.body.tag = "Torso";


            DetachBody(collisionCutComponents.client.body);

        }
        Destroy(partController);

        Debug.Log("Se ha cortado parte del cuerpo");
        hasCut = true;
        OnCutMade?.Invoke();
    }

    private void DetachBody(GameObject part)
    {
        XRGrabInteractable partGrab = (part.GetComponent<XRGrabInteractable>());
        partGrab.trackScale = false;
        partGrab.enabled = false;
        partGrab.colliders.Clear();

        partGrab.enabled = true;
        Destroy(partGrab);
        if (part == null)
        {
            Debug.LogError("El objeto 'part' es nulo. No se puede proceder con DetachPart.");
            return;
        }
        GameObject container = new GameObject($"{part.name}_Centered");
        if (collisionCutComponents.client.TryGetComponent<BodyPartController>(out BodyPartController clientbpc))
        {
            BodyPartController bodypartController = container.AddComponent<BodyPartController>();
            bodypartController.decayTime = clientbpc.decayTimer;
            bodypartController.materials = clientbpc.materials;
        }
        container.tag = "Torso";
        Vector3 containerPosition = part.transform.position;
        Collider partCollider = part.GetComponent<Collider>();

        if (partCollider != null)
        {
            containerPosition = partCollider.bounds.center;
        }
        else
        {
            Debug.LogWarning($"El objeto {part.name} y sus hijos no tienen Collider. Usando la posición original del objeto.");
        }
        container.transform.position = containerPosition;
        container.transform.rotation = part.transform.rotation;

        Vector3 offset = part.transform.position - containerPosition;
        GameObject oldParent = part.transform.parent.parent.gameObject;
        part.transform.SetParent(container.transform);
        oldParent.transform.GetChild(0).SetParent(part.transform);
        Destroy(oldParent);
        Debug.Log(part.transform.localPosition + " offset " + offset);
        part.transform.localPosition = new Vector3(0, 0, 0.3f);

        part.transform.localRotation *= Quaternion.Euler(0, 90, -90);
        Rigidbody partRb = part.GetComponent<Rigidbody>();
        if (partRb != null)
        {
            partRb.isKinematic = true;
        }
        container.AddComponent<DetectableTarget>();
        XRGrabInteractable grabInteractable = container.AddComponent<XRGrabInteractable>();
        grabInteractable.colliders.Add(partCollider);
        grabInteractable.trackScale = false;
        grabInteractable.useDynamicAttach = true;
        grabInteractable.interactionLayers = InteractionLayerMask.GetMask("Default", "BodyParts");
        container.layer = LayerMask.NameToLayer("BodyParts");
        container.AddComponent<BodyPartScaler>();


        if (partRb != null)
        {
            Destroy(partRb);
        }

    }
    public void SetupSlicedComponent(GameObject slicedObject)
    {
        Rigidbody rg = slicedObject.AddComponent<Rigidbody>();
        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();
        collider.convex = true;
        rg.AddExplosionForce(cutForce, slicedObject.transform.position, 1);
    }
    public void SliceClothes(GameObject target, Vector3 planeNormal, GameObject hullParent)
    {
        SkinnedMeshRenderer skinnedMeshRenderer = target.GetComponent<SkinnedMeshRenderer>();
        GameObject tempObject = new GameObject("TempSlicingObject");
        tempObject.transform.position = target.transform.position;
        tempObject.transform.rotation = target.transform.rotation;
        tempObject.transform.localScale = new Vector3(1, 1, 1);

        MeshFilter tempMeshFilter = tempObject.AddComponent<MeshFilter>();
        MeshRenderer tempMeshRenderer = tempObject.AddComponent<MeshRenderer>();

        Mesh bakedMesh = new Mesh();
        if (skinnedMeshRenderer != null)
        {
            skinnedMeshRenderer.BakeMesh(bakedMesh);
            tempMeshFilter.mesh = bakedMesh;
            tempMeshRenderer.materials = skinnedMeshRenderer.sharedMaterials;
        }
        else
        {
            tempMeshFilter.mesh = target.GetComponent<MeshFilter>().mesh;
            tempMeshRenderer.materials = target.GetComponent<MeshRenderer>().materials;
        }

        GameObject outBoundPart = GetSkinnedSubMesh(tempMeshFilter.sharedMesh, collisionCutComponents.ConvertCollidersToBounds(), skinnedMeshRenderer, false, tempMeshFilter, collisionCutComponents.boneNames);

        GameObject inBoundMesh = GetSkinnedSubMesh(tempMeshFilter.sharedMesh, collisionCutComponents.ConvertCollidersToBounds(), skinnedMeshRenderer, true, tempMeshFilter, collisionCutComponents.boneNames);

        if (inBoundMesh == null)
        {
            return;
        }

        if (outBoundPart != null)
        {
            SkinnedMeshRenderer outBoundSkinnedMeshRenderer = outBoundPart.GetComponent<SkinnedMeshRenderer>();
            if (outBoundSkinnedMeshRenderer != null && skinnedMeshRenderer != null)
            {
                skinnedMeshRenderer.sharedMesh = outBoundSkinnedMeshRenderer.sharedMesh;
                skinnedMeshRenderer.sharedMaterials = outBoundSkinnedMeshRenderer.sharedMaterials;
            }
        }

        if (outBoundPart != null)
        {
            Destroy(outBoundPart);
        }

        SkinnedMeshRenderer skinnedMeshRenderer2 = inBoundMesh.GetComponent<SkinnedMeshRenderer>();
        GameObject tempObject2 = new GameObject("TempSlicingObject2");
        tempObject2.transform.position = inBoundMesh.transform.position;
        tempObject2.transform.rotation = inBoundMesh.transform.rotation;
        tempObject2.transform.localScale = new Vector3(1, 1, 1);

        MeshFilter tempMeshFilter2 = tempObject2.AddComponent<MeshFilter>();
        MeshRenderer tempMeshRenderer2 = tempObject2.AddComponent<MeshRenderer>();
        Mesh bakedMesh2 = new Mesh();

        skinnedMeshRenderer2.BakeMesh(bakedMesh2);
        tempMeshFilter2.mesh = bakedMesh2;
        tempMeshRenderer2.materials = skinnedMeshRenderer2.sharedMaterials;

        SlicedHull hull = tempObject2.Slice(endSlicePoint.position, planeNormal);
        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(tempObject2, bloodMaterial);
            GameObject lowerHull = hull.CreateLowerHull(tempObject2, bloodMaterial);

            if (upperHull != null && lowerHull != null)
            {

                float upperDistance = CalculateAverageDistance(upperHull, collisionCutComponents.attachPoint.transform.position);
                float lowerDistance = CalculateAverageDistance(lowerHull, collisionCutComponents.attachPoint.transform.position);

                GameObject detachPart = (upperDistance < lowerDistance) ? lowerHull : upperHull;
                detachPart.transform.SetParent(hullParent.transform);

                GameObject connectedPart = (detachPart == lowerHull) ? upperHull : lowerHull;
                AttachToBody(connectedPart);

            }
            Destroy(tempObject2);

        }
        else
        {
            tempObject2.transform.SetParent(hullParent.transform);
        }
        Destroy(inBoundMesh);
        Destroy(tempObject);

    }

    private float CalculateAverageDistance(GameObject part, Vector3 referencePoint)
    {
        MeshFilter meshFilter = part.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.mesh == null) return float.MaxValue;

        Vector3[] vertices = meshFilter.mesh.vertices;
        float totalDistance = 0;

        foreach (Vector3 vertex in vertices)
        {
            Vector3 worldVertex = part.transform.TransformPoint(vertex);
            totalDistance += Vector3.Distance(worldVertex, referencePoint);
        }

        return totalDistance / vertices.Length;
    }

    private void AttachToBody(GameObject part)
    {
        part.transform.SetParent(collisionCut.transform);
    }
    


    [SerializeField] private GameObject runtimePartContainerPrefab;
    [SerializeField] private GameObject partPrefab;

    private void DetachPart(GameObject part)
    {
        Debug.Log("DENTRO METODO DETACH");
        if (part == null)
        {
            Debug.LogError("El objeto 'part' es nulo. No se puede proceder con DetachPart.");
            return;
        }
        part.tag = collisionCutComponents.gameObjectTag;
        part.layer = LayerMask.NameToLayer("BodyParts");

        GameObject container = new GameObject($"{part.name}_Centered");

        container.tag = part.tag;
        Vector3 containerPosition = part.transform.position;
        Collider partCollider = part.GetComponent<Collider>();

        if (partCollider == null)
        {
            partCollider = part.GetComponentInChildren<Collider>();
        }

        if (partCollider != null)
        {
            containerPosition = partCollider.bounds.center;
        }
        else
        {
            Debug.LogWarning($"El objeto {part.name} y sus hijos no tienen Collider. Usando la posición original del objeto.");
        }
        container.transform.position = containerPosition;
        Quaternion containerRotation = part.transform.rotation;
        container.transform.rotation = part.transform.rotation;
        container.transform.localScale = Vector3.one;

        Vector3 offset = part.transform.position - containerPosition;
        part.transform.SetParent(container.transform);
        Vector3 partPosition = offset;
        part.transform.localPosition = offset;
        Quaternion partRotation = Quaternion.identity;
        part.transform.localRotation = Quaternion.identity;
        part.transform.localScale = Vector3.one;

        Rigidbody partRb = part.GetComponent<Rigidbody>();
        if (partRb != null)
        {
            partRb.isKinematic = true;
        }

        
        container.AddComponent<DetectableTarget>();
        XRGrabInteractable grabInteractable = container.AddComponent<XRGrabInteractable>();
        grabInteractable.trackScale = false;
        grabInteractable.useDynamicAttach = true;
        grabInteractable.interactionLayers = InteractionLayerMask.GetMask("Default", "BodyParts");
        container.layer = LayerMask.NameToLayer("BodyParts");
        container.AddComponent<BodyPartScaler>();

        
        if (partRb != null)
        {
            Destroy(partRb);
        }
        

        if (collisionCutComponents.client.TryGetComponent<BodyPartController>(out BodyPartController clientbpc))
        {
            BodyPartController bodypartController = container.AddComponent<BodyPartController>();
            bodypartController.decayTime = clientbpc.decayTimer;
            bodypartController.materials = clientbpc.materials;
        }
        //NetworkObject no = container.AddComponent<NetworkObject>();
        //no.Spawn();
        //NetworkObjectReference nor = new NetworkObjectReference(no);
        //SpawnPartRpc(nor, new RpcParams());

        sendPart = part;


        List<byte[]> meshDatas = new List<byte[]>();
        List<MaterialData> materialDatas = new List<MaterialData>();
        List<bool> needsCollider = new List<bool>();
        List<Vector3> positions = new List<Vector3>();
        List<Quaternion> rotations = new List<Quaternion>();
        // Primero, el mesh del padre
        MeshFilter parentFilter = part.GetComponent<MeshFilter>();
        if (parentFilter != null && parentFilter.sharedMesh != null)
        {
            meshDatas.Add(YourMeshUtility.SerializeMesh(parentFilter.sharedMesh));
        }
        materialDatas.Add(MaterialData.FromMaterial(part.GetComponent<Renderer>().sharedMaterial));
        needsCollider.Add(false);
        positions.Add(part.transform.localPosition);
        rotations.Add(part.transform.localRotation);
        // Ahora recorremos los hijos
        foreach (Transform child in part.transform)
        {
            if (child.name.Equals("meshCloth"))
            {
                needsCollider.Add(true);
                positions.Add(child.transform.localPosition);
                rotations.Add(child.transform.localRotation);
            }
            else {
                needsCollider.Add(false);
                positions.Add(child.transform.localPosition);
                rotations.Add(child.transform.localRotation);
            }
            MeshFilter childFilter = child.GetComponent<MeshFilter>();
            if (childFilter != null && childFilter.sharedMesh != null)
            {
                Debug.Log("añade mesh");
                meshDatas.Add(YourMeshUtility.SerializeMesh(childFilter.sharedMesh));
            }
            materialDatas.Add(MaterialData.FromMaterial(child.GetComponent<Renderer>().sharedMaterial));
        }
        for (int i = 0; i < meshDatas.Count; i++)
        {
            Debug.Log("Mesh array " + i + ": " + meshDatas[i]);
        }
        byte[] meshData = YourMeshUtility.SerializeMesh(part.GetComponent<MeshFilter>().sharedMesh);
        MaterialData matData = MaterialData.FromMaterial(part.GetComponent<Renderer>().sharedMaterial);
        
        byte[][] meshDataArray = meshDatas.ToArray();
        for(int i = 0; i < meshDataArray.Length; i++)
        {
            Debug.Log("Mesh array " + i + ": " + meshDataArray[i]);
        }
        MaterialData[] materialDataArray = materialDatas.ToArray();
        bool[] needsColliderArray = needsCollider.ToArray();
        Vector3[] positionsArray = positions.ToArray();
        Quaternion[] rotationArray = rotations.ToArray();

        SpawnCutPieceServerRpc(containerPosition, containerRotation,/* meshDataArray, materialDataArray, needsColliderArray, positionsArray,rotationArray,*/ partPosition, partRotation);
        /*foreach (Transform child in part.transform)
        {
            meshData = YourMeshUtility.SerializeMesh(child.gameObject.GetComponent<MeshFilter>().sharedMesh);
            matData = MaterialData.FromMaterial(child.gameObject.GetComponent<Renderer>().sharedMaterial);
            SpawnCutPieceClothesClientRpc(containerRef, meshData, matData);
        }*/
        ////////
        ///
        /*
        MeshFilter meshFilter = part.GetComponent<MeshFilter>();
        Material material = part.GetComponent<Renderer>().sharedMaterial;
        if (meshFilter != null)
        {
            // Registramos el Mesh (esto te da un ID único)
            int meshId = RegisterMesh(meshFilter.sharedMesh);
            int materialId = RegisterMaterial(material);
            // Enviamos el RPC con los datos al servidor
            SpawnRpc(partCollider.bounds.center, part.transform.rotation, SerializeMesh(meshFilter.sharedMesh), materialId, part.transform.position, new RpcParams());
        }*/

    }
    private GameObject sendPart;
    [SerializeField] private GameObject dynamicContainerPrefab;
    [ServerRpc(RequireOwnership = false)]
    public void SpawnCutPieceServerRpc(Vector3 position, Quaternion rotation, /*byte[][] meshData, MaterialData[] matData, bool[] needsCollider,Vector3[] positions, Quaternion[] rotations,*/Vector3 partPosition, Quaternion partRotation)
    {
        //for (int i = 0; i < meshData.Length; i++)
        //{
        //    Debug.Log("Mesh server " + i + ": " + meshData[i]);
        //}
        // Instanciás el contenedor registrado
        GameObject container = Instantiate(dynamicContainerPrefab, position, rotation);

        Rigidbody rb = container.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = container.AddComponent<Rigidbody>();
        }
        //rb.isKinematic = true;
        NetworkObject netObj = container.GetComponent<NetworkObject>();
        netObj.Spawn(true); // Spawn en la red
        // Preparás los datos dinámicos


        container.GetComponent<DynamicPartSync>().bloodMaterial = bloodMaterial;
        container.GetComponent<DynamicPartSync>().setPart(sendPart);


        NetworkObjectReference containerRef = container.GetComponent<NetworkObject>();
        //SpawnCutPieceClientRpc(containerRef, meshData, matData,needsCollider,positions, rotations, partPosition, partRotation);
        // Le mandás al cliente la orden de construir su parte local
        //container.GetComponent<DynamicPartSync>().InitServer(meshData, materialId);
    }
    [ClientRpc]
    private void SpawnCutPieceClientRpc(NetworkObjectReference containerRef, byte[][] meshData, MaterialData[] matData,bool[] needsCollider,Vector3[] positions,Quaternion[] rotations, Vector3 partPosition, Quaternion partRotation)
    {
        if (containerRef.TryGet(out NetworkObject containerNetObj))
        {
            GameObject container = containerNetObj.gameObject;

            // Deserializás el mesh
            Mesh mesh = YourMeshUtility.DeserializeMesh(meshData[0]);

            // Buscás el material (asegurate de tenerlo cargado en los clientes)
            Material mat = matData[0].ToMaterial();

            // Ahora creás la pieza dinámica
            GameObject part = new GameObject("Part");
            part.transform.SetParent(container.transform);
            part.transform.localPosition = partPosition;
            part.transform.localRotation = partRotation;
            part.transform.localScale = Vector3.one;

            // Le ponés componentes
            var filter = part.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            var collider = part.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
            collider.convex = true;

            var renderer = part.AddComponent<MeshRenderer>();
            renderer.material = mat;

            XRGrabInteractable grabInteractable = container.GetComponent<XRGrabInteractable>();
            if (grabInteractable == null)
            {
                Debug.Log("Added XRGrabInteractable component.");
            }

            grabInteractable.enabled = false;
            grabInteractable.colliders.Add(collider);
            grabInteractable.enabled = true;

            Debug.Log("COUNT: "+ meshData.Length + " " + matData.Length + " " + needsCollider.Length + " " + positions.Length + " " + rotations.Length + " ");

            for (int i = 0; i< meshData.Length; i++)
            {
                Debug.Log("MESH " + i + " : " + meshData[i]);
                if (i == 0) continue;
                SpawnCutPieceClothesClientRpc(containerRef, meshData[i], matData[i],needsCollider[i],positions[i],rotations[i]);
            }
            //NotifyObjectCreatedServerRpc(containerRef, NetworkManager.Singleton.LocalClientId);
        }
    }
    [ClientRpc]
    private void SpawnCutPieceClothesClientRpc(NetworkObjectReference containerRef, byte[] meshData, MaterialData matData, bool needsCollider,Vector3 position,Quaternion rotation)
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

            // Le ponés componentes
            var filter = part.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            var renderer = part.AddComponent<MeshRenderer>();
            if (mesh.subMeshCount > 1)
            {
                renderer.materials = new Material[] { mat, bloodMaterial };
            }else
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

    /*
    // Este método se llama después de que el cliente haya creado su objeto
    [ServerRpc(RequireOwnership = false)]
    public void NotifyObjectCreatedServerRpc(NetworkObjectReference containerRef, ulong clientId)
    {
        Debug.Log("METODO NotifyObjectCreatedServerRpc");
        // El servidor guarda el estado de que este cliente ha creado su objeto
        bool updated = false;

        // Buscamos si el cliente ya está en la lista
        for (int i = 0; i < clientsThatSpawnedObjectsList.Count; i++)
        {
            if (clientsThatSpawnedObjectsList[i].clientId == clientId)
            {
                // Si el cliente ya está en la lista, actualizamos el estado
                clientsThatSpawnedObjectsList[i] = new ClientSpawnStatus(clientId, true);
                updated = true;
                break;
            }
        }

        // Si no encontramos el cliente, lo agregamos
        if (!updated)
        {
            clientsThatSpawnedObjectsList.Add(new ClientSpawnStatus(clientId, true));
        }

        // Verifica si todos los clientes han creado su objeto
        CheckIfAllClientsHaveSpawned(containerRef);

    }

    private void CheckIfAllClientsHaveSpawned(NetworkObjectReference containerRef)
    {
        Debug.Log("METODO CheckIfAllClientsHaveSpawned");
        foreach (var clientStatus in clientsThatSpawnedObjectsList)
        {
            Debug.Log($"ClientId: {clientStatus.clientId}, HasSpawned: {clientStatus.hasSpawned}");
        }

        if (clientsThatSpawnedObjectsList.Count == NetworkManager.Singleton.ConnectedClientsList.Count)
        {
            // Comprobar si todos los clientes han spawnado su objeto
            bool allSpawned = true;

            foreach (var clientStatus in clientsThatSpawnedObjectsList)
            {
                if (!clientStatus.hasSpawned)
                {
                    allSpawned = false;
                    break;
                }
            }

            if (allSpawned)
            {
                // Si todos los clientes han spawnado su objeto, entonces procedemos
                ActivateGravityForAllClients(containerRef);
            }
        }
    }

    private void ActivateGravityForAllClients(NetworkObjectReference containerRef)
    {
        Debug.Log("METODO ActivateGravityForAllClients");
        // Aquí activamos la gravedad para todos los clientes
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            ActivateGravityClientRpc(client.ClientId, containerRef);
        }
    }

    [ClientRpc]
    private void ActivateGravityClientRpc(ulong clientId, NetworkObjectReference containerRef)
    {
        Debug.Log("METODO ActivateGravityClientRpc");

        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            if (containerRef.TryGet(out NetworkObject containerNetObj))
            {
                GameObject container = containerNetObj.gameObject;

                Debug.Log($"Activating gravity for container: {container.name}");

                // Asegúrate de que el Rigidbody esté presente
                Rigidbody rb = container.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Debug.Log("Rigidbody found. Activating gravity.");
                    rb.isKinematic = false; // Desactivar cinemática
                }
                else
                {
                    Debug.LogWarning("Rigidbody not found on container!");
                }

                // Comprobar si ya existen los componentes antes de añadirlos
               
            }
            else
            {
                Debug.LogWarning("Container not found in containerRef.");
            }
        }
    }

    [System.Serializable]
    public struct ClientSpawnStatus: INetworkSerializable, IEquatable<ClientSpawnStatus>
    {
        public ulong clientId;
        public bool hasSpawned;

        public ClientSpawnStatus(ulong clientId, bool hasSpawned)
        {
            this.clientId = clientId;
            this.hasSpawned = hasSpawned;
        }
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref hasSpawned);
        }
        public bool Equals(ClientSpawnStatus other)
        {
            return clientId == other.clientId && hasSpawned == other.hasSpawned;
        }

        public override bool Equals(object obj)
        {
            return obj is ClientSpawnStatus other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(clientId, hasSpawned);
        }
    }
    private NetworkList<ClientSpawnStatus> clientsThatSpawnedObjectsList = new NetworkList<ClientSpawnStatus>();
    */
















    [Rpc(SendTo.Everyone)]
    private void SpawnPartRpc(NetworkObjectReference objref, RpcParams rpcParams)
    {
        if(objref.TryGet(out NetworkObject netobj))
            {
            GameObject go = netobj.gameObject;
            NetworkManager.Singleton.AddNetworkPrefab(go);
            var prefabsList = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs;
            foreach (var prefab in prefabsList)
            {
                if (prefab.Prefab != null)
                {
                    Debug.Log($"{NetworkManager.Singleton.LocalClientId}  Prefab: {prefab.Prefab.name}");
                }
                else
                {
                    Debug.LogWarning("Hay un prefab nulo en la lista.");
                }
            }
            
        }
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

    private static Dictionary<int, Mesh> meshRegistry = new Dictionary<int, Mesh>();
    private static Dictionary<int, Material> materialRegistry = new Dictionary<int, Material>();

    private static int nextMeshId = 0;

    // Registrar un Mesh
    public static int RegisterMesh(Mesh mesh)
    {
        foreach (var item in meshRegistry)
        {
            if (item.Value == mesh) return item.Key;
        }

        int meshId = nextMeshId++;
        meshRegistry.Add(meshId, mesh);
        return meshId;
    }
    public static int RegisterMaterial(Material material)
    {
        foreach (var item in materialRegistry)
        {
            if (item.Value == material) return item.Key;
        }

        int materialId = nextMeshId++;
        materialRegistry.Add(materialId, material);
        return materialId;
    }
    private Mesh GetMeshFromRegistry(int meshId)
    {
        if (meshRegistry.TryGetValue(meshId, out Mesh mesh))
        {
            return mesh;
        }
        Debug.LogWarning("No se encontró el Mesh con ID: " + meshId);
        return null;
    }
    private Material GetMaterialFromRegistry(int materialId)
    {
        if (materialRegistry.TryGetValue(materialId, out Material material))
        {
            return material;
        }
        Debug.LogWarning("No se encontró el Material con ID: " + materialId);
        return null;
    }
    [Rpc(SendTo.Server)]
    private void SpawnRpc(Vector3 position, Quaternion rotation, byte[] meshData, int materialId, Vector3 partPosition, RpcParams rpcParams)
    {
        // Instanciamos el contenedor del objeto cortado
        GameObject container = Instantiate(runtimePartContainerPrefab, position, rotation);
        Mesh mesh = DeserializeMesh(meshData); // Deserializamos el Mesh
        GameObject part = Instantiate(partPrefab, position, rotation);
        container.GetComponent<Rigidbody>().isKinematic = true;
        // Vinculamos la parte al contenedor
        

        // Añadimos componentes de MeshFilter y MeshCollider
        MeshFilter meshFilter = part.GetComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        MeshCollider meshCollider = part.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = true;

        // Asignamos el material
        Material material = GetMaterialFromRegistry(materialId);
        MeshRenderer meshRenderer = part.GetComponent<MeshRenderer>();
        meshRenderer.material = material;

        // Añadimos componentes extras
        container.AddComponent<DetectableTarget>();
        XRGrabInteractable grabInteractable = container.AddComponent<XRGrabInteractable>();
        grabInteractable.trackScale = false;
        grabInteractable.useDynamicAttach = true;
        grabInteractable.interactionLayers = InteractionLayerMask.GetMask("Default", "BodyParts");
        container.layer = LayerMask.NameToLayer("BodyParts");
        container.AddComponent<BodyPartScaler>();

        // Inicializamos la sincronización
        var sync = container.GetComponent<CutPieceNetworkSync>();
        sync.Init(meshData, materialId);

        // Aquí añadimos el NetworkObject al `part` y lo spawneamos en la red
        NetworkObject networkObject = part.GetComponent<NetworkObject>(); // Asegúrate de que `part` tiene un `NetworkObject`
        networkObject.Spawn(true);  // Spawn para que Netcode lo gestione en todos los clientes

        // Spawneamos el contenedor en la red también
        container.GetComponent<NetworkObject>().Spawn(true);
        container.GetComponent<Rigidbody>().isKinematic = true;

        part.transform.SetParent(container.transform);
        Vector3 offset = partPosition - position;
        part.transform.localPosition = offset;
        part.transform.localRotation = Quaternion.identity;
        part.transform.localScale = Vector3.one;
    }


    public GameObject GetSkinnedSubMesh(Mesh originalMesh, Bounds[] bounds, SkinnedMeshRenderer originalSkinnedRenderer, bool inBound, MeshFilter originalMeshFilter, List<string> boneNames)
    {
        if (bounds == null || bounds.Length == 0)
        {
            return null;
        }

        Mesh originalSkinnedMesh = originalSkinnedRenderer.sharedMesh;

        Vector3[] notvertices = originalMesh.vertices;

        Vector3[] vertices = originalSkinnedMesh.vertices;
        int[] triangles = originalSkinnedMesh.triangles;
        Vector3[] normals = originalSkinnedMesh.normals;
        Vector2[] uvs = originalSkinnedMesh.uv;
        BoneWeight[] boneWeights = originalSkinnedMesh.boneWeights;
        Matrix4x4[] bindPoses = originalSkinnedMesh.bindposes;

        List<int> addedVertices = new List<int>();
        List<Vector3> filteredVertices = new List<Vector3>();
        List<int> filteredTriangles = new List<int>();
        List<Vector3> filteredNormals = new List<Vector3>();
        List<Vector2> filteredUVs = new List<Vector2>();
        List<BoneWeight> filteredBoneWeights = new List<BoneWeight>();

        bool IsVertexIncluded(Vector3 worldVertex, BoneWeight[] boneWeights, int vertexIndex, Transform[] bones, List<string> boneNames)
        {
            BoneWeight bw = boneWeights[vertexIndex];
            foreach (var bound in bounds)
            {
                if (bound.Contains(worldVertex))
                {
                    if (IsVertexAffectedByLeg(bw, bones, boneNames))
                    {

                        return inBound;
                    }
                    else
                    {
                        return !inBound;
                    }
                }

                else
                {
                    Vector3 puntoCercano = bound.ClosestPoint(worldVertex);

                    float distancia = Vector3.Distance(worldVertex, puntoCercano);

                    if (distancia <= umbralDistancia)
                    {
                        if (IsVertexAffectedByLeg(bw, bones, boneNames))
                        {

                            return inBound;
                        }
                        else
                        {
                            return !inBound;
                        }
                    }
                }
            }

            return !inBound;
        }
        bool IsVertexAffectedByLeg(BoneWeight bw, Transform[] bones, List<string> boneNamesPierna)
        {
            bool affectedByLeg = false;

            affectedByLeg |= boneNamesPierna.Contains(bones[bw.boneIndex0].name);
            affectedByLeg |= boneNamesPierna.Contains(bones[bw.boneIndex1].name);
            affectedByLeg |= boneNamesPierna.Contains(bones[bw.boneIndex2].name);
            affectedByLeg |= boneNamesPierna.Contains(bones[bw.boneIndex3].name);

            return affectedByLeg;
        }
        Vector3[] worldVertices = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            worldVertices[i] = originalMeshFilter.transform.TransformPoint(notvertices[i]);
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            if (IsVertexIncluded(worldVertices[i], boneWeights, i, originalSkinnedRenderer.bones, boneNames) && !addedVertices.Contains(i))
            {
                addedVertices.Add(i);
                filteredVertices.Add(vertices[i]);
                filteredNormals.Add(normals[i]);
                filteredUVs.Add(uvs[i]);

                if (i < boneWeights.Length)
                {
                    filteredBoneWeights.Add(boneWeights[i]);
                }
                else
                {
                    filteredBoneWeights.Add(new BoneWeight());
                }
            }
        }

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int idx1 = triangles[i];
            int idx2 = triangles[i + 1];
            int idx3 = triangles[i + 2];

            bool idx1InList = addedVertices.Contains(idx1);
            bool idx2InList = addedVertices.Contains(idx2);
            bool idx3InList = addedVertices.Contains(idx3);

            if (inBound && idx1InList && idx2InList && idx3InList)
            {
                filteredTriangles.Add(addedVertices.IndexOf(idx1));
                filteredTriangles.Add(addedVertices.IndexOf(idx2));
                filteredTriangles.Add(addedVertices.IndexOf(idx3));
            }
            else if (!inBound)
            {
                if (idx1InList || idx2InList || idx3InList)
                {
                    List<int> validIndices = new List<int>();

                    if (idx1InList) validIndices.Add(addedVertices.IndexOf(idx1));
                    else if (!IsVertexIncluded(worldVertices[idx1], boneWeights, idx1, originalSkinnedRenderer.bones, boneNames))
                    {
                        validIndices.Add(filteredVertices.Count);
                        filteredVertices.Add(vertices[idx1]);
                        filteredNormals.Add(normals[idx1]);
                        filteredUVs.Add(uvs[idx1]);
                        if (idx1 < boneWeights.Length)
                        {
                            filteredBoneWeights.Add(boneWeights[idx1]);
                        }
                        else
                        {
                            filteredBoneWeights.Add(new BoneWeight());
                        }
                    }

                    if (idx2InList) validIndices.Add(addedVertices.IndexOf(idx2));
                    else if (!IsVertexIncluded(worldVertices[idx2], boneWeights, idx2, originalSkinnedRenderer.bones, boneNames))
                    {
                        validIndices.Add(filteredVertices.Count);
                        filteredVertices.Add(vertices[idx2]);
                        filteredNormals.Add(normals[idx2]);
                        filteredUVs.Add(uvs[idx2]);
                        if (idx2 < boneWeights.Length)
                        {
                            filteredBoneWeights.Add(boneWeights[idx2]);
                        }
                        else
                        {
                            filteredBoneWeights.Add(new BoneWeight());
                        }
                    }

                    if (idx3InList) validIndices.Add(addedVertices.IndexOf(idx3));
                    else if (!IsVertexIncluded(worldVertices[idx3], boneWeights, idx3, originalSkinnedRenderer.bones, boneNames))
                    {
                        validIndices.Add(filteredVertices.Count);
                        filteredVertices.Add(vertices[idx3]);
                        filteredNormals.Add(normals[idx3]);
                        filteredUVs.Add(uvs[idx3]);
                        if (idx3 < boneWeights.Length)
                        {
                            filteredBoneWeights.Add(boneWeights[idx3]);
                        }
                        else
                        {
                            filteredBoneWeights.Add(new BoneWeight());
                        }
                    }

                    if (validIndices.Count >= 3)
                    {
                        filteredTriangles.AddRange(validIndices);
                    }
                }
            }
        }

        if (filteredTriangles.Count % 3 != 0)
        {
            return null;
        }

        Mesh newMesh = new Mesh();
        newMesh.vertices = filteredVertices.ToArray();
        newMesh.triangles = filteredTriangles.ToArray();
        newMesh.normals = filteredNormals.ToArray();
        newMesh.uv = filteredUVs.ToArray();
        newMesh.boneWeights = filteredBoneWeights.ToArray();
        newMesh.bindposes = bindPoses;

        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();

        GameObject newObject = new GameObject("SubSkinnedMesh");
        SkinnedMeshRenderer newSkinnedRenderer = newObject.AddComponent<SkinnedMeshRenderer>();
        newSkinnedRenderer.sharedMesh = newMesh;
        newSkinnedRenderer.materials = originalSkinnedRenderer.materials;
        newSkinnedRenderer.bones = originalSkinnedRenderer.bones;
        newSkinnedRenderer.rootBone = originalSkinnedRenderer.rootBone;

        return newObject;
    }

}
