using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelfClothSpawner : MonoBehaviour
{
    public Transform[] clothSpawnPoints;
    public GameObject cloth;

    private float colliderCheckRadius = 0.1f;

    public LayerMask detectionLayer;

    void Update()
    {
        foreach (Transform spawnPoint in clothSpawnPoints)
        {
            Collider[] colliders = Physics.OverlapSphere(spawnPoint.position, colliderCheckRadius, detectionLayer);
            if (colliders.Length == 0)
            {
        Debug.Log("hola");
                Instantiate(cloth, spawnPoint.position, spawnPoint.rotation);
            }
        }
    }
}
