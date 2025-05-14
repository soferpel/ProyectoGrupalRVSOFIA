using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Netcode;

public class PlayerHandControllerMP : NetworkBehaviour
{
    [Header("Hand Interactors")]
    public XRDirectInteractor leftHandInteractor;
    public XRDirectInteractor rightHandInteractor;

    [Header("Hidden Weapons")]
    private GameObject leftHandItem;
    private GameObject rightHandItem;
    private GameObject weaponRight;
    private GameObject weaponLeft;

    private PlayerControls playerControls;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void Start()
    {
        // Solo configurar eventos si es el owner
        if (!IsOwner) return;

        leftHandInteractor.selectEntered.AddListener(OnLeftHandSelect);
        leftHandInteractor.selectExited.AddListener(OnLeftHandDeselect);
        rightHandInteractor.selectEntered.AddListener(OnRightHandSelect);
        rightHandInteractor.selectExited.AddListener(OnRightHandDeselect);
    }

    private void OnLeftHandSelect(SelectEnterEventArgs args)
    {
        if (!IsOwner) return;

        leftHandItem = args.interactableObject.transform.gameObject;
        Debug.Log("Left hand grabbed: " + leftHandItem.name);

        if (rightHandItem != null && rightHandItem.TryGetComponent<WeaponControllerMP>(out WeaponControllerMP weapon))
        {
            weapon.SetGrabbed(true);
        }
    }

    private void OnLeftHandDeselect(SelectExitEventArgs args)
    {
        if (!IsOwner) return;

        Debug.Log("Left hand released: " + (leftHandItem != null ? leftHandItem.name : "Nothing"));

        if (rightHandItem != null && rightHandItem.TryGetComponent<WeaponControllerMP>(out WeaponControllerMP weapon))
        {
            weapon.SetGrabbed(false);
        }

        leftHandItem = null;
    }

    private void OnRightHandSelect(SelectEnterEventArgs args)
    {
        if (!IsOwner) return;

        rightHandItem = args.interactableObject.transform.gameObject;
        Debug.Log("Right hand grabbed: " + rightHandItem.name);

        if (rightHandItem.TryGetComponent<WeaponControllerMP>(out WeaponControllerMP weapon))
        {
            weapon.SetGrabbed(true);
        }
    }

    private void OnRightHandDeselect(SelectExitEventArgs args)
    {
        if (!IsOwner) return;

        Debug.Log("Right hand released: " + (rightHandItem != null ? rightHandItem.name : "Nothing"));

        if (rightHandItem != null && rightHandItem.TryGetComponent<WeaponControllerMP>(out WeaponControllerMP weapon))
        {
            weapon.SetGrabbed(false);
        }

        rightHandItem = null;
    }

    public bool HasBodyPart()
    {
        if ((leftHandItem != null && leftHandItem.layer == LayerMask.NameToLayer("BodyParts")) ||
            (rightHandItem != null && rightHandItem.layer == LayerMask.NameToLayer("BodyParts")))
        {
            return true;
        }
        return false;
    }

    private void OnEnable()
    {
        if (!IsOwner) return;

        playerControls.Player.ClickB.performed += HideKnife;
        playerControls.Enable();
    }

    private void OnDisable()
    {
        if (!IsOwner) return;

        playerControls.Player.ClickB.performed -= HideKnife;
        playerControls.Disable();
    }

    private void HideKnife(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        // Hide right hand weapon
        if (rightHandItem != null && rightHandItem.TryGetComponent<WeaponControllerMP>(out _))
        {
            weaponRight = rightHandItem;
            rightHandItem.SetActive(false);
            rightHandItem = null;
            return;
        }

        // Show right hand weapon
        if (rightHandItem == null && weaponRight != null)
        {
            weaponRight.transform.position = rightHandInteractor.transform.position;
            weaponRight.SetActive(true);
            StartCoroutine(ForceGrabWeapon(weaponRight, rightHandInteractor));
            rightHandItem = weaponRight;
            weaponRight = null;
            return;
        }

        // Hide left hand weapon
        if (leftHandItem != null && leftHandItem.TryGetComponent<WeaponControllerMP>(out _))
        {
            weaponLeft = leftHandItem;
            leftHandItem.SetActive(false);
            leftHandItem = null;
            return;
        }

        // Show left hand weapon
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
            handInteractor.interactionManager.SelectEnter((IXRSelectInteractor)handInteractor, (IXRSelectInteractable)interactable);
        }
        else
        {
            Debug.LogWarning("El objeto no tiene un XRGrabInteractable.");
        }
    }

    public void ReceiveInitialWeapon(GameObject weapon)
    {
        if (!IsOwner) return;

        WeaponControllerMP weaponController = weapon.GetComponent<WeaponControllerMP>();
        if (weaponController != null)
        {
            weaponController.InitializeAsSpawnedWeapon();
        }
    }
}