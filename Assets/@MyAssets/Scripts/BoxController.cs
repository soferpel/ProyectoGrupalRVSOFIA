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
        GameObject placedObject = args.interactableObject.transform.gameObject;

        if (placedObject.CompareTag(lidTag))
        {
            Debug.Log("Se ha colocado la tapa");
            hasLid = true;

            if (!hasContent)
            {
                socketContent.socketActive = false;
            }
        }
        else if (placedObject.CompareTag(clothesTag))
        {
            Debug.Log("Se ha colocado ropa");
            hasContent = true;
            hasClothes = true;
        }
        else if (IsBodyPart(placedObject))
        {
            Debug.Log($"Se ha colocado parte del cuerpo: {placedObject.name}");

            //StartCoroutine(ScaleObject(placedObject, targetScale, scaleDuration));
            ScaleObjectInstantly(placedObject, targetScale);
            if (placedObject.CompareTag(order))
            {
                hasContent = true;
                Debug.Log("Parte del cuerpo coincide con el pedido.");
            }
        }
    }

    private bool IsBodyPart(GameObject obj)
    {
        string[] bodyPartTags = { "Torso", "Cabeza", "Pierna", "Brazo" };
        foreach (string tag in bodyPartTags)
        {
            if (obj.CompareTag(tag))
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

        // Ajustar al tamaño final para asegurar exactitud
        obj.transform.localScale = targetScale;
        Debug.Log($"Tamaño final de {obj.name}: {obj.transform.localScale}");

        Debug.Log("SE HA CAMBIADO TAMAÑO PARTE??");
    }*/


    private void ScaleObjectInstantly(GameObject obj, Vector3 targetScale)
    {
        obj.transform.localScale = targetScale;
        Debug.Log($"Escala cambiada instantáneamente de {obj.name} a {obj.transform.localScale}");
    }
}
