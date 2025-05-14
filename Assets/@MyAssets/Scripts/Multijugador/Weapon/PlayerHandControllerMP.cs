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
        Debug.Log("PlayerHandControllerMP Awake");
        playerControls = new PlayerControls();
    }

    private void Start()
    {
        Debug.Log($"PlayerHandControllerMP Start - IsOwner: {IsOwner}");

        if (!IsOwner) return;

        if (leftHandInteractor == null)
        {
            Debug.LogError("LeftHandInteractor no está asignado!");
            return;
        }

        if (rightHandInteractor == null)
        {
            Debug.LogError("RightHandInteractor no está asignado!");
            return;
        }

        leftHandInteractor.selectEntered.AddListener(OnLeftHandSelect);
        leftHandInteractor.selectExited.AddListener(OnLeftHandDeselect);
        rightHandInteractor.selectEntered.AddListener(OnRightHandSelect);
        rightHandInteractor.selectExited.AddListener(OnRightHandDeselect);

        Debug.Log("Listeners configurados correctamente");
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
        Debug.Log($"PlayerHandControllerMP OnEnable - IsOwner: {IsOwner}");

        if (!IsOwner) return;

        if (playerControls == null)
        {
            Debug.LogError("PlayerControls es NULL!");
            return;
        }

        Debug.Log("Suscribiendo a ClickB");
        playerControls.Player.ClickB.performed += HideKnife;
        playerControls.Enable();
        Debug.Log("PlayerControls habilitado");
    }

    private void OnDisable()
    {
        Debug.Log($"PlayerHandControllerMP OnDisable - IsOwner: {IsOwner}");

        if (!IsOwner) return;

        if (playerControls != null)
        {
            playerControls.Player.ClickB.performed -= HideKnife;
            playerControls.Disable();
            Debug.Log("PlayerControls deshabilitado");
        }
    }

    private void HideKnife(InputAction.CallbackContext context)
    {
        Debug.Log("=== HideKnife INICIADO ===");
        Debug.Log($"IsOwner: {IsOwner}");

        if (!IsOwner)
        {
            Debug.LogError("HideKnife: No es owner, saliendo");
            return;
        }

        Debug.Log($"Estado inicial - RightHandItem: {(rightHandItem != null ? rightHandItem.name : "NULL")}");
        Debug.Log($"Estado inicial - WeaponRight: {(weaponRight != null ? weaponRight.name : "NULL")}");
        Debug.Log($"Estado inicial - LeftHandItem: {(leftHandItem != null ? leftHandItem.name : "NULL")}");
        Debug.Log($"Estado inicial - WeaponLeft: {(weaponLeft != null ? weaponLeft.name : "NULL")}");

        if (rightHandItem != null)
        {
            Debug.Log($"RightHandItem detectado: {rightHandItem.name}");

            if (rightHandItem.TryGetComponent<WeaponControllerMP>(out WeaponControllerMP rightWeapon))
            {
                Debug.Log("WeaponControllerMP encontrado en mano derecha - ESCONDIENDO");

                rightWeapon.SetGrabbed(false);
                Debug.Log("SetGrabbed(false) llamado");

                XRGrabInteractable interactable = rightHandItem.GetComponent<XRGrabInteractable>();
                if (interactable != null)
                {
                    Debug.Log("Deseleccionando arma manualmente");
                    rightHandInteractor.interactionManager.SelectExit((IXRSelectInteractor)rightHandInteractor, (IXRSelectInteractable)interactable);
                }
                else
                {
                    Debug.LogError("XRGrabInteractable no encontrado!");
                }

                // Esperar un frame antes de esconder
                StartCoroutine(HideWeaponCoroutine(rightHandItem, true));
                return;
            }
            else
            {
                Debug.LogWarning("WeaponControllerMP NO encontrado en rightHandItem");
            }
        }
        else
        {
            Debug.Log("RightHandItem es NULL");
        }

        if (rightHandItem == null && weaponRight != null)
        {
            Debug.Log("Mostrando arma derecha guardada");
            weaponRight.transform.position = rightHandInteractor.transform.position;
            weaponRight.SetActive(true);
            StartCoroutine(ForceGrabWeapon(weaponRight, rightHandInteractor));
            rightHandItem = weaponRight;
            weaponRight = null;
            return;
        }

        if (leftHandItem != null)
        {
            Debug.Log($"LeftHandItem detectado: {leftHandItem.name}");

            if (leftHandItem.TryGetComponent<WeaponControllerMP>(out WeaponControllerMP leftWeapon))
            {
                Debug.Log("WeaponControllerMP encontrado en mano izquierda - ESCONDIENDO");

                leftWeapon.SetGrabbed(false);
                Debug.Log("SetGrabbed(false) llamado");

                XRGrabInteractable interactable = leftHandItem.GetComponent<XRGrabInteractable>();
                if (interactable != null)
                {
                    Debug.Log("Deseleccionando arma manualmente");
                    leftHandInteractor.interactionManager.SelectExit((IXRSelectInteractor)leftHandInteractor, (IXRSelectInteractable)interactable);
                }
                else
                {
                    Debug.LogError("XRGrabInteractable no encontrado!");
                }

                StartCoroutine(HideWeaponCoroutine(leftHandItem, false));
                return;
            }
            else
            {
                Debug.LogWarning("WeaponControllerMP NO encontrado en leftHandItem");
            }
        }
        else
        {
            Debug.Log("LeftHandItem es NULL");
        }

        if (leftHandItem == null && weaponLeft != null)
        {
            Debug.Log("Mostrando arma izquierda guardada");
            weaponLeft.transform.position = leftHandInteractor.transform.position;
            weaponLeft.SetActive(true);
            StartCoroutine(ForceGrabWeapon(weaponLeft, leftHandInteractor));
            leftHandItem = weaponLeft;
            weaponLeft = null;
            return;
        }

        Debug.LogWarning("=== No hay armas para esconder/mostrar ===");
    }

    private IEnumerator HideWeaponCoroutine(GameObject weapon, bool isRightHand)
    {
        Debug.Log($"HideWeaponCoroutine iniciado para {weapon.name}");
        yield return new WaitForEndOfFrame();

        Debug.Log($"Ocultando arma: {weapon.name} - Active antes: {weapon.activeInHierarchy}");
        weapon.SetActive(false);
        Debug.Log($"Arma después de SetActive(false): {weapon.activeInHierarchy}");

        if (isRightHand)
        {
            weaponRight = weapon;
            rightHandItem = null;
            Debug.Log("Arma derecha guardada y rightHandItem = null");
        }
        else
        {
            weaponLeft = weapon;
            leftHandItem = null;
            Debug.Log("Arma izquierda guardada y leftHandItem = null");
        }

        Debug.Log($"HideWeaponCoroutine completado - Arma {(isRightHand ? "derecha" : "izquierda")} escondida");
    }

    private IEnumerator ForceGrabWeapon(GameObject weapon, XRDirectInteractor handInteractor)
    {
        Debug.Log($"ForceGrabWeapon iniciado para {weapon.name}");
        yield return new WaitForEndOfFrame();

        var interactable = weapon.GetComponent<XRGrabInteractable>();
        if (interactable != null)
        {
            Debug.Log("Forzando agarre del arma");
            handInteractor.interactionManager.SelectEnter((IXRSelectInteractor)handInteractor, (IXRSelectInteractable)interactable);

            if (weapon.TryGetComponent<WeaponControllerMP>(out WeaponControllerMP weaponController))
            {
                weaponController.SetGrabbed(true);
                Debug.Log("WeaponController.SetGrabbed(true) llamado");
            }
        }
        else
        {
            Debug.LogWarning("El objeto no tiene un XRGrabInteractable.");
        }
    }

    public void ReceiveInitialWeapon(GameObject weapon)
    {
        if (!IsOwner) return;

        Debug.Log($"ReceiveInitialWeapon: {weapon.name}");
        WeaponControllerMP weaponController = weapon.GetComponent<WeaponControllerMP>();
        if (weaponController != null)
        {
            weaponController.InitializeAsSpawnedWeapon();
        }
    }
}