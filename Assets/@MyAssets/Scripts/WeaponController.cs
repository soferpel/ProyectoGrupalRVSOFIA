using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private Collider weaponCollider;
    // Start is called before the first frame update
    void Start()
    {
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
