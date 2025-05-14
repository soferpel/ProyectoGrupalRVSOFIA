using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Netcode;
using System.Collections;

public class PermanentGrab : NetworkBehaviour
{
    private XRGrabInteractable grabInteractable;
    private bool isInitialGrab = true;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnSelectEntered);
            grabInteractable.selectExited.AddListener(OnSelectExited);
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        isInitialGrab = false;
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (!isInitialGrab && IsOwner)
        {
            StartCoroutine(ReGrabWeapon(args));
        }
    }

    private IEnumerator ReGrabWeapon(SelectExitEventArgs args)
    {
        yield return new WaitForEndOfFrame();

        if (args.interactorObject != null && grabInteractable != null)
        {
            if (args.interactorObject is XRDirectInteractor directInteractor)
            {
                if (directInteractor.interactionManager != null)
                {
                    directInteractor.interactionManager.SelectEnter((IXRSelectInteractor)directInteractor, (IXRSelectInteractable)grabInteractable);
                }
            }
        }
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
            grabInteractable.selectExited.RemoveListener(OnSelectExited);
        }
    }
}