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
        /*rb = GetComponent<Rigidbody>();
        objCollider = GetComponent<Collider>(); 
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }*/
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        /*HashSet<Transform> visited = new HashSet<Transform>();

        Debug.Log("tag: "+gameObject.tag);
        if (!gameObject.CompareTag("Lid"))
        {
            SetColliderTrigger(transform, true, visited);
        }
        rb.isKinematic = true;*/
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        /*HashSet<Transform> visited = new HashSet<Transform>();
        Debug.Log("hola");
        SetColliderTrigger(transform, false, visited);
        rb.isKinematic = false;*/
    }

    private void SetColliderTrigger(Transform parent, bool isTrigger, HashSet<Transform> visited)

    {
       /* if (visited.Contains(parent)) return;
        visited.Add(parent);
        Collider parentCollider = parent.GetComponent<Collider>();
        if (parentCollider != null)
        {
            parentCollider.isTrigger = isTrigger;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            SetColliderTrigger(child, isTrigger, visited);
        }*/
    }
}
