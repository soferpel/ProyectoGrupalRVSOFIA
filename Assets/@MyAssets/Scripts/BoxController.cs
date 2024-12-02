using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BoxController : MonoBehaviour
{
    public XRSocketInteractor socketLid;
    public XRSocketInteractor socketContent;
    private MafiaController mafiaController;
    private bool hasLid = false;
    private bool hasContent = false;
    public bool hasClothes = false;
    public string lidTag = "Lid";
    public string clothesTag = "Clothes";


    void Start()
    {
        if (socketLid == null || socketContent == null)
        {
            Debug.LogError("Socket references are not assigned in the Inspector.");
            return;
        }
        mafiaController = FindObjectOfType<MafiaController>();
        socketLid.selectEntered.AddListener(ObjectOnSocket);
        socketContent.selectEntered.AddListener(ObjectOnSocket);
    }

    void Update()
    {

    }

    private void ObjectOnSocket(SelectEnterEventArgs args)
    {
        string order = mafiaController.getGeneratedOrder();
        //string[] possibleContent = { "Clothes", "Torso", "Cabeza", "Pierna Izquierda", "Pierna Derecha", "Brazo Izquierdo", "Brazo Derecho" };
       
        if (args.interactableObject.transform.CompareTag(lidTag))
        {
            Debug.Log("Se ha colocado la tapa");
            hasLid = true;
            if (!hasContent)
            {
                socketContent.socketActive = false;
            }
        }
        else if (args.interactableObject.transform.CompareTag(order)) //solo para partes del cuerpo (pedidos mafioso)
        {
            Debug.Log("Se ha colocado parte del cuerpo");
            hasContent = true;
        }else if (args.interactableObject.transform.CompareTag(clothesTag))
        {
            Debug.Log("Se ha colocado ropa");
            hasContent = true;
            hasClothes = true;
        }

        
    }
    public bool IsReadyForDelivery()
    {
        return hasLid && hasContent;
    }
}