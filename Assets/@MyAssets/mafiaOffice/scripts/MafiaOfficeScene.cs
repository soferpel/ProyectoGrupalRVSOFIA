using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MafiaOfficeScene : MonoBehaviour
{
    void Start()
    {
        Invoke(nameof(LoadNextScene), 20f);
    }
    void LoadNextScene()
    {
        SceneManager.LoadScene("officemafia");
    }
}
