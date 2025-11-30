using UnityEngine;
using TMPro;
using System.Collections;

public class TaskBarItem : MonoBehaviour
{
    public TextMeshProUGUI label;
    private bool isBlinking = false;
    private Coroutine blinkCoroutine;
    private Coroutine countdownCoroutine;
    private float countdownTime = 0f;
    private bool isCountdownActive = false;
    private string originalText;

    public string GetLabel()
    {
        return label.text;
    }

    public void EnableRedBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        blinkCoroutine = StartCoroutine(BlinkRed());
    }

    public void DisableBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        if (label != null)
        {
            label.color = Color.white;
        }
    }

    // Запуск обратного отсчета
    public void StartCountdown(float seconds)
    {
        countdownTime = seconds;
        originalText = "Consult with Manager"; // Сохраняем оригинальный текст
        isCountdownActive = true;

        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }
        countdownCoroutine = StartCoroutine(CountdownRoutine());
    }

    // Остановка обратного отсчета
    public void StopCountdown()
    {
        isCountdownActive = false;
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        // Возвращаем оригинальный текст без отсчета
        if (label != null && originalText != null)
        {
            label.text = originalText;
        }
    }

    private IEnumerator BlinkRed()
    {
        isBlinking = true;
        float blinkSpeed = 1.5f;

        while (isBlinking)
        {
            label.color = Color.red;
            yield return new WaitForSeconds(0.5f / blinkSpeed);
            label.color = new Color(1f, 0f, 0f, 0.3f);
            yield return new WaitForSeconds(0.5f / blinkSpeed);
        }
    }

    private IEnumerator CountdownRoutine()
    {
        while (countdownTime > 0f && isCountdownActive)
        {
            int currentSeconds = Mathf.CeilToInt(countdownTime);
            label.text = $"{originalText} ({currentSeconds}s)";
            countdownTime -= Time.deltaTime;
            yield return null;
        }

        // Когда время вышло - просто показываем 0 секунд
        // ObjectHighlighter сам обработает удаление задачи
        if (isCountdownActive && countdownTime <= 0f)
        {
            label.text = $"{originalText} (0s)";

            // Быстрое мигание в конце
            for (int i = 0; i < 4; i++)
            {
                label.color = new Color(1f, 0f, 0f, 0.3f);
                yield return new WaitForSeconds(0.2f);
                label.color = Color.red;
                yield return new WaitForSeconds(0.2f);
            }

            // НЕ удаляем автоматически - ObjectHighlighter сделает это
        }
    }

    private void RemoveFromTaskBar()
    {
        TaskBarMenu taskBarMenu = FindObjectOfType<TaskBarMenu>();
        if (taskBarMenu != null)
        {
            taskBarMenu.RemoveTaskBar(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        DisableBlinking();
        StopCountdown();
    }
}