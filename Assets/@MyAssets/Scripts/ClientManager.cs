using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    public GameObject clientPrefab;
    public GameObject mafiaPrefab;
    public Transform spawnPoint;

    public float initialMinInterval = 25f;
    public float initialMaxInterval = 50f;
    public float minIntervalLimit = 10f;
    public float intervalDecrement = 0.1f;

    public float mafiaSpawnInterval = 120f;
    public GameObject mafia = null;

    private float currentMinInterval;
    private float currentMaxInterval;

    public Transform buyPoint;
    public Transform finalPoint;
    public Transform doorExitPoint;
    public Transform finalDestinationPoint;
    public BuyPointController buyPointController;
    public ShopNavigator shopNavigator;
    public GameObject player;
    protected virtual void Start()
    {
        currentMinInterval = initialMinInterval;
        currentMaxInterval = initialMaxInterval;

        StartCoroutine(SpawnClients());
        StartCoroutine(SpawnMafioso());
    }

    private IEnumerator SpawnClients()
    {
        while (true)
        {
            SpawnClient(clientPrefab);

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
    private IEnumerator SpawnMafioso()
    {
        while (true)
        {
            yield return new WaitForSeconds(mafiaSpawnInterval);

            if (mafia == null)
            {
                mafia = SpawnClient(mafiaPrefab);
            }
        }
    }
    private GameObject SpawnClient(GameObject prefab)
    {
        GameObject client = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        PersonController controller = client.GetComponent<PersonController>();
        controller.SetBuyPoint(buyPoint);
        controller.SetFinalPoint(finalPoint);
        controller.SetDoorExitPoint(doorExitPoint);
        controller.SetFinalDestinationPoint(finalDestinationPoint);
        controller.SetBuyPointController(buyPointController);
        controller.SetShopNavigator(shopNavigator);
        client.SetActive(true);

        if(client.TryGetComponent(out MafiaController mafiacontr))
        {
            mafiacontr.player = player;
        }
        return client;
    }
}