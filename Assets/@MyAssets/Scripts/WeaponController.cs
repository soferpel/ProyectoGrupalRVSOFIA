using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private Collider weaponCollider;
    public int durability = 100;  // Durabilidad inicial
    public int maxDurability = 100; // Durabilidad máxima
    public int repairCost = 50; // Costo de reparación
    public int currentDurability;  // Durabilidad actual

    // Start is called before the first frame update
    void Start()
    {
        currentDurability = durability;
        weaponCollider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void setColliderTrigger(bool isTrigger)
    {
        weaponCollider.isTrigger = isTrigger;
    }
}
