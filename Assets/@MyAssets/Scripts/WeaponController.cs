using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private Collider collider;
    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Weapon: OnCollisionEnter");

        if (collision.gameObject.GetComponent<ClientController>())
        {
            Debug.Log("Weapon: OnCollisionEnter Client");

            collider.isTrigger = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Weapon: OnTriggerExit");
        /*
        if (other.gameObject.GetComponent<ClientController>())
        {
            Debug.Log("Weapon: OnTriggerExit Client");

            collider.isTrigger = false;
        }*/
    }
    public void setColliderTrigger(bool isTrigger)
    {
        collider.isTrigger = isTrigger;
    }
}
