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
    public UnityEvent OnSuccessfulJoin = new UnityEvent();

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApprovalCallBack;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.StartHost();

    }

    private void ConnectionApprovalCallBack(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        bool isLobbyScene = currentScene == FullGameManager.GAME_STATES.LobbyScene.ToString();
        bool isFull = NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT;

        response.Approved = isLobbyScene && !isFull;

        if (response.Approved)
        {
            response.CreatePlayerObject = true;
        }
        else
        {
            OnFailedJoin.Invoke();
            response.CreatePlayerObject = false;
        }
    }

    public void StartClient()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.StartClient();
    }

    public void GoBack()
    {
        FullGameManager.Instance.gameState = FullGameManager.GAME_STATES.StartMenuGame;
        SceneManager.LoadScene(FullGameManager.Instance.gameState.ToString());
    }
    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            OnSuccessfulJoin.Invoke();
        }
    }
    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback -= ConnectionApprovalCallBack;
        }
    }
}
