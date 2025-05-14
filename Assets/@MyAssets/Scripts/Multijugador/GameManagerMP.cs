using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManagerMP : MonoBehaviour
{
    public static bool isGameOver = false;

    public static GameManagerMP Instance { get; private set; }

    public static float SurvivalTime { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
