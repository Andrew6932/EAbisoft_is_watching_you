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
    private GameObject puzzleUI;
    private TextMeshProUGUI progressText;
    private Image progressBar;
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
        if (puzzleUI != null) return;

        // Создаем Canvas
        GameObject canvasObj = new GameObject("WaitPuzzleCanvas");
        canvasObj.layer = LayerMask.NameToLayer("UI");

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9998;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // Контейнер
        GameObject container = CreateUIElement("Container", canvasObj.transform);
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.7f);
        containerRect.anchorMax = new Vector2(0.5f, 0.7f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(400, 100);

        // Текст прогресса
        GameObject progressTextObj = CreateUIElement("ProgressText", container.transform);
        progressText = progressTextObj.AddComponent<TextMeshProUGUI>();

        if (textFont != null)
            progressText.font = textFont;

        progressText.fontSize = fontSize;
        progressText.color = progressColor;
        progressText.alignment = TextAlignmentOptions.Center;
        progressText.text = "Ожидание...";
        progressText.enableAutoSizing = false;

        RectTransform textRect = progressTextObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.6f);
        textRect.anchorMax = new Vector2(0.5f, 0.6f);
        textRect.sizeDelta = new Vector2(380, 30);

        // Прогресс бар (если включен)
        if (showProgress)
        {
            GameObject progressBarObj = CreateUIElement("ProgressBar", container.transform);

            // Фон прогресс бара
            GameObject bgObj = CreateUIElement("Background", progressBarObj.transform);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.3f, 0.3f, 0.3f, 0.7f);

            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 0f);
            bgRect.anchorMax = new Vector2(1f, 1f);
            bgRect.sizeDelta = Vector2.zero;

            // Заполнение прогресс бара
            GameObject fillObj = CreateUIElement("Fill", progressBarObj.transform);
            progressBar = fillObj.AddComponent<Image>();
            progressBar.color = progressColor;
            progressBar.type = Image.Type.Filled;
            progressBar.fillMethod = Image.FillMethod.Horizontal;
            progressBar.fillAmount = 0f;

            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.sizeDelta = Vector2.zero;

            RectTransform barRect = progressBarObj.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0.1f, 0.3f);
            barRect.anchorMax = new Vector2(0.9f, 0.4f);
            barRect.sizeDelta = new Vector2(0, 15);
        }

        puzzleUI = canvasObj;
        puzzleUI.SetActive(false);
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

        if (puzzleUI == null)
        {
            CreatePuzzleUI();
        }

        if (puzzleUI != null)
        {
            puzzleUI.SetActive(true);
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

        if (puzzleUI != null)
        {
            puzzleUI.SetActive(false);
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