using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MafiaController : PersonController
{

    public string appearanceDescription;
    public string orderDescription;
    
    protected override void Start()
    {
        buyProbability = 1;
        GenerateOrder();
        base.Start();
    }

    private void GenerateOrder()
    {
        string[] bodyParts = { "Torso", "Cabeza", "Pierna", "Brazo"};
        string selectedPart = bodyParts[Random.Range(0, bodyParts.Length)];
        orderDescription = selectedPart + "";
        Debug.Log("Pedido del mafioso: " + orderDescription);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<WeaponController>())
        {
            Debug.Log("El mafioso no puede ser atacado.");
        }
    }

    public string AppearanceDescription
    {
        get => appearanceDescription;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogError("La apariencia asignada es vacía o nula.");
            }
            else
            {
                appearanceDescription = value;
                Debug.Log("Apariencia del mafioso asignada: " + appearanceDescription);
            }
        }
    }
    void OnEnable()
    {
        Clothing.OnMafiaAppearanceGenerated += HandleAppearanceGenerated;
    }

    void OnDisable()
    {
        Clothing.OnMafiaAppearanceGenerated -= HandleAppearanceGenerated;
    }

    private void HandleAppearanceGenerated(string description)
    {
        AppearanceDescription = description;
    }

    public string getGeneratedOrder()
    {
        return orderDescription;
    }
}
