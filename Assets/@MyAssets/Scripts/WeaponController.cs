using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private Collider weaponCollider;
    private SliceObject sliceObject;
    private OrderController orderController;
    public static bool isGrabbed = false;
    private Dictionary<ClientController, bool> clientStates = new Dictionary<ClientController, bool>();

    public int durability = 10;  
    public int maxDurability = 10;
    public int repairCost = 50; 
    public int currentDurability;
    public bool hasCut;

    public Material knifeMaterial;
    private Color maxDurabilityColor = Color.white;
    private Color minDurabilityColor = new Color(217f / 255f, 173f / 255f, 155f / 255f);

    void Start()
    {
        currentDurability = durability;
        weaponCollider = GetComponent<Collider>();
        sliceObject = GetComponent<SliceObject>();
        orderController = FindObjectOfType<OrderController>();

        if (sliceObject != null)
        {
            sliceObject.OnCutMade.AddListener(OnCutDetected);
        }
        UpdateKnifeColor();
    }
    void UpdateKnifeColor()
    {
        if (knifeMaterial != null)
        {
            float t = (float)currentDurability / maxDurability; 
            knifeMaterial.color = Color.Lerp(minDurabilityColor, maxDurabilityColor, t);
        }
    }
    public void setColliderTrigger(bool isTrigger)
    {
        weaponCollider.isTrigger = isTrigger;
    }

    public void RepairKnife() 
    {
        if (currentDurability <= 0)
        {
            if (orderController.cash >= repairCost) 
            {
                orderController.cash -= repairCost;
                currentDurability = maxDurability;
                sliceObject.enabled = true;
                UpdateKnifeColor();
                Debug.Log("El cuchillo ha sido reparado");
            }
            else
            {
                Debug.Log("No se dispone de suficiente dinero para reparar el arma");
            }
        }
    }
    private void DisableKnife()
    {
        if (sliceObject != null)
        {
            sliceObject.enabled = false;
            Debug.Log("El cuchillo está roto y ya no puede cortar");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isGrabbed)
        {
            ClientController client = other.GetComponent<ClientController>();
            if (client != null)
            {
                ProcessClient(client);
            }

        }
    }

    private void ProcessClient(ClientController client)
    {
        if (!clientStates.ContainsKey(client))
        {
            clientStates[client] = false; 
        }

        if (currentDurability > 0 && !clientStates[client])
        {
            currentDurability--; 
            Debug.Log($"Cuchillo usado en cliente {client.name}. Durabilidad actual: {currentDurability}");
            UpdateKnifeColor();
            clientStates[client] = true; 

            if (currentDurability <= 0)
            {
                Debug.Log("El cuchillo se ha roto.");
                DisableKnife();
            }
        }
        else if (clientStates[client])
        {
            Debug.Log($"El cliente {client.name} ya ha sido procesado. No puedes atacarlo nuevamente.");
        }
    }

    private void OnCutDetected()
    {
        if (currentDurability > 0)
        {
            currentDurability--;
            UpdateKnifeColor();
            Debug.Log("Corte detectado. Durabilidad actual: " + currentDurability);

            if (currentDurability <= 0)
            {
                DisableKnife();
            }
        }
    }
}
