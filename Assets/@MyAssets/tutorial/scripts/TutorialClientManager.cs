using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialClientManager : ClientManager
{
    private GameObject clientOrder = null;
    public TutorialManager tutorialManager;

    public GameObject clientOrderPrefab;
    public GameObject clientKillPrefab;
    public GameObject mafiaOrderPrefab;
    public bool spawnClientOrder = false;
    public bool spawnMafia = false;
    public Transform killPoint;

    protected override void Start()
    {

    }

    private void Update()
    {
        if (spawnClientOrder && clientOrder == null)
        {
            clientOrder = SpawnClient(clientOrderPrefab);
        }
        if (spawnMafia && mafia == null)
        {
            mafia = SpawnClient(mafiaOrderPrefab);
        }
    }
    public GameObject SpawnClient(GameObject prefab)
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

        if (client.TryGetComponent(out TutorialMafia mafiacontr))
        {
            mafiacontr.player = player;
            mafiacontr.tutorialManager = tutorialManager;
        }
        else if (client.TryGetComponent(out TutorialClientKill killcontr))
        {
            killcontr.killPoint = killPoint;
            killcontr.tutorialManager = tutorialManager;
        }
        else if (client.TryGetComponent(out TutorialClientOrder ordercontr))
        {

            ordercontr.tutorialManager = tutorialManager;
        }

        return client;
    }

    public void spawnClientKill()
    {
        SpawnClient(clientKillPrefab);
    }
    public void spawnMafiaOrder()
    {
        SpawnClient(mafiaOrderPrefab);
    }

}
