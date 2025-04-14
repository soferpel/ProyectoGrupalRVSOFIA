using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShopNavigatorMP : NetworkBehaviour
{
    public List<Collider> storeAreas;
    public Animator doorAnimator;
    public Transform GetRandomPointWithinStoreBounds()
    {
        if (storeAreas == null || storeAreas.Count == 0)
        {
            Debug.LogError("No hay áreas de la tienda asignadas.");
            return null;
        }

        Vector3 randomPosition;
        int maxAttempts = 100;
        int attempts = 0;

        do
        {
            Collider randomArea = storeAreas[Random.Range(0, storeAreas.Count)];

            Bounds bounds = randomArea.bounds;
            randomPosition = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.center.y,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            attempts++;
        } while (!IsPointInsideCollider(randomPosition) && attempts < maxAttempts);

        if (attempts >= maxAttempts)
        {
            Debug.LogWarning("No se encontró un punto válido dentro de los colliders de la tienda.");
            return null;
        }

        GameObject tempTarget = new GameObject("RandomTarget");
        tempTarget.transform.position = randomPosition;

        return tempTarget.transform;
    }

    private bool IsPointInsideCollider(Vector3 point)
    {
        Ray ray = new Ray(new Vector3(point.x, 50f, point.z), Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            return storeAreas.Contains(hit.collider);
        }
        return false;
    }

    public void OpenDoor()
    {
        doorAnimator.SetTrigger("Open");
    }

    [ClientRpc]
    public void OpenDoorClientRpc()
    {
        OpenDoor();
    }
}
