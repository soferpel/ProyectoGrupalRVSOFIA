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
    public float scaleDuration = 0.2f;
    public Vector3 targetScale = new Vector3(0.5f, 0.5f, 0.5f);

    public List<string> containedItems = new List<string>();


    void Start()
    {
        if (socketLid == null || socketContent == null)
        {
            Debug.LogError("Socket references are not assigned in the Inspector.");
            return;
        }
        mafiaController = FindObjectOfType<MafiaController>();

        socketLid.selectEntered.AddListener(ObjectOnSocket);
        socketLid.selectExited.AddListener(ObjectOutSocket);
        socketContent.selectEntered.AddListener(ObjectOnSocket);
       
    }

    void Update()
    {

    }


    private void ObjectOnSocket(SelectEnterEventArgs args)
    {
        GameObject placedObject = args.interactableObject.transform.gameObject;

            if (placedObject.CompareTag(lidTag))
            {
                Debug.Log("Se ha colocado la tapa");
                hasLid = true;
                if(!hasContent)
            {
                socketContent.socketActive = false;
            }
            }
            else if (placedObject.CompareTag(clothesTag))
            {
                containedItems.Add(clothesTag); 
                Debug.Log("Se ha colocado ropa");
                hasContent = true;
                hasClothes = true;
            }
            else if (IsBodyPart(placedObject.tag))
            {
                containedItems.Add(placedObject.tag); 
                Debug.Log($"Se ha colocado parte del cuerpo: {placedObject.name}");
                hasContent = true;

                if (mafiaController != null)
                {
                    string order = mafiaController.getGeneratedOrder();
                    if (placedObject.CompareTag(order))
                    {
                        Debug.Log("Parte del cuerpo coincide con el pedido.");
                    }
                }
            }
        
    }
    public void ObjectOutSocket(SelectExitEventArgs args)
    {
        Debug.Log("quitada tapa");
        hasLid = false;
        if (!hasContent)
        {
            socketContent.socketActive = true;
        }
    }

    private bool IsBodyPart(string tag)
    {
        string[] bodyPartTags = { "Torso", "Cabeza", "Pierna", "Brazo" };
        foreach (string bodyPartTag in bodyPartTags)
        {
            if (tag.Equals(bodyPartTag))
            {
                return true;
            }
        }
        return false;
    }

    public bool ContainsBodyPart()
    {
        foreach (string item in containedItems)
        {
            if (IsBodyPart(item))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsReadyForDelivery()
    {
        return hasLid && hasContent;
    }

}
