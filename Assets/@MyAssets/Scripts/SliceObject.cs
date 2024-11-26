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
    private Borrar collisionCutComponents;
    void Update()
    {
        bool hasHit = Physics.Linecast(startSlicePoint.position, endSlicePoint.position, out RaycastHit hit, sliceableLayer);
        if (hasHit)
        {
            collisionCut = hit.collider.gameObject;
            if (collisionCut.TryGetComponent<Borrar>(out Borrar borrar) && borrar.target!=null )
            {
                collisionCutComponents = borrar;
                //Debug.Log(borrar.target);
                Slice(borrar.target);
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
        DrawDebugPlane(endSlicePoint.position, planeNormal, 0.5f);
        //Debug.Log("1");

        SkinnedMeshRenderer skinnedMeshRenderer = target.GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer != null)
        {
            Mesh bakedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(bakedMesh);

            GameObject tempObject = new GameObject("TempSlicingObject");
            tempObject.transform.position = target.transform.position;
            tempObject.transform.rotation = target.transform.rotation;
            tempObject.transform.localScale = new Vector3(1,1,1);

            MeshFilter tempMeshFilter = tempObject.AddComponent<MeshFilter>();
            tempMeshFilter.mesh = bakedMesh;
            MeshRenderer tempMeshRenderer = tempObject.AddComponent<MeshRenderer>();
            tempMeshRenderer.materials = skinnedMeshRenderer.sharedMaterials;

            SlicedHull hull = tempObject.Slice(endSlicePoint.position, planeNormal);
            //Debug.Log("2");
            if (hull != null)
            {
                //Debug.Log("3");

                GameObject upperHull = hull.CreateUpperHull(tempObject, bloodMaterial);
                SetupSlicedComponent(upperHull);

                GameObject lowerHull = hull.CreateLowerHull(tempObject, bloodMaterial);
                SetupSlicedComponent(lowerHull);
                foreach (Transform child in target.transform)
                {
                    foreach (Transform child2 in child.transform)
                    {
                        if (child2.gameObject.activeSelf)
                        {

                            SliceChild(child2.gameObject, upperHull, lowerHull, planeNormal, upperHull);
                        }
                    }
                }
                Destroy(target);
            }

            Destroy(tempObject);
        }
        else
        {

            //Debug.LogWarning("The target does not have a SkinnedMeshRenderer!");
            SlicedHull hull = target.Slice(endSlicePoint.position, planeNormal);
            //Debug.Log("2");
            if (hull != null)
            {
                //Debug.Log("3");

                GameObject upperHull = hull.CreateUpperHull(target, bloodMaterial);
                upperHull.transform.position = target.transform.position;
                upperHull.transform.rotation = target.transform.rotation;
                upperHull.transform.localScale = new Vector3(100, 100, 100);
                GameObject lowerHull = hull.CreateLowerHull(target, bloodMaterial);
                lowerHull.transform.position = target.transform.position;
                lowerHull.transform.rotation = target.transform.rotation;
                lowerHull.transform.localScale = new Vector3(100, 100, 100);

                if (upperHull != null && lowerHull != null)
                {
                    float upperDistance = CalculateAverageDistance(upperHull, collisionCutComponents.attachPoint.transform.position);
                    float lowerDistance = CalculateAverageDistance(lowerHull, collisionCutComponents.attachPoint.transform.position);

                    GameObject detachPart = null;
                    if (upperDistance < lowerDistance)
                    {
                        detachPart = lowerHull;
                        SetupSlicedComponent(lowerHull);
                        AttachToBody(upperHull);
                        DetachPart(lowerHull);
                    }
                    else
                    {
                        detachPart = upperHull;
                        SetupSlicedComponent(upperHull);
                        AttachToBody(lowerHull);
                        DetachPart(upperHull);
                    }
                    foreach (GameObject clothes in collisionCutComponents.clothes)
                    {
                        foreach (Transform child in clothes.transform)
                        {
                            if (child.gameObject.activeSelf)
                            {

                                SliceChild(child.gameObject, upperHull, lowerHull, planeNormal, detachPart);
                            }
                        }
                    }
                    
                }
                Destroy(target);
            }
        }
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
        //Debug.Log($"Intentando cortar el hijo: {child.name}");

        SkinnedMeshRenderer skinnedMeshRenderer = child.GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer != null)
        {
            Mesh bakedMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(bakedMesh);

            GameObject tempObject = new GameObject("TempSlicingObject");
            tempObject.transform.position = child.transform.position;
            tempObject.transform.rotation = child.transform.rotation;
            tempObject.transform.localScale = new Vector3(1, 1, 1);

            MeshFilter tempMeshFilter1 = tempObject.AddComponent<MeshFilter>();
            tempMeshFilter1.mesh = bakedMesh;
            MeshRenderer tempMeshRenderer1 = tempObject.AddComponent<MeshRenderer>();
            tempMeshRenderer1.materials = skinnedMeshRenderer.sharedMaterials;

            Bounds armBounds = GetArmBounds(collisionCut);
            if (armBounds == new Bounds())
            {
                //Debug.LogWarning("No se pudo obtener los Bounds del brazo para la ropa.");
                return;
            }

            Mesh mangaMesh = GetSubMeshInBounds(tempMeshFilter1.sharedMesh, collisionCutComponents.ConvertCollidersToBounds(), child.transform,tempMeshFilter1);
            if (mangaMesh == null || mangaMesh.vertexCount == 0)
            {
                //Debug.LogWarning("No se encontraron v�rtices en los Bounds especificados para la ropa.");
                return;
            }
            Destroy(tempObject);
            
            Mesh clothMesh = GetSubMeshOutOfBounds(tempMeshFilter1.sharedMesh, collisionCutComponents.ConvertCollidersToBounds(), child.transform, tempMeshFilter1);
            GameObject clothObject = new GameObject($"Temp_{child.name}_Manga_hola");
            clothObject.transform.position = child.transform.position;
            clothObject.transform.rotation = child.transform.rotation;
            clothObject.transform.localScale = new Vector3(1, 1, 1);

            MeshFilter clothMeshFilter = clothObject.AddComponent<MeshFilter>();
            clothMeshFilter.mesh = clothMesh;

            MeshRenderer clothMeshRenderer = clothObject.AddComponent<MeshRenderer>();
            clothMeshRenderer.materials = skinnedMeshRenderer.sharedMaterials;

            clothObject.transform.SetParent(collisionCutComponents.cuttedClothes.transform);
            child.SetActive(false);

            GameObject tempMangaObject = new GameObject($"Temp_{child.name}_Manga");
            tempMangaObject.transform.position = child.transform.position;
            tempMangaObject.transform.rotation = child.transform.rotation;
            tempMangaObject.transform.localScale = new Vector3(1,1,1);

            MeshFilter tempMeshFilter = tempMangaObject.AddComponent<MeshFilter>();
            tempMeshFilter.mesh = mangaMesh;

            MeshRenderer tempMeshRenderer = tempMangaObject.AddComponent<MeshRenderer>();
            tempMeshRenderer.materials = skinnedMeshRenderer.sharedMaterials;

            SlicedHull hull = tempMangaObject.Slice(endSlicePoint.position, planeNormal);
            if (hull != null)
            {
                //Debug.Log($"Corte exitoso para: {child.name}");

                GameObject upperHull = hull.CreateUpperHull(tempMangaObject, bloodMaterial);
                if (upperHull != null)
                {
                    upperHull.transform.SetParent(upperHullParent.transform, true);
                }

                GameObject lowerHull = hull.CreateLowerHull(tempMangaObject, bloodMaterial);
                if (lowerHull != null)
                {
                    lowerHull.transform.SetParent(lowerHullParent.transform, true);
                }
            Destroy(tempMangaObject);
            }
            else
            {
                //Debug.LogWarning($"No se pudo cortar la manga del objeto dasd: {child.name}");
                tempMangaObject.transform.SetParent(detachPart.transform);
            }

        }
        else
        {

            //Debug.LogWarning($"El objeto {child.name} no tiene un SkinnedMeshRenderer. Saltando corte.");
            //Debug.LogWarning("The target does not have a SkinnedMeshRenderer!");

            GameObject tempObject = new GameObject("TempSlicingObjectdwedwed");
            tempObject.transform.position = child.transform.position;
            tempObject.transform.rotation = child.transform.rotation;
            tempObject.transform.localScale = new Vector3(1, 1, 1);

            MeshFilter tempMeshFilter1 = tempObject.AddComponent<MeshFilter>();
            tempMeshFilter1.mesh = child.GetComponent<MeshFilter>().mesh;
            MeshRenderer tempMeshRenderer1 = tempObject.AddComponent<MeshRenderer>();
            tempMeshRenderer1.materials = child.GetComponent<MeshRenderer>().materials;

            
            Mesh mangaMesh = GetSubMeshInBounds(tempMeshFilter1.sharedMesh, collisionCutComponents.ConvertCollidersToBounds(), child.transform, tempMeshFilter1);
            if (mangaMesh == null || mangaMesh.vertexCount == 0)
            {
                //Debug.LogWarning("No se encontraron v�rtices en los Bounds especificados para la ropa.");
                Destroy(tempObject);
                return;
            }

            Mesh clothMesh = GetSubMeshOutOfBounds(tempMeshFilter1.sharedMesh, collisionCutComponents.ConvertCollidersToBounds(), child.transform, tempMeshFilter1);
            Destroy(tempObject);
            GameObject clothObject = new GameObject($"Temp_{child.name}_Manga_hola");
            clothObject.transform.position = child.transform.position;
            clothObject.transform.rotation = child.transform.rotation;
            clothObject.transform.localScale = new Vector3(1, 1, 1);

            MeshFilter clothMeshFilter = clothObject.AddComponent<MeshFilter>();
            clothMeshFilter.mesh = clothMesh;

            MeshRenderer clothMeshRenderer = clothObject.AddComponent<MeshRenderer>();
            clothMeshRenderer.materials = child.GetComponent<MeshRenderer>().materials;

            clothObject.transform.SetParent(collisionCutComponents.cuttedClothes.transform);
            child.SetActive(false);

            GameObject tempMangaObject = new GameObject($"Temp_{child.name}_Manga");
            tempMangaObject.transform.position = child.transform.position;
            tempMangaObject.transform.rotation = child.transform.rotation;
            tempMangaObject.transform.localScale = new Vector3(1, 1, 1);

            MeshFilter tempMeshFilter = tempMangaObject.AddComponent<MeshFilter>();
            tempMeshFilter.mesh = mangaMesh;

            MeshRenderer tempMeshRenderer = tempMangaObject.AddComponent<MeshRenderer>();
            tempMeshRenderer.materials = child.GetComponent<MeshRenderer>().materials;
            SlicedHull hull = tempMangaObject.Slice(endSlicePoint.position, planeNormal);
            //Debug.Log("2");
            if (hull != null)
            {
                //Debug.Log($"Corte exitoso para: {child.name}");

                GameObject upperHull = hull.CreateUpperHull(tempMangaObject, bloodMaterial);
                if (upperHull != null)
                {
                    upperHull.transform.SetParent(upperHullParent.transform, true);
                }

                GameObject lowerHull = hull.CreateLowerHull(tempMangaObject, bloodMaterial);
                if (lowerHull != null)
                {
                    lowerHull.transform.SetParent(lowerHullParent.transform, true);
                }
                //Debug.Log("3");

                Destroy(child);
                Destroy(tempMangaObject);
            }
            else
            {
                //Debug.LogWarning($"No se pudo cortar el hijo: {child.name}");
                tempMangaObject.transform.SetParent(detachPart.transform);
            }
        }
    }
    void DrawDebugPlane(Vector3 pointOnPlane, Vector3 planeNormal, float size)
    {
        Vector3 right = Vector3.Cross(planeNormal, Vector3.up).normalized * size;
        if (right == Vector3.zero)
        {
            right = Vector3.Cross(planeNormal, Vector3.forward).normalized * size;
        }
        Vector3 forward = Vector3.Cross(planeNormal, right).normalized * size;

        Vector3 topLeft = pointOnPlane + forward - right;
        Vector3 topRight = pointOnPlane + forward + right;
        Vector3 bottomLeft = pointOnPlane - forward - right;
        Vector3 bottomRight = pointOnPlane - forward + right;

        Debug.DrawLine(topLeft, topRight, Color.green, 0.1f);
        Debug.DrawLine(topRight, bottomRight, Color.green, 0.1f);
        Debug.DrawLine(bottomRight, bottomLeft, Color.green, 0.1f);
        Debug.DrawLine(bottomLeft, topLeft, Color.green, 0.1f);

        Debug.DrawRay(pointOnPlane, planeNormal * size, Color.red, 0.1f);
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

        XRGrabInteractable grabInteractable = part.AddComponent<XRGrabInteractable>();
        GameObject attachPoint = new GameObject("AttachPoint");
        attachPoint.transform.SetParent(part.transform);
        attachPoint.transform.localPosition = part.GetComponent<MeshFilter>().sharedMesh.bounds.center;
        grabInteractable.attachTransform = attachPoint.transform;

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

        }
    }
    private Bounds GetArmBounds(GameObject armObject)
    {
        Collider armCollider = armObject.GetComponent<Collider>();
        if (armCollider != null)
        {
            return armCollider.bounds;
        }
        else
        {
            return new Bounds();
        }
    }



