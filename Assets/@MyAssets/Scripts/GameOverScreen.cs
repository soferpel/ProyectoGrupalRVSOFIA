using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI survivalTimeText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    private void Start()
    {
        float survivalTime = GameManager.SurvivalTime;

        int minutes = Mathf.FloorToInt(survivalTime / 60f);
        int seconds = Mathf.FloorToInt(survivalTime % 60f);

        survivalTimeText.text = $"{minutes:00}:{seconds:00}";
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Game 1");
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("StartMenuGame");
    }
}
