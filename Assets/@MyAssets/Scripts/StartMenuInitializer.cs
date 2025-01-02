using UnityEngine;

public class StartMenuInitializer : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject clientManager;

    private void Start()
    {
        int showMenu = PlayerPrefs.GetInt("ShowMainMenu", 1);
        mainMenuPanel.SetActive(showMenu == 1);

        int clientManagerActive = PlayerPrefs.GetInt("ClientManagerActive", 0);
        clientManager.SetActive(clientManagerActive == 1);
    }
}
