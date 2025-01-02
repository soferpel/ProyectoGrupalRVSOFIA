using UnityEngine;

public class InitialPlayerStateManager : MonoBehaviour
{
    public bool isMovementLocked = true;
    public CharacterController characterController;


    void Start()
    {
        characterController = GetComponent<CharacterController>();

        UpdateMovementState();
    }

    public void UpdateMovementState()
    {
        // Bloquear movimiento del cuerpo
        if (characterController != null)
            characterController.enabled = !isMovementLocked;
    }

    public void UnlockMovement()
    {
        isMovementLocked = false;
        UpdateMovementState();
    }

    public void LockMovement()
    {
        isMovementLocked = true;
        UpdateMovementState();
    }
}
