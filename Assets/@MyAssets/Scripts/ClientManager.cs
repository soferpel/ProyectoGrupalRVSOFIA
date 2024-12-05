using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    public GameObject clientPrefab;
    public Transform spawnPoint;

    public float initialMinInterval = 25f;
    public float initialMaxInterval = 50f;
    public float minIntervalLimit = 10f;
    public float intervalDecrement = 0.1f;

    private float currentMinInterval;
    private float currentMaxInterval;

    public Transform buyPoint;
    public Transform finalPoint;
    public Transform doorExitPoint;
    public Transform finalDestinationPoint;
    public BuyPointController buyPointController;
    public ShopNavigator shopNavigator;
    void Start()
    {
        currentMinInterval = initialMinInterval;
        currentMaxInterval = initialMaxInterval;

        StartCoroutine(SpawnClients());
    }

    IEnumerator SpawnClients()
    {
        while (true)
        {
            SpawnClient();

            float waitTime = Random.Range(currentMinInterval, currentMaxInterval);

            yield return new WaitForSeconds(waitTime);

            if (currentMinInterval > minIntervalLimit)
            {
                currentMinInterval = Mathf.Max(minIntervalLimit, currentMinInterval - intervalDecrement);
            }

            if (currentMaxInterval > minIntervalLimit)
            {
                currentMaxInterval = Mathf.Max(minIntervalLimit, currentMaxInterval - intervalDecrement);
            }
        }
    }

    void SpawnClient()
    {
        GameObject client = Instantiate(clientPrefab, spawnPoint.position, Quaternion.identity);
        ClientController controller = client.GetComponent<ClientController>();
        controller.SetBuyPoint(buyPoint);
        controller.SetFinalPoint(finalPoint);
        controller.SetDoorExitPoint(doorExitPoint);
        controller.SetFinalDestinationPoint(finalDestinationPoint);
        controller.SetBuyPointController(buyPointController);
        controller.SetShopNavigator(shopNavigator);
    }
}