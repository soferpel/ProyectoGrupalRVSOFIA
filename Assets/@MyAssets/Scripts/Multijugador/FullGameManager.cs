using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.SceneManagement;

public class FullGameManager : NetworkBehaviour
{

    public enum GAME_STATES
    {
        StartMenuGame = 0,
        LobbyScene = 1,
        GameMultiplayer = 2,
        GameOverScene = 3
    }

    [SerializeField] private GameObject playerPrefab;
    public NetworkList<PlayerData> playerDataList ;
    public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
    {
        public ulong clientId;
        public bool isReady;
        public PlayerData(ulong id, bool ready)
        {
            clientId = id;
            isReady = ready;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref isReady);
        }

        public bool Equals(PlayerData other)
        {
            return clientId == other.clientId && isReady == other.isReady;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(clientId, isReady);
        }
    }
    public GAME_STATES gameState;
    public static FullGameManager Instance { get; private set; }

    public void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        gameState = GAME_STATES.LobbyScene;

        playerDataList = new NetworkList<PlayerData>();
    }

    public void GoToGame()
    {
        ulong localClientId = NetworkManager.Singleton.LocalClientId;
        Debug.Log("GoToGame "+ localClientId);
        GoToGameRpc(localClientId);
    }
    [Rpc(SendTo.Server)]
    public void GoToGameRpc(ulong clientId)
    {
        Debug.Log("GoToGameRPC");
        bool playerInList = false;
        for (int i = 0; i < playerDataList.Count; i++)
        {
            if (playerDataList[i].clientId == clientId)
            {
                playerInList = true;
                playerDataList[i] = new PlayerData(clientId, true);
                Debug.Log("Jugador en la lista");
                break;
            }
        }
        if (!playerInList)
        {
            playerDataList.Add(new PlayerData(clientId, true));
            Debug.Log("Jugador añadido");
        }
        Debug.Log("1: "+ NetworkManager.ConnectedClientsList.Count+ "   2:"+ playerDataList.Count);
        if (NetworkManager.ConnectedClientsList.Count > 1 && playerDataList.Count > 1)
        {
            Debug.Log("Todos conectados");
            bool allReady = true;
            for (int i = 0; i < playerDataList.Count; i++)
            {
                if (!playerDataList[i].isReady)
                {
                    Debug.Log("uno no esta ready");

                    allReady = false;
                    break;
                }
            }
            if (allReady)
            {
                Debug.Log("todos listos");

                NetworkManager.Singleton.SceneManager.OnLoadComplete += GameSceneLoaded;
                NetworkManager.Singleton.SceneManager.LoadScene(GAME_STATES.GameMultiplayer.ToString(), LoadSceneMode.Single);
            }
        }
    }

    private void GameSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        Debug.Log("escena cargada");

        gameState = GAME_STATES.GameMultiplayer;
        foreach (PlayerData playerData in playerDataList)
        {
           // GameObject playerGO = Instantiate(playerPrefabs[playerData.playerType]);
           // playerGO.GetComponent<NetworkObject>().SpawnAsPlayerObject(playerData.clientId, true);
        }
    }


    public void TriggerGameOver()
    {
        if (IsServer)
        {
            StartCoroutine(LoadGameOverScene());
        }
    }

    private IEnumerator LoadGameOverScene()
    {
        yield return new WaitForSeconds(1.5f);
        GoToGameOverClientRpc();
    }

    [ClientRpc]
    private void GoToGameOverClientRpc()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(GAME_STATES.GameOverScene.ToString(), LoadSceneMode.Single);
    }
}
