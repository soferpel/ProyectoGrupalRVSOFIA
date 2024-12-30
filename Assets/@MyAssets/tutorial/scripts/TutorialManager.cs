using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    private int tutorialStep = 0;
    public GameObject[] tutorialPanels;
    public TutorialClientManager clientManager;
    public GameObject weapon;
    void Start()
    {
        NextStep(0);
    }

    public void NextStep(int step)
    {
        if (step == tutorialStep)
        {
            tutorialStep++;

            tutorialPanels[tutorialStep - 1].SetActive(true);
            if(tutorialStep > 1) tutorialPanels[tutorialStep - 2].SetActive(false);
            switch (tutorialStep)
            {
                case 1:
               
                    break;

                case 2:
                    clientManager.spawnClientOrder = true;
                    break;

                case 3:
                    clientManager.spawnClientOrder = false;
                    clientManager.spawnClientKill();
                    weapon.SetActive(true);
                    break;

                case 4:
                    clientManager.spawnMafia = true;

                    break;

                case 5:
                    clientManager.spawnMafia = false;
                    Invoke(nameof(LoadNextScene), 5f);
                    break;
            }

        }

    }
    void LoadNextScene()
    {
        SceneManager.LoadScene("Game 1");
    }

}
