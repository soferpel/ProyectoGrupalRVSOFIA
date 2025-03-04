using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class MyNetworkManager : MonoBehaviour
{
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        FullGameManager.Instance.gameState = FullGameManager.GAME_STATES.GameMultiplayer;
        NetworkManager.Singleton.SceneManager.LoadScene(FullGameManager.Instance.gameState.ToString(), LoadSceneMode.Single);
    }
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
    public void GoBack()
    {
        FullGameManager.Instance.gameState = FullGameManager.GAME_STATES.StartMenuGame;
        SceneManager.LoadScene(FullGameManager.Instance.gameState.ToString());
    }
}
