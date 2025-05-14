using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.SceneManagement;

public class FullGameManager : NetworkBehaviour
{
    public static FullGameManager Instance { get; private set; }

    public GAME_STATES gameState;
    public enum GAME_STATES
    {
        StartMenuGame = 0,
        LobbyScene = 1,
        GameMultiplayer = 2,
        GameOverScene = 3
    }

    public GameObject playerPrefab;
    public NetworkList<PlayerData> playerDataList;
    public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
    {
        public ulong clientId;
        public int skinIndex;
        public int hairStyleIndex;
        public int hairColorIndex;
        public bool isReady;
        public PlayerData(ulong id, int skinIndex, int hairStyleIndex, int hairColorIndex, bool ready)
        {
            clientId = id;
            this.skinIndex = skinIndex;
            this.hairStyleIndex = hairStyleIndex;
            this.hairColorIndex = hairColorIndex;
            isReady = ready;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref skinIndex);
            serializer.SerializeValue(ref hairStyleIndex);
            serializer.SerializeValue(ref hairColorIndex);
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
            return HashCode.Combine(clientId, skinIndex, hairStyleIndex, hairColorIndex, isReady);
        }
    }

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
                PlayerData newPlayerData = new PlayerData(playerDataList[i].clientId, playerDataList[i].skinIndex, playerDataList[i].hairStyleIndex, playerDataList[i].hairColorIndex, true);
                playerDataList[i] = newPlayerData;
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
                    allReady = false;
                    break;
                }
            }
            if (allReady)
            {
                NetworkManager.Singleton.SceneManager.OnLoadComplete += GameSceneLoaded;
                NetworkManager.Singleton.SceneManager.LoadScene(GAME_STATES.GameMultiplayer.ToString(), LoadSceneMode.Single);
            }
        }
    }

    private void GameSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        gameState = GAME_STATES.GameMultiplayer;
        foreach (PlayerData playerData in playerDataList)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(playerData.clientId, out var client))
            {
                var playerGO = client.PlayerObject.gameObject;
                playerGO.GetComponent<NetworkPlayer>().UpdateCharacterClientRpc(
                    playerData.skinIndex,
                    playerData.hairStyleIndex,
                    playerData.hairColorIndex
                );
            }
        }
    }

    public void GoToGameOver()
    {
        GoToGameOverRpc();
    }

    [Rpc(SendTo.Server)]
    private void GoToGameOverRpc()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete += GameOverSceneLoaded;
        NetworkManager.Singleton.SceneManager.LoadScene(GAME_STATES.GameOverScene.ToString(), LoadSceneMode.Single);
    }
    private void GameOverSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        gameState = GAME_STATES.GameOverScene;
    }

    public void SelectPlayer(int skinIndex, int hairStyleIndex, int hairColorIndex)
    {
        ulong localClientId = NetworkManager.Singleton.LocalClientId;
        SelectPlayerRpc(localClientId, skinIndex, hairStyleIndex, hairColorIndex);
    }

    [Rpc(SendTo.Server)]
    public void SelectPlayerRpc(ulong clientId, int skinIndex, int hairStyleIndex, int hairColorIndex)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            var playerGO = client.PlayerObject.gameObject;
            playerGO.GetComponent<NetworkPlayer>().UpdateCharacterClientRpc(
                skinIndex,
                hairStyleIndex,
                hairColorIndex
            );
        }
        for (int i = 0; i < playerDataList.Count; i++)
        {
            if (playerDataList[i].clientId == clientId)
            {
                playerDataList[i] = new PlayerData(clientId, skinIndex, hairStyleIndex, hairColorIndex, false);
                return;
            }
        }
        playerDataList.Add(new PlayerData(clientId, skinIndex, hairStyleIndex, hairColorIndex, false));
    }
    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;
        if (NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            EnableStartButtonsClientRpc();
        }
        bool exists = false;
        foreach (var player in playerDataList)
        {
            if (player.clientId == clientId)
            {
                exists = true;
                break;
            }
        }

        if (!exists)
        {
            playerDataList.Add(new PlayerData(clientId, 0, 0, 0, false));
        }
        foreach (var playerData in playerDataList)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(playerData.clientId, out var client))
            {
                var playerGO = client.PlayerObject.gameObject;
                playerGO.GetComponent<NetworkPlayer>().UpdateCharacterClientRpc(
                    playerData.skinIndex,
                    playerData.hairStyleIndex,
                    playerData.hairColorIndex
                );
            }
        }
    }
    [ClientRpc]
    public void EnableStartButtonsClientRpc()
    {
        CustomizePlayerMP ui = FindObjectOfType<CustomizePlayerMP>();
        if (ui != null)
            ui.EnableStartButton();
    }
}
