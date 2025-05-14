using Unity.Netcode;
using UnityEngine;

public class ShelfClothSpawnerMP : NetworkBehaviour
{
    public Transform[] clothSpawnPoints;
    public GameObject clothPrefab;

    private float colliderCheckRadius = 0.1f;
    public LayerMask detectionLayer;

    private void Update()
    {
        if (!IsServer) return;

        foreach (Transform spawnPoint in clothSpawnPoints)
        {
            Collider[] colliders = Physics.OverlapSphere(spawnPoint.position, colliderCheckRadius, detectionLayer);
            if (colliders.Length == 0)
            {
                GameObject cloth = Instantiate(clothPrefab, spawnPoint.position, spawnPoint.rotation);
                cloth.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
