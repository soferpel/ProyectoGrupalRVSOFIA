using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OrderController : MonoBehaviour
{
    private XRSocketInteractor socketBox;
    private ClientController client;
    public string boxTag = "Box";
    //private bool isBoxOnSocket;

    void Start()
    {
        socketBox = GetComponent<XRSocketInteractor>();
        client = FindObjectOfType<ClientController>();
        socketBox.selectEntered.AddListener(OnBoxOnSocket);
    }

    void Update()
    {
    }

    private void OnBoxOnSocket(SelectEnterEventArgs args)
    {
        if (args.interactableObject.transform.CompareTag(boxTag))
        {
            //isBoxOnSocket = true;
            if (client.isOnBuyPoint())
            {
                Debug.Log("Objecto se ha destruido");
                Destroy(args.interactableObject.transform.gameObject);
            }
        }
    }
}
