using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static bool isGameOver = false;
    //[SerializeField] private string gameOverSceneName = "GameOver";
    [SerializeField] private float delayBeforeSceneChange = 2f;

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

        Debug.Log("Escena Cambiada");
        //SceneManager.LoadScene(gameOverSceneName);
    }
}
