using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ClientManagerMP : NetworkBehaviour
{
    public GameObject clientPrefab;
    public GameObject mafiaPrefab;
    public Transform spawnPoint;

    public float initialMinInterval = 25f;
    public float initialMaxInterval = 50f;
    public float minIntervalLimit = 10f;
    public float intervalDecrement = 0.1f;

    public float mafiaInitialMinInterval = 20f;
    public float mafiaInitialMaxInterval = 30f;
    public float mafiaMinIntervalLimit = 10f;
    public float mafiaIntervalDecrement = 0.1f;

    private float currentMafiaMinInterval;
    private float currentMafiaMaxInterval;

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
        if (!IsServer) return;

        currentMinInterval = initialMinInterval;
        currentMaxInterval = initialMaxInterval;

        currentMafiaMinInterval = mafiaInitialMinInterval;
        currentMafiaMaxInterval = mafiaInitialMaxInterval;

        StartCoroutine(SpawnClients());
        StartCoroutine(SpawnMafioso());
    }
    private void Update()
    {
        if (!IsServer) return;

        if (mafia == null)
        {
            mafia = SpawnClient(mafiaPrefab);
            mafia.GetComponent<MafiaController>().GenerateOrder();
            mafia.SetActive(false);
        }
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
            float mafiaWaitTime = Random.Range(currentMafiaMinInterval, currentMafiaMaxInterval);

            yield return new WaitForSeconds(mafiaWaitTime);

            mafia.SetActive(true);

            if (currentMafiaMinInterval > mafiaMinIntervalLimit)
            {
                currentMafiaMinInterval = Mathf.Max(mafiaMinIntervalLimit, currentMafiaMinInterval - mafiaIntervalDecrement);
            }

            if (currentMafiaMaxInterval > mafiaMinIntervalLimit)
            {
                currentMafiaMaxInterval = Mathf.Max(mafiaMinIntervalLimit, currentMafiaMaxInterval - mafiaIntervalDecrement);
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

        if (client.TryGetComponent(out MafiaController mafiacontr))
        {
            mafiacontr.player = player;
        }

        client.GetComponent<NetworkObject>().Spawn(true);
        return client;
    }
}