using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class WeaponControllerMP : NetworkBehaviour
{
    [Header("Weapon Settings")]
    public int durability = 10;
    public int maxDurability = 10;
    public int repairCost = 50;
    public Material knifeMaterial;

    [Header("Spawn Settings")]
    public bool isSpawnedWeapon = false;

    private Collider weaponCollider;
    private SliceObject sliceObject;
    private OrderController orderController;

    public NetworkVariable<bool> isGrabbed = new NetworkVariable<bool>(false);
    public NetworkVariable<int> currentDurability = new NetworkVariable<int>(10);

    private Dictionary<ClientController, bool> clientStates = new Dictionary<ClientController, bool>();

    // Colors for durability indication
    private Color maxDurabilityColor = Color.white;
    private Color minDurabilityColor = new Color(217f / 255f, 173f / 255f, 155f / 255f);

    void Start()
    {
        weaponCollider = GetComponent<Collider>();
        sliceObject = GetComponent<SliceObject>();
        orderController = FindObjectOfType<OrderController>();

        if (IsServer)
        {
            currentDurability.Value = durability;
            isGrabbed.Value = false;
        }

        if (sliceObject != null)
        {
            sliceObject.OnCutMade.AddListener(OnCutDetected);
        }

        currentDurability.OnValueChanged += OnDurabilityChanged;
        UpdateKnifeColor();
    }

    void OnDurabilityChanged(int previousValue, int newValue)
    {
        UpdateKnifeColor();
    }

    void UpdateKnifeColor()
    {
        if (knifeMaterial != null)
        {
            float t = (float)currentDurability.Value / maxDurability;
            knifeMaterial.color = Color.Lerp(minDurabilityColor, maxDurabilityColor, t);
        }
    }

    public void SetGrabbed(bool grabbed)
    {
        if (!IsOwner) return;

        SetGrabbedServerRpc(grabbed);
        SetColliderTrigger(grabbed);
    }

    private void SetColliderTrigger(bool isTrigger)
    {
        if (weaponCollider != null)
        {
            weaponCollider.isTrigger = isTrigger;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetGrabbedServerRpc(bool grabbed)
    {
        isGrabbed.Value = grabbed;
        SetGrabbedClientRpc(grabbed);
    }

    [ClientRpc]
    public void SetGrabbedClientRpc(bool grabbed)
    {
        SetColliderTrigger(grabbed);
    }

    public void RepairKnife()
    {
        if (!IsServer) return;

        if (currentDurability.Value <= 0)
        {
            if (orderController != null && orderController.cash >= repairCost)
            {
                orderController.cash -= repairCost;
                currentDurability.Value = maxDurability;

                if (sliceObject != null)
                {
                    sliceObject.enabled = true;
                }

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
        if (!IsServer) return;

        if (isGrabbed.Value)
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

        if (currentDurability.Value > 0 && !clientStates[client])
        {
            currentDurability.Value--;
            Debug.Log($"Cuchillo usado en cliente {client.name}. Durabilidad actual: {currentDurability.Value}");
            clientStates[client] = true;

            if (currentDurability.Value <= 0)
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
        if (!IsServer) return;

        if (currentDurability.Value > 0)
        {
            currentDurability.Value--;
            Debug.Log("Corte detectado. Durabilidad actual: " + currentDurability.Value);

            if (currentDurability.Value <= 0)
            {
                DisableKnife();
            }
        }
    }

    public void InitializeAsSpawnedWeapon()
    {
        isSpawnedWeapon = true;
    }
}