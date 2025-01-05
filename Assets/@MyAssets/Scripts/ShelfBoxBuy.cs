using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelfBoxBuy : MonoBehaviour
{
    public Transform[] boxSpawnPoints;
    public GameObject box;
    public GameObject boxLid;

    private Vector3 offset = new Vector3(0, -0.2f, 0);
    private float colliderCheckRadius = 0.1f;

    public LayerMask detectionLayer;

    public OrderController orderController;

    public void SpawnBoxes(int coins)
    {
        if(orderController.cash>= coins && boxSpawnPoints.Length == GetFreePoints())
        {
            orderController.cash -= coins;
            foreach (Transform spawnPoint in boxSpawnPoints)
            {
                Collider[] colliders = Physics.OverlapSphere(spawnPoint.position, colliderCheckRadius, detectionLayer);
                if (colliders.Length == 0)
                {
                    Instantiate(box, spawnPoint.position, spawnPoint.rotation);
                    Instantiate(boxLid, spawnPoint.position + offset, spawnPoint.rotation);
                }
            }
        }
    }

    public int GetFreePoints()
    {
        int cont = 0;

        foreach (Transform spawnPoint in boxSpawnPoints)
        {
            Collider[] colliders = Physics.OverlapSphere(spawnPoint.position, colliderCheckRadius, detectionLayer);
            if (colliders.Length == 0)
            {
                cont++;
            }
        }
        return cont;
    }
}
