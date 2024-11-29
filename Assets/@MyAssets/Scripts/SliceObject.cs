using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using UnityEngine.XR.Interaction.Toolkit;

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

    private List<GameObject> temporalcuttedClothes = new List<GameObject>();
    void Update()
    {
        if (Physics.Linecast(startSlicePoint.position, endSlicePoint.position, out RaycastHit hit, sliceableLayer))
        {
            collisionCut = hit.collider.gameObject;
            if (collisionCut.TryGetComponent(out SliceablePartController partController) && partController.target!=null )
            {
                collisionCutComponents = partController;
                Slice(partController.target);
                Destroy(partController);
            }
            collisionCutComponents = null;
            collisionCut = null;
            
        }
    }
    public void Slice(GameObject target)
    {
        Vector3 velocity = velocityEstimator.GetVelocityEstimate();
        Vector3 planeNormal = Vector3.Cross(endSlicePoint.position - startSlicePoint.position, velocity);
        planeNormal.Normalize();

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

        GameObject outBoundPart = GetSkinnedSubMesh(tempMeshFilter.sharedMesh, collisionCutComponents.ConvertCollidersToBounds(), skinnedMeshRenderer, false, tempMeshFilter);

        GameObject inBoundMesh = GetSkinnedSubMesh(tempMeshFilter.sharedMesh, collisionCutComponents.ConvertCollidersToBounds(), skinnedMeshRenderer, true, tempMeshFilter);
        
        if (inBoundMesh == null)
        {
            return;
        }

        // Asignamos el SkinnedMeshRenderer de outBoundPart al target
        if (outBoundPart != null)
        {
            SkinnedMeshRenderer outBoundSkinnedMeshRenderer = outBoundPart.GetComponent<SkinnedMeshRenderer>();
            if (outBoundSkinnedMeshRenderer != null && skinnedMeshRenderer != null)
            {
                // Asignamos el skinned mesh renderer de outBoundPart al target
                skinnedMeshRenderer.sharedMesh = outBoundSkinnedMeshRenderer.sharedMesh;
                skinnedMeshRenderer.sharedMaterials = outBoundSkinnedMeshRenderer.sharedMaterials;
            }
        }

        // Destruir el outBoundPart después de asignar el SkinnedMeshRenderer
        if (outBoundPart != null)
        {
            Destroy(outBoundPart);
        }

        // Creamos el objeto temporal para las submallas
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

        // Cortamos el objeto en dos partes (upperHull y lowerHull)
        SlicedHull hull = tempObject2.Slice(endSlicePoint.position, planeNormal);
        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(tempObject2, bloodMaterial);
            GameObject lowerHull = hull.CreateLowerHull(tempObject2, bloodMaterial);
            if (upperHull != null && lowerHull != null)
            {
                foreach(Collider collider in collisionCutComponents.boundsColliders)
                {
                    Destroy(collider);
                }
                float upperDistance = CalculateAverageDistance(upperHull, collisionCutComponents.attachPoint.transform.position);
                float lowerDistance = CalculateAverageDistance(lowerHull, collisionCutComponents.attachPoint.transform.position);

                GameObject detachPart = (upperDistance < lowerDistance) ? lowerHull : upperHull;
                SetupSlicedComponent(detachPart);
                DetachPart(detachPart);

                GameObject connectedPart = (detachPart == lowerHull) ? upperHull : lowerHull;
                AttachToBody(connectedPart);

                /*
                collisionCutComponents.bodyGrab.enabled = false;
                foreach (Collider collider in collisionCutComponents.boundsColliders)
                {
                    collisionCutComponents.bodyGrab.colliders.Remove(collider);
                }
                collisionCutComponents.bodyGrab.enabled = true;
                */
                /*
                foreach (GameObject clothes in collisionCutComponents.clothes)
                {
                    ProcessClothChildren(clothes.transform, upperHull, lowerHull, planeNormal, detachPart);
                }
                UpdateClothesHierarchy();*/
            }
            // Destruir objetos temporales
            Destroy(tempObject2);
            Destroy(inBoundMesh);
        }

        Destroy(tempObject);
        collisionCutComponents.dragController.countBodyParts--;
        if (collisionCutComponents.dragController.countBodyParts <= 0)
        {
            collisionCutComponents.bodyGrab.trackPosition = true;
            collisionCutComponents.bodyGrab.trackRotation = true;
            collisionCutComponents.dragController.enabled = false;
        }
    }


    private void ApplyHullTransform(GameObject hull, Vector3 position, Quaternion rotation)
    {
        if (hull != null)
        {
            hull.transform.position = position;
            hull.transform.rotation = rotation;
            hull.transform.localScale = new Vector3(100, 100, 100);
        }
    }

    private void ProcessClothChildren(Transform clothesTransform, GameObject upperHull, GameObject lowerHull, Vector3 planeNormal, GameObject detachPart)
    {
        foreach (Transform child in clothesTransform)
        {
            if (child.gameObject.activeSelf)
            {
                SliceChild(child.gameObject, upperHull, lowerHull, planeNormal, detachPart);
            }
        }
    }

    private void UpdateClothesHierarchy()
    {
        foreach (GameObject cloth in temporalcuttedClothes)
        {
            cloth.transform.SetParent(collisionCutComponents.cuttedClothes.transform);
        }
        temporalcuttedClothes.Clear();
    }

    public void SetupSlicedComponent(GameObject slicedObject)
    {
        Rigidbody rg = slicedObject.AddComponent<Rigidbody>();
        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();
        collider.convex = true;
        rg.AddExplosionForce(cutForce, slicedObject.transform.position, 1);
    }
    public void SliceChild(GameObject child, GameObject upperHullParent, GameObject lowerHullParent, Vector3 planeNormal, GameObject detachPart)
    {
        SkinnedMeshRenderer skinnedMeshRenderer = child.GetComponent<SkinnedMeshRenderer>();
        GameObject tempObject = new GameObject("TempSlicingObject");
        tempObject.transform.position = child.transform.position;
        tempObject.transform.rotation = child.transform.rotation;
        tempObject.transform.localScale = new Vector3(1, 1, 1);

        MeshFilter tempMeshFilter1 = tempObject.AddComponent<MeshFilter>();
        
        MeshRenderer tempMeshRenderer1 = tempObject.AddComponent<MeshRenderer>();
        

        Mesh bakedMesh = new Mesh();
        if (skinnedMeshRenderer != null)
        {
            skinnedMeshRenderer.BakeMesh(bakedMesh);
            tempMeshFilter1.mesh = bakedMesh;
            tempMeshRenderer1.materials = skinnedMeshRenderer.sharedMaterials;
        }
        else
        {
            tempMeshFilter1.mesh = child.GetComponent<MeshFilter>().mesh;
            tempMeshRenderer1.materials = child.GetComponent<MeshRenderer>().materials;
        }
            GameObject outBoundPart = GetSubMeshOutOfBounds(tempMeshFilter1.sharedMesh, collisionCutComponents.ConvertCollidersToBounds(), tempMeshFilter1);
            outBoundPart.transform.position = child.transform.position;
            outBoundPart.transform.rotation = child.transform.rotation;
            outBoundPart.transform.localScale = Vector3.one;

            temporalcuttedClothes.Add(outBoundPart);
            child.SetActive(false);

            GameObject inBoundMesh = GetSubMeshInBounds(tempMeshFilter1.sharedMesh, collisionCutComponents.ConvertCollidersToBounds(), tempMeshFilter1);
            if (inBoundMesh == null)
            {
                return;
            }
            inBoundMesh.transform.position = child.transform.position;
            inBoundMesh.transform.rotation = child.transform.rotation;
            inBoundMesh.transform.localScale = Vector3.one;

            SlicedHull hull = inBoundMesh.Slice(endSlicePoint.position, planeNormal);
            if (hull != null)
            {
                GameObject upperHull = hull.CreateUpperHull(inBoundMesh, bloodMaterial);
                if (upperHull != null)
                {
                    upperHull.transform.SetParent(upperHullParent.transform, true);
                }

                GameObject lowerHull = hull.CreateLowerHull(inBoundMesh, bloodMaterial);
                if (lowerHull != null)
                {
                    lowerHull.transform.SetParent(lowerHullParent.transform, true);
                }
                Destroy(inBoundMesh);
            }
            else
            {
                inBoundMesh.transform.SetParent(detachPart.transform);
            }
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
        part.tag = collisionCutComponents.gameObjectTag;
        XRGrabInteractable grabInteractable = part.AddComponent<XRGrabInteractable>();
        grabInteractable.useDynamicAttach = true;
        /*
        grabInteractable.enabled = false;
        foreach (Collider collider in collisionCutComponents.boundsColliders)
        {
            grabInteractable.colliders.Add(collider);
        }
        grabInteractable.enabled = true;*/
        /*
        foreach (GameObject child in collisionCutComponents.children)
        {
            child.transform.SetParent(part.transform);
        }

        foreach (GameObject clothesChild in collisionCutComponents.clothesChild)
        {
            foreach (Transform child in clothesChild.transform)
            {
                if (child.gameObject.activeSelf)
                {

                    SkinnedMeshRenderer childSkinnedMeshRenderer = child.GetComponent<SkinnedMeshRenderer>();
                    if (childSkinnedMeshRenderer != null)
                    {
                        Mesh bakedMesh = new Mesh();
                        childSkinnedMeshRenderer.BakeMesh(bakedMesh);

                        GameObject tempObject = new GameObject("hats");
                        tempObject.transform.position = child.transform.position;
                        tempObject.transform.rotation = child.transform.rotation;
                        tempObject.transform.localScale = new Vector3(1, 1, 1);

                        MeshFilter tempMeshFilter1 = tempObject.AddComponent<MeshFilter>();
                        tempMeshFilter1.mesh = bakedMesh;
                        MeshRenderer tempMeshRenderer1 = tempObject.AddComponent<MeshRenderer>();
                        tempMeshRenderer1.materials = childSkinnedMeshRenderer.sharedMaterials;
                        tempObject.transform.SetParent(part.transform, true);
                        Destroy(child.gameObject);
                    }
                }
            }

        }*/
    }

    public GameObject GetSubMesh(Mesh originalMesh, Bounds[] bounds, MeshFilter originalMeshFilter, bool inBound)
    {
        if (bounds == null || bounds.Length == 0)
        {
            return null;
        }

        Vector3[] vertices = originalMesh.vertices;
        int[] triangles = originalMesh.triangles;
        Vector3[] normals = originalMesh.normals;
        Vector2[] uvs = originalMesh.uv;

        List<int> addedVertices = new List<int>();
        List<Vector3> filteredVertices = new List<Vector3>();
        List<int> filteredTriangles = new List<int>();
        List<Vector3> filteredNormals = new List<Vector3>();
        List<Vector2> filteredUVs = new List<Vector2>();

        bool IsVertexIncluded(Vector3 worldVertex)
        {
            foreach (var bound in bounds)
            {
                if (bound.Contains(worldVertex))
                {
                    return inBound;
                }
            }
            return !inBound;
        }

        Vector3[] worldVertices = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            worldVertices[i] = originalMeshFilter.transform.TransformPoint(vertices[i]);
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            if (IsVertexIncluded(worldVertices[i]) && !addedVertices.Contains(i))
            {
                addedVertices.Add(i);
                filteredVertices.Add(vertices[i]);
                filteredNormals.Add(normals[i]);
                filteredUVs.Add(uvs[i]);
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
                    else if (!IsVertexIncluded(worldVertices[idx1]))
                    {
                        validIndices.Add(filteredVertices.Count);
                        filteredVertices.Add(vertices[idx1]);
                        filteredNormals.Add(normals[idx1]);
                        filteredUVs.Add(uvs[idx1]);
                    }

                    if (idx2InList) validIndices.Add(addedVertices.IndexOf(idx2));
                    else if (!IsVertexIncluded(worldVertices[idx2]))
                    {
                        validIndices.Add(filteredVertices.Count);
                        filteredVertices.Add(vertices[idx2]);
                        filteredNormals.Add(normals[idx2]);
                        filteredUVs.Add(uvs[idx2]);
                    }

                    if (idx3InList) validIndices.Add(addedVertices.IndexOf(idx3));
                    else if (!IsVertexIncluded(worldVertices[idx3]))
                    {
                        validIndices.Add(filteredVertices.Count);
                        filteredVertices.Add(vertices[idx3]);
                        filteredNormals.Add(normals[idx3]);
                        filteredUVs.Add(uvs[idx3]);
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

        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();

        GameObject newObject = new GameObject("SubMesh");
        MeshFilter meshFilter = newObject.AddComponent<MeshFilter>();
        meshFilter.mesh = newMesh;

        MeshRenderer meshRenderer = newObject.AddComponent<MeshRenderer>();
        meshRenderer.material = originalMeshFilter.GetComponent<MeshRenderer>().material;

        return newObject;
    }

    public GameObject GetSkinnedSubMesh(Mesh originalMesh, Bounds[] bounds, SkinnedMeshRenderer originalSkinnedRenderer, bool inBound, MeshFilter originalMeshFilter)
{
    if (bounds == null || bounds.Length == 0)
    {
        return null;
    }

    // Usar el mesh asociado al SkinnedMeshRenderer en lugar del originalMesh
    Mesh originalSkinnedMesh = originalSkinnedRenderer.sharedMesh;

        // Obtener los datos del mesh original
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

    // Función para saber si el vértice está dentro de la zona delimitada por los bounds
    bool IsVertexIncluded(Vector3 worldVertex)
    {
        foreach (var bound in bounds)
        {
            if (bound.Contains(worldVertex))
            {
                return inBound;
            }
        }
        return !inBound;
    }

    // Transformar los vértices del mesh original al espacio mundial
    Vector3[] worldVertices = new Vector3[vertices.Length];
    for (int i = 0; i < vertices.Length; i++)
    {
        worldVertices[i] = originalMeshFilter.transform.TransformPoint(notvertices[i]);
    }

    // Filtrar los vértices según si están dentro de los bounds
    for (int i = 0; i < vertices.Length; i++)
    {
        if (IsVertexIncluded(worldVertices[i]) && !addedVertices.Contains(i))
        {
            addedVertices.Add(i);
            filteredVertices.Add(vertices[i]);
            filteredNormals.Add(normals[i]);
            filteredUVs.Add(uvs[i]);

            // Validar índice dentro de boneWeights
            if (i < boneWeights.Length)
            {
                filteredBoneWeights.Add(boneWeights[i]);
            }
            else
            {
                // Agregar un BoneWeight por defecto si no existe
                filteredBoneWeights.Add(new BoneWeight());
            }
        }
    }

    // Filtrar los triángulos que contienen los vértices filtrados
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
                else if (!IsVertexIncluded(worldVertices[idx1]))
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
                else if (!IsVertexIncluded(worldVertices[idx2]))
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
                else if (!IsVertexIncluded(worldVertices[idx3]))
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

    // Crear un nuevo mesh con los vértices filtrados
    Mesh newMesh = new Mesh();
    newMesh.vertices = filteredVertices.ToArray();
    newMesh.triangles = filteredTriangles.ToArray();
    newMesh.normals = filteredNormals.ToArray();
    newMesh.uv = filteredUVs.ToArray();
    newMesh.boneWeights = filteredBoneWeights.ToArray();
    newMesh.bindposes = bindPoses;

    // Recalcular las propiedades del nuevo mesh
    newMesh.RecalculateBounds();
    newMesh.RecalculateNormals();

    // Crear el GameObject con un SkinnedMeshRenderer para el nuevo submesh
    GameObject newObject = new GameObject("SubSkinnedMesh");
    SkinnedMeshRenderer newSkinnedRenderer = newObject.AddComponent<SkinnedMeshRenderer>();
    newSkinnedRenderer.sharedMesh = newMesh;
    newSkinnedRenderer.materials = originalSkinnedRenderer.materials;
    newSkinnedRenderer.bones = originalSkinnedRenderer.bones;
    newSkinnedRenderer.rootBone = originalSkinnedRenderer.rootBone;

    return newObject;
}



    public GameObject GetSubMeshInBounds(Mesh originalMesh, Bounds[] bounds, MeshFilter originalMeshFilter)
    {
        return GetSubMesh(originalMesh, bounds, originalMeshFilter, true);
    }

    public GameObject GetSubMeshOutOfBounds(Mesh originalMesh, Bounds[] bounds, MeshFilter originalMeshFilter)
    {
        return GetSubMesh(originalMesh, bounds, originalMeshFilter, false);
    }
}
