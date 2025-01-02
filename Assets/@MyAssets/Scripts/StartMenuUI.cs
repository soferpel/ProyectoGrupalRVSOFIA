using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class StartMenuUI : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject controlPanel;
    public GameObject clientManager;
    private void Start()
    {
        mainPanel.SetActive(true);
    }

    public void OnPlayButtonPressed()
    {
        Debug.Log("Jugar button pressed");
        PlayerPrefs.SetInt("ShowMainMenu", 0);
        PlayerPrefs.SetInt("ClientManagerActive", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Game 1", LoadSceneMode.Single);
    }

    public void OnTutorialButtonPressed()
    {
        Debug.Log("Tutorial button pressed");
        SceneManager.LoadScene("tutorial", LoadSceneMode.Single);
    }

    public void OnControlsButtonPressed()
    {
        Debug.Log("Controles button pressed");
        mainPanel.SetActive(false);
        controlPanel.SetActive(true);
    }

    public void OnBackButtonPressed()
    {
        Debug.Log("Volver button pressed");
        mainPanel.SetActive(true);
        controlPanel.SetActive(false);
    }
    public void OnExitButtonPressed()
    {
        Debug.Log("Salir button pressed");
        Application.Quit(); // solo se ve en un build
    }
}
