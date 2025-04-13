using Unity.Netcode;
using UnityEngine;

public class ShelfBoxSpawnerMP : NetworkBehaviour
{
    public Transform[] boxSpawnPoints;
    public GameObject boxPrefab;
    public GameObject boxLidPrefab;

    private Vector3 offset = new Vector3(0, -0.2f, 0);
    private float colliderCheckRadius = 0.1f;

    public LayerMask detectionLayer;
    public bool startBox = false;

    private void Start()
    {
        if (IsServer && startBox) SpawnBoxes();
    }

    public void SpawnBoxes()
    {
        foreach (Transform spawnPoint in boxSpawnPoints)
        {
            Collider[] colliders = Physics.OverlapSphere(spawnPoint.position, colliderCheckRadius, detectionLayer);
            if (colliders.Length == 0)
            {
                GameObject box = Instantiate(boxPrefab, spawnPoint.position, spawnPoint.rotation);
                GameObject lid = Instantiate(boxLidPrefab, spawnPoint.position + offset, spawnPoint.rotation);

                box.GetComponent<NetworkObject>().Spawn();
                lid.GetComponent<NetworkObject>().Spawn();
            }
        }
    }

    public int GetFreePoints()
    {
        int cont = 0;

        foreach (Transform spawnPoint in boxSpawnPoints)
        {
            Collider[] colliders = Physics.OverlapSphere(spawnPoint.position, colliderCheckRadius, detectionLayer);
            if (colliders.Length == 0) cont++;
        }
        return cont;
    }
}
