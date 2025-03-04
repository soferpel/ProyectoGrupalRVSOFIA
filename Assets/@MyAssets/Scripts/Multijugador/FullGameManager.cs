using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.SceneManagement;

public class FullGameManager : NetworkBehaviour
{
    [SerializeField] private GameObject[] playerPrefabs;

    public enum GAME_STATES
    {
        StartMenuGame = 4,
        LobbyScene = 6,
        GameMultiplayer = 1,
        GameOverScene = 2
    }

    public GAME_STATES gameState;
    public static FullGameManager Instance { get; private set; }

    public void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        gameState = GAME_STATES.LobbyScene;
    }
}
