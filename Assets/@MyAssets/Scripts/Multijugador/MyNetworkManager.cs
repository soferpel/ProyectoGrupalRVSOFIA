using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Events;

public class MyNetworkManager : MonoBehaviour
{
    private const int MAX_PLAYER_AMOUNT = 2;
    public UnityEvent OnFailedJoin = new UnityEvent();
    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApprovalCallBack;
        Debug.Log("StartHost1");
        NetworkManager.Singleton.StartHost();
        Debug.Log("StartHost2");

        FullGameManager.Instance.GoToGame();
        Debug.Log("StartHost3");

        //FullGameManager.Instance.gameState = FullGameManager.GAME_STATES.GameMultiplayer;
        //NetworkManager.Singleton.SceneManager.LoadScene(FullGameManager.Instance.gameState.ToString(), LoadSceneMode.Single);
    }

    private void ConnectionApprovalCallBack(NetworkManager.ConnectionApprovalRequest rquest, NetworkManager.ConnectionApprovalResponse response)
    {
        if (SceneManager.GetActiveScene().name == FullGameManager.GAME_STATES.GameMultiplayer.ToString() || NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
            response.Approved = false;
        else
            response.Approved = true;
    }

    public void StartClient()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnectedCallBack;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.StartClient();
        FullGameManager.Instance.GoToGame();

    }

    private void OnClientConnected(ulong clientId)
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        FullGameManager.Instance.GoToGame();
    }
    private void ClientDisconnectedCallBack(ulong obj)
    {
        OnFailedJoin.Invoke();
    }

    public void GoBack()
    {
        FullGameManager.Instance.gameState = FullGameManager.GAME_STATES.StartMenuGame;
        SceneManager.LoadScene(FullGameManager.Instance.gameState.ToString());
    }
}
