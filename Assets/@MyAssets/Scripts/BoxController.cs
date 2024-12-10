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
                containedItems.Add(clothesTag); // Registrar ropa
                Debug.Log("Se ha colocado ropa");
                hasContent = true;
                hasClothes = true;
            }
            else if (IsBodyPart(placedObject.tag))
            {
                containedItems.Add(placedObject.tag); // Registrar partes del cuerpo
                Debug.Log($"Se ha colocado parte del cuerpo: {placedObject.name}");
                hasContent = true;

            //StartCoroutine(ScaleObject(placedObject, targetScale, scaleDuration));
            ScaleObjectInstantly(placedObject, targetScale);
                if (mafiaController != null)
                {
                    string order = mafiaController.getGeneratedOrder();
                    //string[] possibleContent = { "Clothes", "Torso", "Cabeza", "Pierna Izquierda", "Pierna Derecha", "Brazo Izquierdo", "Brazo Derecho" };
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

    /*
    private IEnumerator ScaleObject(GameObject obj, Vector3 targetScale, float duration)
    {
        Debug.Log($"Escalando objeto: {obj.name}");
        obj.transform.localScale = targetScale;
        Debug.Log($"Cambio manual a escala: {obj.transform.localScale}");
        Vector3 initialScale = obj.transform.localScale;
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            Debug.Log("Entra en el while");
            obj.transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ajustar al tama�o final para asegurar exactitud
        obj.transform.localScale = targetScale;
        Debug.Log($"Tama�o final de {obj.name}: {obj.transform.localScale}");

        Debug.Log("SE HA CAMBIADO TAMA�O PARTE??");
    }*/


    private void ScaleObjectInstantly(GameObject obj, Vector3 targetScale)
    {
        obj.transform.localScale = targetScale;
        Debug.Log($"Escala cambiada instant�neamente de {obj.name} a {obj.transform.localScale}");
    }
}
