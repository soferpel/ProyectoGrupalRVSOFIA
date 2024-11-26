using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OrderController : MonoBehaviour
{
    private XRSocketInteractor socketBox;
    private ClientController client;
    private BoxController boxController;
    public string boxTag = "Box";
    public GameObject counterPosition; //posicion en la que esperan los clientes (buypoint)

    void Start()
    {
        socketBox = GetComponent<XRSocketInteractor>();
        socketBox.selectEntered.AddListener(OnBoxOnSocket);
        client = FindObjectOfType<ClientController>();
    }

    void Update()
    {
    }

    private void OnBoxOnSocket(SelectEnterEventArgs args)
    {
        if (args.interactableObject.transform.CompareTag(boxTag))
        {
            Debug.Log("Caja en socket");


            if (args.interactableObject.transform.CompareTag(boxTag))
            {
                boxController = args.interactableObject.transform.GetComponent<BoxController>();
                Debug.Log(boxController.IsReadyForDelivery());
                if (client.isOnBuyPoint() && boxController.IsReadyForDelivery())
                {
                    Debug.Log("Objecto se ha destruido");
                    var lidSocket = boxController.socketLid.transform.gameObject;
                    var contentSocket = boxController.socketContent.transform.gameObject;
                    Destroy(args.interactableObject.transform.gameObject);
                    Destroy(lidSocket);
                    Destroy(contentSocket);
                }
            }

        }
    }
}
