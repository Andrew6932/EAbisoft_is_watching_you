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
    private GameObject puzzleContainer; // Изменил имя с puzzleUI на puzzleContainer
    private TextMeshProUGUI progressText;
    private UnityEngine.UI.Image progressBar; // Указал полный namespace
    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        CreatePuzzleUI();
    }

    void Update()
    {
        if (isPuzzleActive && playerInZone)
        {
            UpdateWaitProgress();
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
        if (puzzleContainer != null) return;

        // Находим существующий Canvas
        Canvas mainCanvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("Canvas не найден на сцене!");
            return;
        }

        mainCanvas.sortingOrder = 10;

        // Создаем контейнер внутри существующего Canvas
        puzzleContainer = CreateUIElement("WaitTimeContainer", mainCanvas.transform);
        RectTransform containerRect = puzzleContainer.GetComponent<RectTransform>();

        // Настраиваем контейнер - строго по центру экрана
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(400, 120);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.localScale = Vector3.one;

        // Добавляем фон для лучшей читаемости
        GameObject background = CreateUIElement("Background", puzzleContainer.transform);
        UnityEngine.UI.Image bgImage = background.AddComponent<UnityEngine.UI.Image>(); // Указал полный namespace
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

        progressText.fontSize = fontSize;
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
            UnityEngine.UI.Image bgImageBar = bgObj.AddComponent<UnityEngine.UI.Image>(); // Указал полный namespace
            bgImageBar.color = new Color(0.3f, 0.3f, 0.3f, 0.7f);

            RectTransform bgRectBar = bgObj.GetComponent<RectTransform>();
            bgRectBar.anchorMin = Vector2.zero;
            bgRectBar.anchorMax = Vector2.one;
            bgRectBar.offsetMin = Vector2.zero;
            bgRectBar.offsetMax = Vector2.zero;
            bgRectBar.localScale = Vector3.one;

            // Заполнение прогресс бара
            GameObject fillObj = CreateUIElement("Fill", progressBarObj.transform);
            progressBar = fillObj.AddComponent<UnityEngine.UI.Image>(); // Указал полный namespace
            progressBar.color = progressColor;
            progressBar.type = UnityEngine.UI.Image.Type.Filled;
            progressBar.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
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
        TextMeshProUGUI hintText = hintDisplay.AddComponent<TextMeshProUGUI>();
        hintText.text = "Стойте в зоне ожидания (ESC - выход)";

        if (textFont != null)
            hintText.font = textFont;

        hintText.fontSize = fontSize - 4;
        hintText.color = Color.gray;
        hintText.alignment = TextAlignmentOptions.Center;
        hintText.enableAutoSizing = false;

        RectTransform hintRect = hintDisplay.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0f, 0f);     // Слева снизу
        hintRect.anchorMax = new Vector2(1f, 0.33f);  // Справа, 1/3 высоты
        hintRect.offsetMin = new Vector2(10f, 5f);    // Отступы
        hintRect.offsetMax = new Vector2(-10f, -5f);
        hintRect.localScale = Vector3.one;

        puzzleContainer.SetActive(false);
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
            progressText.text = $"Ожидание: {progress * 100:F0}%";
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

        yield return new WaitForSeconds(2f);

        if (onPuzzleComplete != null)
        {
            onPuzzleComplete.Invoke();
        }

        ClosePuzzle();

        // Деактивируем объект после завершения
        ObjectHighlighter highlighter = GetComponent<ObjectHighlighter>();
        if (highlighter != null)
        {
            highlighter.StopHighlight();
            highlighter.enabled = false;
        }
    }

    public void StartPuzzle()
    {
        Debug.Log("Запуск пазла ожидания: " + gameObject.name);

        isPuzzleActive = true;
        currentWaitTime = 0f;

        if (puzzleContainer == null)
        {
            CreatePuzzleUI();
        }

        if (puzzleContainer != null)
        {
            puzzleContainer.SetActive(true);
        }

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

        if (puzzleContainer != null)
        {
            puzzleContainer.SetActive(false);
        }

        SetPlayerControl(true);
    }

    void SetPlayerControl(bool enabled)
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            player.enabled = enabled;
        }
    }

    // Методы для настройки из других скриптов
    public void SetWaitTime(float newTime)
    {
        waitTimeRequired = newTime;
    }

    public void SetShowProgress(bool show)
    {
        showProgress = show;
    }
}