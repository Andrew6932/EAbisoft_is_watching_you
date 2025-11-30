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

    int missedManagerCalls = 0; 
    bool endIteration = false;

    private void Start()
    {
        gameCount = 0;
        gameSuccessfulCount = 0f;
        missedManagerCalls = 0;
        Debug.Log("GameManager Start");
        StartCoroutine(startGameIteration());
    }

    public void addGameCount()
    {
        gameCount++;
    }

    public void OnManagerCallMissed()
    {
        missedManagerCalls++;


        if (missedManagerCalls >= 2)
        {

            lostGame();
            missedManagerCalls = 0;
        }
        else
        {

            float currentProgress = gameCompletionBar.getProgress();
            float newProgress = Mathf.Max(0f, currentProgress - 0.05f);
            gameCompletionBar.setProgress(newProgress, 20f);
        }
    }

    public IEnumerator startGameIteration()
    {
        while(true){
            timeBar.setInstantProgressToOne();
            
            
            gameCompletionBar.setProgress(0f, 100);

            Debug.Log("Running GameManager");

            yield return new WaitForSeconds(1f);
            timeBar.SetProgress(0, 0.01f + (gameSuccessfulCount * 0.005f));
            yield return new WaitForSeconds(1f);
            endIteration = false;

        yield return new WaitForSeconds(2f);
        while (!endIteration)
        {
            checkTimers();
            checkGameCompletion();
            yield return new WaitForSeconds(1f);
        }
        Debug.Log("End of startGameManager Loop");
        yield return null;
        }
    }

    public void checkTimers()
    {
        if (timeBar.getProgress() == 0f)
        {
            Debug.Log(timeBar.getIsEmpty());
            endIteration = true;
            incompleteLaunch();
        }
    }

    public void checkGameCompletion()
    {
        if (gameCompletionBar.getProgress() >= 1f)
        {
            endIteration = true;
            CompleteGameIteration();
        }
    }

    public void CompleteGameIteration()
    {
        Debug.Log("Game iteration completed! Resetting progress and time.");
        timeBar.setInstantProgressToOne();
        gameCompletionBar.setProgress(0f, 100f);

        gameCount++;
        gameSuccessfulCount++;

        missedManagerCalls = 0;

        Debug.Log($"Iteration completed. Total games: {gameCount}, Successful: {gameSuccessfulCount}");
    }

    public void incompleteLaunch()
    {
        if (gameCompletionBar.getProgress() <= 0.5f)
        {
            Debug.Log("Problem with compBar");
            lostGame();
            return;
        }
        else
        {
            Debug.Log("ElSE");
            float percent = 1f - gameCompletionBar.getProgress();
            if (isFired(percent))
            {
                lostGame();
            }
            else
            {
                Debug.Log("&&&&&&&&&&&!");

                timeBar.setInstantProgressToOne();
                gameCompletionBar.setProgress(0f, 100f);

                gameCount++;
        

                missedManagerCalls = 0;
                endIteration = true;
                return;
            }
        }
    }

    public void lostGame()
    {
        Debug.Log("Game Lost!");
        SceneManager.LoadScene("LoseScreen", LoadSceneMode.Single);
    }

    public bool isFired(float percent)
    {
        System.Random rnd = new System.Random();
        float num = (float)rnd.Next(1, 100) / 100f;
        Debug.Log(num);
        Debug.Log(percent);
        return num <= percent;
    }
}