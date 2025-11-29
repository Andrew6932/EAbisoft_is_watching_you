using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ColorSequencePuzzle : MonoBehaviour
{
    [Header("Sequence Settings")]
    public int sequenceLength = 4;
    public float showTime = 1f; // Время показа каждого цвета
    public float delayBetweenColors = 0.5f; // Задержка между цветами

    [Header("Colors")]
    public Color[] availableColors = new Color[]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        Color.magenta,
        Color.cyan
    };

    [Header("UI Settings")]
    public TMP_FontAsset textFont;
    public int fontSize = 24;
    public Color textColor = Color.white;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onPuzzleStart;
    public UnityEngine.Events.UnityEvent onPuzzleComplete;
    public UnityEngine.Events.UnityEvent onPuzzleFail;

    private List<int> correctSequence = new List<int>();
    private List<int> playerSequence = new List<int>();
    private bool isPuzzleActive = false;
    private bool isShowingSequence = false;
    private GameObject puzzleUI;
    private TextMeshProUGUI messageText;
    private GameObject[] colorButtons;
    private Image[] colorButtonImages;
    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        GenerateSequence();
    }

    void Update()
    {
        if (isPuzzleActive && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePuzzle();
        }
    }

    void GenerateSequence()
    {
        correctSequence.Clear();
        for (int i = 0; i < sequenceLength; i++)
        {
            int randomColorIndex = Random.Range(0, availableColors.Length);
            correctSequence.Add(randomColorIndex);
        }
        Debug.Log("Сгенерирована последовательность: " + string.Join(", ", correctSequence));
    }

    void CreatePuzzleUI()
    {
        if (puzzleUI != null) return;

        // Создаем Canvas
        GameObject canvasObj = new GameObject("ColorSequencePuzzleCanvas");
        canvasObj.layer = LayerMask.NameToLayer("UI");

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // Контейнер
        GameObject container = CreateUIElement("Container", canvasObj.transform);
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(600, 400);
        containerRect.anchoredPosition = Vector2.zero;

        // Текст сообщения
        GameObject messageDisplay = CreateUIElement("Message", container.transform);
        messageText = messageDisplay.AddComponent<TextMeshProUGUI>();

        if (textFont != null)
            messageText.font = textFont;

        messageText.fontSize = fontSize;
        messageText.color = textColor;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.fontStyle = FontStyles.Bold;
        messageText.text = "Запомните последовательность цветов!";
        messageText.enableAutoSizing = false;

        RectTransform messageRect = messageDisplay.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0.5f, 0.8f);
        messageRect.anchorMax = new Vector2(0.5f, 0.8f);
        messageRect.pivot = new Vector2(0.5f, 0.5f);
        messageRect.sizeDelta = new Vector2(580, 40);

        // Контейнер для кнопок цветов
        GameObject buttonsContainer = CreateUIElement("ButtonsContainer", container.transform);
        RectTransform buttonsRect = buttonsContainer.GetComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonsRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonsRect.pivot = new Vector2(0.5f, 0.5f);
        buttonsRect.sizeDelta = new Vector2(500, 200);

        // Создаем кнопки цветов
        colorButtons = new GameObject[availableColors.Length];
        colorButtonImages = new Image[availableColors.Length];

        float buttonSize = 80f;
        float spacing = 20f;
        float totalWidth = (availableColors.Length * buttonSize) + ((availableColors.Length - 1) * spacing);
        float startX = -totalWidth / 2 + buttonSize / 2;

        for (int i = 0; i < availableColors.Length; i++)
        {
            // Создаем кнопку
            GameObject buttonObj = CreateUIElement("ColorButton_" + i, buttonsContainer.transform);
            buttonObj.AddComponent<RectTransform>();
            buttonObj.AddComponent<CanvasRenderer>();

            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = availableColors[i];
            buttonImage.type = Image.Type.Sliced;

            // Добавляем кнопку UI
            Button button = buttonObj.AddComponent<Button>();
            int colorIndex = i; // Важно для замыкания
            button.onClick.AddListener(() => OnColorButtonClick(colorIndex));

            // Настраиваем позицию
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(buttonSize, buttonSize);
            buttonRect.anchoredPosition = new Vector2(startX + i * (buttonSize + spacing), 0);

            colorButtons[i] = buttonObj;
            colorButtonImages[i] = buttonImage;
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

    void OnColorButtonClick(int colorIndex)
    {
        if (!isPuzzleActive || isShowingSequence) return;

        playerSequence.Add(colorIndex);
        StartCoroutine(HighlightButton(colorIndex));

        // Проверяем правильность после каждого нажатия
        CheckSequence();
    }

    IEnumerator HighlightButton(int colorIndex)
    {
        Color originalColor = colorButtonImages[colorIndex].color;
        colorButtonImages[colorIndex].color = Color.white; // Подсветка
        yield return new WaitForSeconds(0.3f);
        colorButtonImages[colorIndex].color = originalColor;
    }

    void CheckSequence()
    {
        // Проверяем текущую последовательность
        for (int i = 0; i < playerSequence.Count; i++)
        {
            if (playerSequence[i] != correctSequence[i])
            {
                StartCoroutine(PuzzleFail());
                return;
            }
        }

        // Если последовательность полная и правильная
        if (playerSequence.Count == correctSequence.Count)
        {
            StartCoroutine(PuzzleComplete());
        }
        else
        {
            // Обновляем сообщение
            if (messageText != null)
            {
                messageText.text = $"Правильно! Осталось: {correctSequence.Count - playerSequence.Count}";
                messageText.color = Color.green;
            }
        }
    }

    IEnumerator ShowSequence()
    {
        isShowingSequence = true;

        if (messageText != null)
        {
            messageText.text = "Запоминайте последовательность...";
            messageText.color = Color.yellow;
        }

        // Блокируем кнопки на время показа
        SetButtonsInteractable(false);

        yield return new WaitForSeconds(1f);

        // Показываем последовательность
        for (int i = 0; i < correctSequence.Count; i++)
        {
            int colorIndex = correctSequence[i];
            Color originalColor = colorButtonImages[colorIndex].color;

            // Подсвечиваем кнопку
            colorButtonImages[colorIndex].color = Color.white;
            yield return new WaitForSeconds(showTime);

            // Возвращаем оригинальный цвет
            colorButtonImages[colorIndex].color = originalColor;
            yield return new WaitForSeconds(delayBetweenColors);
        }

        // Включаем кнопки для игрока
        SetButtonsInteractable(true);
        isShowingSequence = false;

        if (messageText != null)
        {
            messageText.text = "Повторите последовательность!";
            messageText.color = Color.white;
        }
    }

    void SetButtonsInteractable(bool interactable)
    {
        foreach (GameObject button in colorButtons)
        {
            if (button != null)
            {
                Button btn = button.GetComponent<Button>();
                if (btn != null)
                {
                    btn.interactable = interactable;
                }
            }
        }
    }

    IEnumerator PuzzleComplete()
    {
        if (messageText != null)
        {
            messageText.text = "✓ ПОСЛЕДОВАТЕЛЬНОСТЬ ВЕРНА!";
            messageText.color = Color.green;
        }

        // Мигаем правильной последовательностью
        yield return StartCoroutine(ShowSuccessAnimation());

        yield return new WaitForSeconds(2f);

        if (onPuzzleComplete != null)
        {
            onPuzzleComplete.Invoke();
        }

        ClosePuzzle();

        ObjectHighlighter highlighter = GetComponent<ObjectHighlighter>();
        if (highlighter != null)
        {
            highlighter.StopHighlight();
            highlighter.enabled = false;
        }
    }

    IEnumerator ShowSuccessAnimation()
    {
        for (int i = 0; i < 3; i++)
        {
            foreach (int colorIndex in correctSequence)
            {
                colorButtonImages[colorIndex].color = Color.white;
            }
            yield return new WaitForSeconds(0.2f);

            foreach (int colorIndex in correctSequence)
            {
                colorButtonImages[colorIndex].color = availableColors[colorIndex];
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator PuzzleFail()
    {
        if (messageText != null)
        {
            messageText.text = "✗ ОШИБКА! Попробуйте снова";
            messageText.color = Color.red;
        }

        // Мигаем красным
        foreach (var image in colorButtonImages)
        {
            image.color = Color.red;
        }
        yield return new WaitForSeconds(0.5f);

        // Возвращаем цвета
        for (int i = 0; i < colorButtonImages.Length; i++)
        {
            colorButtonImages[i].color = availableColors[i];
        }

        yield return new WaitForSeconds(1f);

        // Перезапускаем пазл
        playerSequence.Clear();
        StartCoroutine(ShowSequence());

        if (onPuzzleFail != null)
        {
            onPuzzleFail.Invoke();
        }
    }

    public void StartPuzzle()
    {
        Debug.Log("Запуск пазла с последовательностью: " + gameObject.name);

        isPuzzleActive = true;
        playerSequence.Clear();
        GenerateSequence();

        if (puzzleUI == null)
        {
            CreatePuzzleUI();
        }

        if (puzzleUI != null)
        {
            puzzleUI.SetActive(true);
        }

        SetPlayerControl(false);
        StartCoroutine(ShowSequence());

        if (onPuzzleStart != null)
        {
            onPuzzleStart.Invoke();
        }
    }

    public void ClosePuzzle()
    {
        isPuzzleActive = false;
        isShowingSequence = false;

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
    public void SetSequenceLength(int length)
    {
        sequenceLength = Mathf.Clamp(length, 2, 8);
        GenerateSequence();
    }

    public void SetShowTime(float time)
    {
        showTime = Mathf.Clamp(time, 0.5f, 3f);
    }
}