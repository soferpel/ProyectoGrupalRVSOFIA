using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BodyPartScaler : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private Collider childCollider;

    private Vector3 targetScale = new Vector3(0.5f, 0.5f, 0.5f);
    private Vector3 originalScale;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        originalScale = transform.localScale;
        childCollider = GetComponentInChildren<Collider>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        /*if (IsBodyPart())
        {
            //childCollider.enabled = false;
            transform.localScale = targetScale;
            Debug.Log($"Escala de {gameObject.name} cambiada a {targetScale}");
        }*/
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        /*
        if (IsBodyPart())
        {
            transform.localScale = originalScale;
            //childCollider.enabled = true;
            Debug.Log($"Escala de {gameObject.name} cambiada a {originalScale}");
        }*/
    }

    private bool IsBodyPart()
    {
        string[] bodyPartTags = { "Torso", "Cabeza", "Pierna", "Brazo" };
        foreach (string tag in bodyPartTags)
        {
            if (CompareTag(tag))
            {
                return true;
            }
        }
        return false;
    }
}
