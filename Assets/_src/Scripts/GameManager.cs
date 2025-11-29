using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    int gameCount;
    float gameSuccessfulCount;
    public TimeBar timeBar;
    public GameCompletionBar gameCompletionBar;
    bool gameRelease;
    
    
    private void Start()
    {
        gameCount = 0;
        gameSuccessfulCount = 0f;
        gameRelease = false;
        Debug.Log("GameManager Start");
        StartCoroutine(startGameIteration());
    }
    
    public void addGameCount()
    {
        gameCount++;
    }

    public IEnumerator startGameIteration()
    {
        Debug.Log("Running GameManager");
        gameCompletionBar.setProgress(0f, 100);
        timeBar.SetProgress(0, 0.01f + (gameSuccessfulCount * 0.01f));
        yield return new WaitForSeconds(1f);
        while (true)
        {
            checkTimers();
            yield return new WaitForSeconds(1f);
            if (gameRelease)
            {
                gameRelease = false;
                break;
            }
        }
        Debug.Log("End of startGameManager");
        
    }

    public void checkTimers()
    {
        if (timeBar.getIsEmpty())
        {
            incompleteLaunch();
        }
    }

    public void incompleteLaunch()
    {
        gameRelease = true;
        if(gameCompletionBar.getProgress() <= 0.5f)
        {
            lostGame();
        }
        else
        {
            float percent = 1f - gameCompletionBar.getProgress();
            if (isFired(percent))
            {
                lostGame();
            }
        }
    }

    public void lostGame()
    {
        Debug.Log("Game Lost!");
        StopCoroutine(startGameIteration());
        SceneManager.LoadScene("LoseScreen",LoadSceneMode.Single);
    }

    public bool isFired(float percent)
    {
        System.Random rnd = new System.Random();
        float num = rnd.Next(1, 100) / 100;
        return num <= percent;
    }
}