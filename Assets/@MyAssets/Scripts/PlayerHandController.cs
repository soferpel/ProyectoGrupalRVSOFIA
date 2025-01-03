using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerHandController : MonoBehaviour
{
    public XRDirectInteractor leftHandInteractor;
    public XRDirectInteractor rightHandInteractor;

    private GameObject leftHandItem;
    private GameObject rightHandItem;

    private void Start()
    {
        leftHandInteractor.selectEntered.AddListener(OnLeftHandSelect);
        leftHandInteractor.selectExited.AddListener(OnLeftHandDeselect);

        rightHandInteractor.selectEntered.AddListener(OnRightHandSelect);
        rightHandInteractor.selectExited.AddListener(OnRightHandDeselect);
    }

    private void OnLeftHandSelect(SelectEnterEventArgs args)
    {
        leftHandItem = args.interactableObject.transform.gameObject;
        Debug.Log("Left hand grabbed: " + leftHandItem.name);
    }

    private void OnLeftHandDeselect(SelectExitEventArgs args)
    {
        Debug.Log("Left hand released: " + (leftHandItem != null ? leftHandItem.name : "Nothing"));
        leftHandItem = null;
    }

    private void OnRightHandSelect(SelectEnterEventArgs args)
    {
        rightHandItem = args.interactableObject.transform.gameObject;
        Debug.Log("Right hand grabbed: " + rightHandItem.name);
    }

    private void OnRightHandDeselect(SelectExitEventArgs args)
    {
        Debug.Log("Right hand released: " + (rightHandItem != null ? rightHandItem.name : "Nothing"));
        rightHandItem = null;
    }

    public bool HasBodyPart()
    {
        if((leftHandItem != null && leftHandItem.layer == LayerMask.NameToLayer("BodyParts")) || (rightHandItem != null && rightHandItem.layer == LayerMask.NameToLayer("BodyParts")))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
