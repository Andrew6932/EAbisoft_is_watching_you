using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.SceneManagement;

public class UICode : MonoBehaviour
{
    [SerializeField]
    public void exitGame()
    {
        Application.Quit();
    }

    public void playGame()
    {
        SceneManager.LoadScene("SampleScene",LoadSceneMode.Single);
    }

    public void goMainMenu()
    {
        SceneManager.LoadScene("MainMenuScreen",LoadSceneMode.Single);
    }

    public void goAboutScreen()
    {
        SceneManager.LoadScene("AboutScreen",LoadSceneMode.Single);
    }
}