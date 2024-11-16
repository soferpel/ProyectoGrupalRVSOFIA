using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OrderController : MonoBehaviour
{
    private XRSocketInteractor socketBox;
    public string boxTag = "Box";
    public GameObject counterPosition; //posicion en la que esperan los clientes
    private bool isBoxOnSocket;

    // Start is called before the first frame update
    void Start()
    {
        socketBox = GetComponent<XRSocketInteractor>();
        socketBox.selectEntered.AddListener(OnBoxOnSocket);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnBoxOnSocket(SelectEnterEventArgs args)
    {//comprobar que la caja esta bien puesta con el producto adecuado
        if (args.interactableObject.transform.CompareTag(boxTag))
        {
            isBoxOnSocket = true;
            Debug.Log("Caja en socket");
        }
    }
    /*
    private void OnTriggerEnter(Collider other)
    {
        if(other)
    }
    /*
    private bool ClientOnCounter()
    {
        if ()
            return;
    }
    */
}
