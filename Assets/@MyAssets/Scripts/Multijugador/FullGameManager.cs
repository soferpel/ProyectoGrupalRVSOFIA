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
        GoToGameRpc(localClientId);
    }
    [Rpc(SendTo.Server)]
    public void GoToGameRpc(ulong clientId)
    {
        for (int i = 0; i < playerDataList.Count; i++)
        {
            if (playerDataList[i].clientId == clientId)
            {
                playerDataList[i] = new PlayerData(playerDataList[i].clientId, true);
                Debug.Log("jugador añad list");
                break;
            }
        }

        if (NetworkManager.ConnectedClientsList.Count > 1 && playerDataList.Count > 1)
        {
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
}
