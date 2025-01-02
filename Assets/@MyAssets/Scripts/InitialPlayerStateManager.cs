using UnityEngine;

public class InitialPlayerStateManager : MonoBehaviour
{
    public bool isMovementLocked = true;
    public CharacterController characterController;
    public GameObject clientManager;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        int clientManagerActive = PlayerPrefs.GetInt("ClientManagerActive", 0);
        clientManager.SetActive(clientManagerActive == 1);

        UpdateMovementState();
    }

    public void UpdateMovementState()
    {
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
