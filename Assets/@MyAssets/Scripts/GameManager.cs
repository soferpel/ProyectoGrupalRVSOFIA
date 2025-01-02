using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static bool isGameOver = false;
    [SerializeField] private float delayBeforeSceneChange = 1.5f;

    public static GameManager Instance { get; private set; }

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

    private void Update()
    {
        if (isGameOver)
        {
            isGameOver = false;
            StartCoroutine(LoadGameOverScene());
        }
    }

    private IEnumerator LoadGameOverScene()
    {
        yield return new WaitForSeconds(delayBeforeSceneChange);
        SceneManager.LoadScene("GameOver");
    }
}
