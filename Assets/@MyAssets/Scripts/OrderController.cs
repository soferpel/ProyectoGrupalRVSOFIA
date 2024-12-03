using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OrderController : MonoBehaviour
{
    private XRSocketInteractor socketBox;
    private ClientController client;
    private MafiaController mafia;
    private BoxController boxController;
    public int cash = 0;
    public string boxTag = "Box";

    public GameObject counterPosition; //(buypoint)

    void Start()
    {
        socketBox = GetComponent<XRSocketInteractor>();
        socketBox.selectEntered.AddListener(OnBoxOnSocket);
        client = FindObjectOfType<ClientController>();
        mafia = FindObjectOfType<MafiaController>();
        boxController = FindObjectOfType<BoxController>();
    }

    void Update()
    {
    }

    private void OnBoxOnSocket(SelectEnterEventArgs args)
    {
        if (args.interactableObject.transform.CompareTag(boxTag))
        {
            Debug.Log("Caja en socket");
            boxController = args.interactableObject.transform.GetComponent<BoxController>();
            if ((client.isOnBuyPoint() || mafia.isOnBuyPoint()) && boxController.IsReadyForDelivery())
            {
                Debug.Log("Objecto se ha destruido. Pedido entregado");
                var lidSocket = boxController.socketLid.GetOldestInteractableSelected()?.transform.gameObject;
                var contentSocket = boxController.socketContent.GetOldestInteractableSelected()?.transform.gameObject;
                Destroy(args.interactableObject.transform.gameObject);
                Destroy(lidSocket);
                Destroy(contentSocket);
                if (boxController.hasClothes)
                {
                    Debug.Log("Objecto se ha destruido. Pedido entregado ROPA");
                    AddCash();
                }
            }
        }
    }

    private int AddCash()
    {
        return cash += 10;
    }
}
