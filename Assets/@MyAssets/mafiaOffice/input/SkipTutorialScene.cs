using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SkipTutorialScene : MonoBehaviour
{
    private PlayerControls playerControls;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Player.skipIntro.performed += SkipIntro;
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Player.skipIntro.performed -= SkipIntro;
        playerControls.Disable();
    }

    private void SkipIntro(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene("StartMenuGame");
    }
}
