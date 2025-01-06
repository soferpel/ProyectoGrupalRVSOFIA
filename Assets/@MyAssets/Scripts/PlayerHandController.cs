using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;

public class PlayerHandController : MonoBehaviour
{
    public XRDirectInteractor leftHandInteractor;
    public XRDirectInteractor rightHandInteractor;

    private GameObject leftHandItem;
    private GameObject rightHandItem;

    private PlayerControls playerControls;

    private GameObject weaponRight;
    private GameObject weaponLeft;


    private void Awake()
    {
        playerControls = new PlayerControls();
    }

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
        if (rightHandItem.TryGetComponent<WeaponController>(out _))
        {
            WeaponController.isGrabbed = true;
        }
    }

    private void OnLeftHandDeselect(SelectExitEventArgs args)
    {
        Debug.Log("Left hand released: " + (leftHandItem != null ? leftHandItem.name : "Nothing"));
        if (rightHandItem.TryGetComponent<WeaponController>(out _))
        {
            WeaponController.isGrabbed = false;
        }
        leftHandItem = null;
    }

    private void OnRightHandSelect(SelectEnterEventArgs args)
    {
        rightHandItem = args.interactableObject.transform.gameObject;
        Debug.Log("Right hand grabbed: " + rightHandItem.name);
        if(rightHandItem.TryGetComponent<WeaponController>(out _ ))
        {
            WeaponController.isGrabbed = true;
        }
    }

    private void OnRightHandDeselect(SelectExitEventArgs args)
    {
        Debug.Log("Right hand released: " + (rightHandItem != null ? rightHandItem.name : "Nothing"));
        if (rightHandItem.TryGetComponent<WeaponController>(out _))
        {
            WeaponController.isGrabbed = false;
        }
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

    private void OnEnable()
    {
        playerControls.Player.ClickB.performed += HideKnife;
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Player.ClickB.performed -= HideKnife;
        playerControls.Disable();
    }

    private void HideKnife(InputAction.CallbackContext context)
    {
        if(rightHandItem!=null && rightHandItem.TryGetComponent<WeaponController>(out _))
        {
            weaponRight = rightHandItem;
            rightHandItem.SetActive(false);
            rightHandItem = null;
            return;
        }

        if(rightHandItem == null && weaponRight != null)
        {
            weaponRight.transform.position = rightHandInteractor.transform.position;
            weaponRight.SetActive(true);
            StartCoroutine(ForceGrabWeapon(weaponRight, rightHandInteractor));
            rightHandItem = weaponRight;

            weaponRight = null;
            return;
        }

        if (leftHandItem != null && leftHandItem.TryGetComponent<WeaponController>(out _))
        {
            weaponLeft = leftHandItem;
            leftHandItem.SetActive(false);
            leftHandItem = null;
            return;
        }

        if (leftHandItem == null && weaponLeft != null)
        {
            weaponLeft.transform.position = leftHandInteractor.transform.position;
            weaponLeft.SetActive(true);
            StartCoroutine(ForceGrabWeapon(weaponLeft, leftHandInteractor));
            leftHandItem = weaponLeft;

            weaponLeft = null;
            return;
        }

    }

    private IEnumerator ForceGrabWeapon(GameObject weapon, XRDirectInteractor handInteractor)
    {
        yield return null;

        var interactable = weapon.GetComponent<XRGrabInteractable>();
        if (interactable != null)
        {
            handInteractor.interactionManager.SelectEnter(handInteractor, interactable);
        }
        else
        {
            Debug.LogWarning("El objeto no tiene un XRGrabInteractable.");
        }
    }
}
