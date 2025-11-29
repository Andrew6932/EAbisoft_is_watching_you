using System;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class GameManager : MonoBehaviour
{
    int gameCount = 0;
    float gameSuccessfulCount = 0f;
    TimeBar timeBar;
    GameCompletionBar gameCompletionBar;
    
    
    private void Start()
    {
        startGameIteration();
    }
    
    public void addGameCount()
    {
        gameCount++;
    }

    public void startGameIteration()
    {
        gameCompletionBar.setProgress(0, 0);
        timeBar.SetProgress(1f, -1f - (gameSuccessfulCount / 10f));
        
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