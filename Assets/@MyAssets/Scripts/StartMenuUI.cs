using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class StartMenuUI : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject controlPanel;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPlayButtonPressed()
    {
        Debug.Log("Jugar button pressed");
        // yo creo que no funciona bien lo siguiente
        SceneManager.LoadScene("Game 1", LoadSceneMode.Single); // Reinicia la escena
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
