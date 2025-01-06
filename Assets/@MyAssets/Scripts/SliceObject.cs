using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

public class SliceObject : MonoBehaviour
{
    public Transform startSlicePoint;
    public Transform endSlicePoint;
    public VelocityEstimator velocityEstimator;
    public float cutForce;
    public LayerMask sliceableLayer;
    public Material bloodMaterial;

    private GameObject collisionCut;
    private SliceablePartController collisionCutComponents;
    public float umbralDistancia =0.1f;
    public bool hasCut = false;
    public UnityEvent OnCutMade;
    public AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    void Update()
    {
        if (Physics.Linecast(startSlicePoint.position, endSlicePoint.position, out RaycastHit hit, sliceableLayer))
        {
            collisionCut = hit.collider.gameObject;
            if (collisionCut.TryGetComponent(out SliceablePartController partController) && partController.target != null)
            {
                collisionCutComponents = partController;
                Slice(partController.target, partController);
            }
            collisionCutComponents = null;
            collisionCut = null;

        }
    }
    public void Slice(GameObject target, SliceablePartController partController)
    {
        Vector3 velocity = velocityEstimator.GetVelocityEstimate();
        Vector3 planeNormal = Vector3.Cross(endSlicePoint.position - startSlicePoint.position, velocity);
        planeNormal.Normalize();
        float alignment = Vector3.Dot(planeNormal, partController.gameObject.transform.up);
        if (Mathf.Abs(alignment) < 0.95f)
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
                SetupSlicedComponent(detachPart);
                DetachPart(detachPart);

                GameObject connectedPart = (detachPart == lowerHull) ? upperHull : lowerHull;
                AttachToBody(connectedPart);

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
                            XRGrabInteractable grabInteractable = detachPart.transform.parent.GetComponent<XRGrabInteractable>();
                            grabInteractable.trackScale = false;
                            grabInteractable.enabled = false;
                            grabInteractable.colliders.Add(collider);
                            grabInteractable.enabled = true;
                        }
                    }
                }
                

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
        part.transform.localPosition = new Vector3(0,0, 0.3f);

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

    private void DetachPart(GameObject part)
    {
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
        container.transform.rotation = part.transform.rotation; 
        container.transform.localScale = Vector3.one;

        Vector3 offset = part.transform.position - containerPosition;
        part.transform.SetParent(container.transform);
        part.transform.localPosition = offset;     

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
            if (IsVertexIncluded(worldVertices[i],boneWeights,i, originalSkinnedRenderer.bones, boneNames) && !addedVertices.Contains(i))
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
                    else if (!IsVertexIncluded(worldVertices[idx1],boneWeights, idx1, originalSkinnedRenderer.bones, boneNames))
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
