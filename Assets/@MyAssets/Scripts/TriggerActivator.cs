using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TriggerActivator : MonoBehaviour
{
    private Rigidbody rb;
    private Collider objCollider;
    private XRGrabInteractable grabInteractable;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        objCollider = GetComponent<Collider>(); 
        grabInteractable = GetComponent<XRGrabInteractable>(); 

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        SetColliderTrigger(true);  
        rb.isKinematic = true;     
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        SetColliderTrigger(false); 
        rb.isKinematic = false;
    }

    private void SetColliderTrigger(bool isTrigger)
    {
        if (objCollider != null)
        {
            objCollider.isTrigger = isTrigger;
        }
    }
}
