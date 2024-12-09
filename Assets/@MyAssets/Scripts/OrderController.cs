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
    private BuyPointController buyPointController;
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
        buyPointController = FindObjectOfType<BuyPointController>();
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
            Debug.Log(buyPointController.IsOccupied() + "    " + boxController.IsReadyForDelivery());
            if (buyPointController.IsOccupied() && boxController.IsReadyForDelivery())
            {
                PersonController currentCustomer = buyPointController.currentCustomer;
                if (currentCustomer is ClientController)
                {
                    Debug.Log("cleinte");
                    ClientOrder(args);
                }
                else if (currentCustomer is MafiaController)
                {
                    Debug.Log("mafia");
                    MafiaOrder(args);
                }
            }
        }
    }


    private void ClientOrder(SelectEnterEventArgs args)
    {
        if (boxController.hasClothes)
        {
            Debug.Log("Pedido correcto. Se entrega al cliente");
            DestroyOrder(args);
            AddCash();
        }else if (boxController.ContainsBodyPart())
        {
            GameManager.isGameOver = true;
        }
        else
        {
            Debug.Log("Pedido incorrecto. No se entrega al cliente");
        }
    }
    private void MafiaOrder(SelectEnterEventArgs args)
    {
        MafiaController mafia = buyPointController.currentCustomer as MafiaController;
        string requiredItem = mafia.getGeneratedOrder();

        if (boxController.containedItems.Contains(requiredItem))
        {
            Debug.Log("Pedido correcto. Se entrega al mafioso");
            DestroyOrder(args);
        }
        else if (boxController.hasClothes)
        {
            GameManager.isGameOver = true;
            Debug.Log("Pedido incorrecto. GameOver");
        }
        else
        {
            Debug.Log("Pedido incorrecto. No se entrega al mafioso");
        }
    }

    private void DestroyOrder(SelectEnterEventArgs args)
    {
        var lidSocket = boxController.socketLid.GetOldestInteractableSelected()?.transform.gameObject;
        var contentSocket = boxController.socketContent.GetOldestInteractableSelected()?.transform.gameObject;
        Destroy(args.interactableObject.transform.gameObject);
        Destroy(lidSocket);
        Destroy(contentSocket);

        buyPointController.FreePoint(); 
    }

    private int AddCash()
    {
        return cash += 10;
    }
}
