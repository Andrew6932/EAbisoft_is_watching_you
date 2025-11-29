using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class GameManager : MonoBehaviour
{
    int gameCount = 0;
    float gameSuccessfulCount = 0f;
    public TimeBar timeBar;
    public GameCompletionBar gameCompletionBar;
    
    
    private void Start()
    {
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
        timeBar.SetProgress(1, -1f - (gameSuccessfulCount / 10f));
        yield return new WaitForSeconds(1f);
        
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
        if(gameCompletionBar.getProgress() <= 0.5f)
        {
            lostGame();
        }
        else
        {
            float percent = 1f - gameCompletionBar.getProgress();

        }
    }

    public void lostGame()
    {
        Debug.Log("Game Lost!");
    }

    public bool isFired(float percent)
    {
        System.Random rnd = new System.Random();
        float num = rnd.Next(1, 100) / 100;
        return num <= percent;
    }
}