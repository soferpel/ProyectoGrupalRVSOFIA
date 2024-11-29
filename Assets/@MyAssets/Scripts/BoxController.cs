using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BoxController : MonoBehaviour
{
    public XRSocketInteractor socketLid;
    public XRSocketInteractor socketContent;
    private bool hasLid = false;
    private bool hasContent = false;
    public string lidTag = "Lid";
    public string contentTag = "Clothes";

    void Start()
    {
        if (socketLid == null || socketContent == null)
        {
            Debug.LogError("Socket references are not assigned in the Inspector.");
            return;
        }

        socketLid.selectEntered.AddListener(ObjectOnSocket);
        socketContent.selectEntered.AddListener(ObjectOnSocket);
    }

    void Update()
    {

    }

    private void ObjectOnSocket(SelectEnterEventArgs args)
    {
        if (args.interactableObject.transform.CompareTag(lidTag))
        {
            Debug.Log("Se ha colocado la tapa");
            hasLid = true;
            if (!hasContent)
            {
                socketContent.socketActive = false;
            }
        }
        else if (args.interactableObject.transform.CompareTag(contentTag))
        {
            hasContent = true;
            //comprobar para partes del cuerpo

        }
    }
    public bool IsReadyForDelivery()
    {
        return hasLid && hasContent;
    }
}