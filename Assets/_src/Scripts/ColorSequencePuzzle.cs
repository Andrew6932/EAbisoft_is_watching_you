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
    private Canvas mainCanvas;
    private ObjectHighlighter highlighter;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        highlighter = GetComponent<ObjectHighlighter>();
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
        // Удаляем старый UI если есть
        if (puzzleUI != null)
        {
            Destroy(puzzleUI);
            puzzleUI = null;
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
        puzzleUI = CreateUIElement("ColorSequenceContainer", mainCanvas.transform);
        RectTransform containerRect = puzzleUI.GetComponent<RectTransform>();

        // Настраиваем контейнер - строго по центру экрана (исходные размеры)
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(500, 300); // Исходный размер
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.localScale = Vector3.one;

        // Добавляем фон для лучшей читаемости
        GameObject background = CreateUIElement("Background", puzzleUI.transform);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.85f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        bgRect.localScale = Vector3.one;

        // Текст сообщения - позиционируем относительно контейнера
        GameObject messageDisplay = CreateUIElement("Message", puzzleUI.transform);
        messageText = messageDisplay.AddComponent<TextMeshProUGUI>();

        if (textFont != null)
            messageText.font = textFont;

        messageText.fontSize = fontSize; // Исходный размер шрифта
        messageText.color = textColor;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.fontStyle = FontStyles.Bold;
        messageText.text = "Запомните последовательность цветов!";
        messageText.enableAutoSizing = false;
        messageText.overflowMode = TextOverflowModes.Overflow;

        RectTransform messageRect = messageDisplay.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0f, 0.75f);   // Слева, 3/4 высоты
        messageRect.anchorMax = new Vector2(1f, 0.95f);   // Справа, 95% высоты
        messageRect.offsetMin = new Vector2(10f, 0f);     // Отступ слева
        messageRect.offsetMax = new Vector2(-10f, 0f);    // Отступ справа
        messageRect.localScale = Vector3.one;

        // Контейнер для кнопок цветов - позиционируем относительно контейнера
        GameObject buttonsContainer = CreateUIElement("ButtonsContainer", puzzleUI.transform);
        RectTransform buttonsRect = buttonsContainer.GetComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0f, 0.3f);    // Слева, 30% высоты
        buttonsRect.anchorMax = new Vector2(1f, 0.75f);   // Справа, 75% высоты
        buttonsRect.offsetMin = new Vector2(20f, 10f);    // Отступы
        buttonsRect.offsetMax = new Vector2(-20f, -10f);
        buttonsRect.localScale = Vector3.one;

        // Создаем кнопки цветов
        colorButtons = new GameObject[availableColors.Length];
        colorButtonImages = new Image[availableColors.Length];

        float buttonSize = 70f; // Исходный размер кнопок
        float spacing = 15f;    // Исходный отступ
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

            // Настраиваем цвета кнопки
            ColorBlock colors = button.colors;
            colors.normalColor = availableColors[i];
            colors.highlightedColor = Color.Lerp(availableColors[i], Color.white, 0.3f);
            colors.pressedColor = Color.Lerp(availableColors[i], Color.black, 0.3f);
            colors.selectedColor = availableColors[i];
            colors.disabledColor = Color.Lerp(availableColors[i], Color.gray, 0.7f);
            button.colors = colors;

            int colorIndex = i; // Важно для замыкания
            button.onClick.AddListener(() => OnColorButtonClick(colorIndex));

            // НАСТРОЙКА ПОЗИЦИОНИРОВАНИЯ
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);  // Центр по X и Y
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);  // Центр по X и Y
            buttonRect.pivot = new Vector2(0.5f, 0.5f);      // Центр как точка привязки
            buttonRect.sizeDelta = new Vector2(buttonSize, buttonSize);
            // Позиционируем относительно центра контейнера
            buttonRect.anchoredPosition = new Vector2(startX + i * (buttonSize + spacing), 0);
            buttonRect.localScale = Vector3.one;

            colorButtons[i] = buttonObj;
            colorButtonImages[i] = buttonImage;
        }

        // Подсказка - позиционируем относительно контейнера
        GameObject hintDisplay = CreateUIElement("Hint", puzzleUI.transform);
        TextMeshProUGUI hintText = hintDisplay.AddComponent<TextMeshProUGUI>();
        hintText.text = "Повторите последовательность цветов (ESC - выход)";

        if (textFont != null)
            hintText.font = textFont;

        hintText.fontSize = fontSize - 4; // Исходный размер шрифта
        hintText.color = Color.gray;
        hintText.alignment = TextAlignmentOptions.Center;
        hintText.enableAutoSizing = false;

        RectTransform hintRect = hintDisplay.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0f, 0f);     // Слева снизу
        hintRect.anchorMax = new Vector2(1f, 0.3f);   // Справа, 30% высоты
        hintRect.offsetMin = new Vector2(10f, 5f);    // Отступы
        hintRect.offsetMax = new Vector2(-10f, -5f);
        hintRect.localScale = Vector3.one;

        puzzleUI.SetActive(true); // Показываем сразу
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

    [Header("Progress Bar")]
    public GameCompletionBar progressBar;
    IEnumerator PuzzleComplete()
    {
        if (messageText != null)
        {
            float progress = 20 + Random.Range(-7, 7);
            progressBar.addProgress(progress);
            messageText.text = "✓ ПОСЛЕДОВАТЕЛЬНОСТЬ ВЕРНА!";
            messageText.color = Color.green;
        }

        // Мигаем правильной последовательностью
        yield return StartCoroutine(ShowSuccessAnimation());

        yield return new WaitForSeconds(0.5f);

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

        // Перезапускаем пазл с новой последовательностью
        playerSequence.Clear();
        GenerateSequence(); // Генерируем новую последовательность
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

        // Всегда создаем новый UI
        CreatePuzzleUI();

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
            // Не удаляем полностью, чтобы можно было переиспользовать при ошибках
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
        if (puzzleUI != null)
        {
            Destroy(puzzleUI);
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

    public void SetAvailableColors(Color[] newColors)
    {
        if (newColors != null && newColors.Length > 0)
        {
            availableColors = newColors;
            // Пересоздаем UI если он активен
            if (isPuzzleActive && puzzleUI != null)
            {
                CreatePuzzleUI();
                StartCoroutine(ShowSequence());
            }
        }
    }

    public void AddColor(Color newColor)
    {
        List<Color> colorsList = new List<Color>(availableColors);
        colorsList.Add(newColor);
        availableColors = colorsList.ToArray();

        if (isPuzzleActive && puzzleUI != null)
        {
            CreatePuzzleUI();
            StartCoroutine(ShowSequence());
        }
    }

    public void ResetToDefaultColors()
    {
        availableColors = new Color[]
        {
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
            Color.magenta,
            Color.cyan
        };

        if (isPuzzleActive && puzzleUI != null)
        {
            CreatePuzzleUI();
            StartCoroutine(ShowSequence());
        }
    }
}