using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WaitTimePuzzle : MonoBehaviour
{
    [Header("Puzzle Settings")]
    public float waitTimeRequired = 5f; // Время в секундах
    public bool showProgress = true;

    [Header("UI Settings")]
    public TMP_FontAsset textFont;
    public int fontSize = 24;
    public Color progressColor = Color.cyan;
    public Color completeColor = Color.green;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onPuzzleStart;
    public UnityEngine.Events.UnityEvent onPuzzleComplete;
    public UnityEngine.Events.UnityEvent onPuzzleFail;

    private bool isPuzzleActive = false;
    private bool playerInZone = false;
    private float currentWaitTime = 0f;
    private GameObject puzzleContainer;
    private TextMeshProUGUI progressText;
    private Image progressBar;
    private TextMeshProUGUI hintText;
    private GameObject player;
    private Canvas mainCanvas;
    private ObjectHighlighter highlighter;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        highlighter = GetComponent<ObjectHighlighter>();
    }

    void Update()
    {
        if (isPuzzleActive && playerInZone)
        {
            UpdateWaitProgress();
        }

        if (isPuzzleActive && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePuzzle();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            if (isPuzzleActive)
            {
                StartWait();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            if (isPuzzleActive)
            {
                StopWait();
            }
        }
    }

    void CreatePuzzleUI()
    {
        // Удаляем старый UI если есть
        if (puzzleContainer != null)
        {
            Destroy(puzzleContainer);
            puzzleContainer = null;
        }

        // Находим существующий Canvas
        mainCanvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("Canvas не найден на сцене!");
            return;
        }

        mainCanvas.sortingOrder = 10;

        // Создаем контейнер внутри существующего Canvas
        puzzleContainer = CreateUIElement("WaitTimeContainer", mainCanvas.transform);
        RectTransform containerRect = puzzleContainer.GetComponent<RectTransform>();

        // Настраиваем контейнер - строго по центру экрана (исходные размеры)
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(400, 120); // Исходный размер
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.localScale = Vector3.one;

        // Добавляем фон для лучшей читаемости
        GameObject background = CreateUIElement("Background", puzzleContainer.transform);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.85f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        bgRect.localScale = Vector3.one;

        // Текст прогресса - позиционируем относительно контейнера
        GameObject progressTextObj = CreateUIElement("ProgressText", puzzleContainer.transform);
        progressText = progressTextObj.AddComponent<TextMeshProUGUI>();

        if (textFont != null)
            progressText.font = textFont;

        progressText.fontSize = fontSize; // Исходный размер шрифта
        progressText.color = progressColor;
        progressText.alignment = TextAlignmentOptions.Center;
        progressText.text = "Ожидание...";
        progressText.enableAutoSizing = false;
        progressText.overflowMode = TextOverflowModes.Overflow;

        RectTransform textRect = progressTextObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0.66f);  // Слева, 2/3 высоты
        textRect.anchorMax = new Vector2(1f, 0.9f);   // Справа, 90% высоты
        textRect.offsetMin = new Vector2(10f, 0f);    // Отступ слева
        textRect.offsetMax = new Vector2(-10f, 0f);   // Отступ справа
        textRect.localScale = Vector3.one;

        // Прогресс бар (если включен) - позиционируем относительно контейнера
        if (showProgress)
        {
            GameObject progressBarObj = CreateUIElement("ProgressBar", puzzleContainer.transform);

            // Фон прогресс бара
            GameObject bgObj = CreateUIElement("Background", progressBarObj.transform);
            Image bgImageBar = bgObj.AddComponent<Image>();
            bgImageBar.color = new Color(0.3f, 0.3f, 0.3f, 0.7f);

            RectTransform bgRectBar = bgObj.GetComponent<RectTransform>();
            bgRectBar.anchorMin = Vector2.zero;
            bgRectBar.anchorMax = Vector2.one;
            bgRectBar.offsetMin = Vector2.zero;
            bgRectBar.offsetMax = Vector2.zero;
            bgRectBar.localScale = Vector3.one;

            // Заполнение прогресс бара
            GameObject fillObj = CreateUIElement("Fill", progressBarObj.transform);
            progressBar = fillObj.AddComponent<Image>();
            progressBar.color = progressColor;
            progressBar.type = Image.Type.Filled;
            progressBar.fillMethod = Image.FillMethod.Horizontal;
            progressBar.fillAmount = 0f;

            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fillRect.localScale = Vector3.one;

            RectTransform barRect = progressBarObj.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 0.33f);   // Слева, 1/3 высоты
            barRect.anchorMax = new Vector2(1f, 0.66f);   // Справа, 2/3 высоты
            barRect.offsetMin = new Vector2(20f, 5f);     // Отступы
            barRect.offsetMax = new Vector2(-20f, -5f);
            barRect.localScale = Vector3.one;
        }

        // Подсказка - позиционируем относительно контейнера
        GameObject hintDisplay = CreateUIElement("Hint", puzzleContainer.transform);
        hintText = hintDisplay.AddComponent<TextMeshProUGUI>();
        hintText.text = "Стойте в зоне ожидания (ESC - выход)";

        if (textFont != null)
            hintText.font = textFont;

        hintText.fontSize = fontSize - 4; // Исходный размер шрифта
        hintText.color = Color.gray;
        hintText.alignment = TextAlignmentOptions.Center;
        hintText.enableAutoSizing = false;

        RectTransform hintRect = hintDisplay.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0f, 0f);     // Слева снизу
        hintRect.anchorMax = new Vector2(1f, 0.33f);  // Справа, 1/3 высоты
        hintRect.offsetMin = new Vector2(10f, 5f);    // Отступы
        hintRect.offsetMax = new Vector2(-10f, -5f);
        hintRect.localScale = Vector3.one;

        puzzleContainer.SetActive(true); // Показываем сразу
    }

    GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject element = new GameObject(name);
        element.transform.SetParent(parent);
        element.AddComponent<RectTransform>();
        return element;
    }

    void UpdateWaitProgress()
    {
        currentWaitTime += Time.deltaTime;

        if (showProgress && progressBar != null)
        {
            float progress = currentWaitTime / waitTimeRequired;
            progressBar.fillAmount = progress;
            progressText.text = $"Ожидание: {progress * 78:F0}%";
        }
        else if (progressText != null)
        {
            progressText.text = $"Ожидание: {currentWaitTime:F1}/{waitTimeRequired:F1}с";
        }

        // Проверяем завершение
        if (currentWaitTime >= waitTimeRequired)
        {
            CompletePuzzle();
        }
    }

    void StartWait()
    {
        if (progressText != null)
        {
            progressText.color = progressColor;
            progressText.text = "Ожидание...";
        }

        if (progressBar != null)
        {
            progressBar.fillAmount = 0f;
            progressBar.color = progressColor;
        }
    }

    void StopWait()
    {
        if (progressText != null)
        {
            progressText.color = Color.red;
            progressText.text = "Вернитесь в зону!";
        }
    }

    void CompletePuzzle()
    {
        StartCoroutine(PuzzleComplete());
    }

    IEnumerator PuzzleComplete()
    {
        if (progressText != null)
        {
            progressText.color = completeColor;
            progressText.text = "✓ ЗАДАНИЕ ВЫПОЛНЕНО!";
        }

        if (progressBar != null)
        {
            progressBar.color = completeColor;
            progressBar.fillAmount = 1f;
        }

        yield return new WaitForSeconds(2f);

        if (onPuzzleComplete != null)
        {
            onPuzzleComplete.Invoke();
        }

        ClosePuzzle();

        // Уведомляем ObjectHighlighter о завершении пазла
        if (highlighter != null)
        {
            highlighter.OnPuzzleCompleted();
        }
    }

    public void StartPuzzle()
    {
        Debug.Log("Запуск пазла ожидания: " + gameObject.name);

        isPuzzleActive = true;
        currentWaitTime = 0f;
        playerInZone = false;

        // Всегда создаем новый UI
        CreatePuzzleUI();

        SetPlayerControl(false);

        if (onPuzzleStart != null)
        {
            onPuzzleStart.Invoke();
        }
    }

    public void ClosePuzzle()
    {
        isPuzzleActive = false;
        playerInZone = false;
        currentWaitTime = 0f;

        if (puzzleContainer != null)
        {
            puzzleContainer.SetActive(false);
            // Не удаляем полностью, чтобы можно было переиспользовать
        }

        // Восстанавливаем нормальный sorting order при закрытии пазла
        if (mainCanvas != null)
        {
            mainCanvas.sortingOrder = 0;
        }

        SetPlayerControl(true);

        // Уведомляем ObjectHighlighter что пазл закрыт
        if (highlighter != null)
        {
            highlighter.OnPuzzleClosed();
        }
    }

    void SetPlayerControl(bool enabled)
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            player.enabled = enabled;
        }
    }

    // Очистка при уничтожении
    void OnDestroy()
    {
        if (puzzleContainer != null)
        {
            Destroy(puzzleContainer);
        }
    }

    // Методы для настройки из других скриптов
    public void SetWaitTime(float newTime)
    {
        waitTimeRequired = Mathf.Max(0.1f, newTime);
    }

    public void SetShowProgress(bool show)
    {
        showProgress = show;
        // Если пазл активен, пересоздаем UI с новыми настройками
        if (isPuzzleActive)
        {
            CreatePuzzleUI();
        }
    }

    public void SetProgressColor(Color newColor)
    {
        progressColor = newColor;
        if (isPuzzleActive && progressBar != null)
        {
            progressBar.color = progressColor;
        }
    }

    public void ResetProgress()
    {
        currentWaitTime = 0f;
        if (progressBar != null)
        {
            progressBar.fillAmount = 0f;
        }
        if (progressText != null)
        {
            progressText.text = "Ожидание...";
            progressText.color = progressColor;
        }
    }

    public float GetCurrentProgress()
    {
        return currentWaitTime / waitTimeRequired;
    }

    public bool IsPlayerInZone()
    {
        return playerInZone;
    }

    public bool IsPuzzleActive()
    {
        return isPuzzleActive;
    }
}