public Mesh GetSubMesh(Mesh originalMesh, Bounds[] bounds, Transform parentTransform, MeshFilter originalMeshFilter, bool inBound)
{
    if (bounds == null || bounds.Length == 0)
    {
        return null;
    }
        foreach (var bound in bounds)
        {
            DrawBounds(bound);
        }
            Vector3[] vertices = originalMesh.vertices;
    int[] triangles = originalMesh.triangles;
    Vector3[] normals = originalMesh.normals;
    BoneWeight[] boneWeights = originalMesh.boneWeights;
    Matrix4x4[] bindPoses = originalMesh.bindposes;     

    Vector3[] worldVertices = new Vector3[vertices.Length];
    for (int i = 0; i < vertices.Length; i++)
    {
        worldVertices[i] = originalMeshFilter.transform.TransformPoint(vertices[i]);
    }

    List<Vector3> filteredVertices = new List<Vector3>();
    List<int> filteredTriangles = new List<int>();
    List<Vector3> filteredNormals = new List<Vector3>();
    List<BoneWeight> filteredBoneWeights = new List<BoneWeight>();
    Dictionary<int, int> vertexMap = new Dictionary<int, int>();

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

    for (int i = 0; i < worldVertices.Length; i++)
    {
        if (IsVertexIncluded(worldVertices[i]))
        {
            vertexMap[i] = filteredVertices.Count;
            filteredVertices.Add(vertices[i]);
            filteredNormals.Add(normals[i]);
            if (boneWeights.Length > i)
            {
                filteredBoneWeights.Add(boneWeights[i]);
            }
        }
    }

    
    for (int i = 0; i < triangles.Length; i += 3)
    {
        int idx1 = triangles[i];
        int idx2 = triangles[i + 1];
        int idx3 = triangles[i + 2];

        bool idx1InMap = vertexMap.ContainsKey(idx1);
        bool idx2InMap = vertexMap.ContainsKey(idx2);
        bool idx3InMap = vertexMap.ContainsKey(idx3);

        if (inBound && idx1InMap && idx2InMap && idx3InMap)
        {
            filteredTriangles.Add(vertexMap[idx1]);
            filteredTriangles.Add(vertexMap[idx2]);
            filteredTriangles.Add(vertexMap[idx3]);
        }
        else if (!inBound)
        {
            List<int> validIndices = new List<int>();
            if (idx1InMap) validIndices.Add(vertexMap[idx1]);
            if (idx2InMap) validIndices.Add(vertexMap[idx2]);
            if (idx3InMap) validIndices.Add(vertexMap[idx3]);

            if (validIndices.Count == 3)
            {
                filteredTriangles.AddRange(validIndices);
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

    if (boneWeights.Length > 0)
    {
        newMesh.boneWeights = filteredBoneWeights.ToArray();
        newMesh.bindposes = bindPoses;
    }

    newMesh.RecalculateBounds();
    newMesh.RecalculateNormals();
    newMesh.RecalculateTangents();

    return newMesh;
}





void DrawBounds(Bounds b, float delay = 10)
    {
        var p1 = new Vector3(b.min.x, b.min.y, b.min.z);
        var p2 = new Vector3(b.max.x, b.min.y, b.min.z);
        var p3 = new Vector3(b.max.x, b.min.y, b.max.z);
        var p4 = new Vector3(b.min.x, b.min.y, b.max.z);

        Debug.DrawLine(p1, p2, Color.blue, delay);
        Debug.DrawLine(p2, p3, Color.red, delay);
        Debug.DrawLine(p3, p4, Color.yellow, delay);
        Debug.DrawLine(p4, p1, Color.magenta, delay);

        var p5 = new Vector3(b.min.x, b.max.y, b.min.z);
        var p6 = new Vector3(b.max.x, b.max.y, b.min.z);
        var p7 = new Vector3(b.max.x, b.max.y, b.max.z);
        var p8 = new Vector3(b.min.x, b.max.y, b.max.z);

        Debug.DrawLine(p5, p6, Color.blue, delay);
        Debug.DrawLine(p6, p7, Color.red, delay);
        Debug.DrawLine(p7, p8, Color.yellow, delay);
        Debug.DrawLine(p8, p5, Color.magenta, delay);

        Debug.DrawLine(p1, p5, Color.white, delay);
        Debug.DrawLine(p2, p6, Color.gray, delay);
        Debug.DrawLine(p3, p7, Color.green, delay);
        Debug.DrawLine(p4, p8, Color.cyan, delay);
    }

    public Mesh GetSubMeshInBounds(Mesh originalMesh, Bounds[] bounds, Transform parentTransform, MeshFilter originalMeshFilter)
    {
        return GetSubMesh(originalMesh, bounds, parentTransform, originalMeshFilter, true);
    }

    public Mesh GetSubMeshOutOfBounds(Mesh originalMesh, Bounds[] bounds, Transform parentTransform, MeshFilter originalMeshFilter)
    {
        return GetSubMesh(originalMesh, bounds, parentTransform, originalMeshFilter, false);
    }
}
