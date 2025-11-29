using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MathPuzzle : MonoBehaviour
{
    [Header("Math Puzzle Settings")]
    public int minNumber = 1;
    public int maxNumber = 20;
    public int maxOperations = 3; // Максимум операций в примере
    public bool useAddition = true;
    public bool useSubtraction = true;
    public bool useMultiplication = false;
    public bool useDivision = false;

    [Header("UI Settings")]
    public TMP_FontAsset textFont;
    public int fontSize = 28;
    public Color equationColor = Color.yellow;
    public Color inputColor = Color.green;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onPuzzleStart;
    public UnityEngine.Events.UnityEvent onPuzzleComplete;
    public UnityEngine.Events.UnityEvent onPuzzleFail;

    private string mathEquation = "";
    private int correctAnswer = 0;
    private string playerInput = "";
    private bool isPuzzleActive = false;
    private GameObject puzzleUI;
    private TextMeshProUGUI equationText;
    private TextMeshProUGUI inputText;
    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        GenerateMathProblem();
    }

    void Update()
    {
        if (isPuzzleActive)
        {
            HandleKeyboardInput();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClosePuzzle();
            }
        }
    }

    void GenerateMathProblem()
    {
        // Генерируем случайный математический пример
        System.Text.StringBuilder equationBuilder = new System.Text.StringBuilder();
        int result = 0;
        int operationCount = Random.Range(1, maxOperations + 1);

        // Первое число
        int currentNumber = Random.Range(minNumber, maxNumber + 1);
        equationBuilder.Append(currentNumber);
        result = currentNumber;

        // Генерируем операции
        for (int i = 0; i < operationCount; i++)
        {
            char operation = GetRandomOperation();
            int nextNumber = Random.Range(minNumber, maxNumber + 1);

            // Для деления проверяем делимость
            if (operation == '/' && (nextNumber == 0 || result % nextNumber != 0))
            {
                operation = '+'; // Заменяем деление на сложение если не делится
                nextNumber = Random.Range(1, maxNumber + 1);
            }

            equationBuilder.Append(" ");
            equationBuilder.Append(operation);
            equationBuilder.Append(" ");
            equationBuilder.Append(nextNumber);

            // Вычисляем результат
            switch (operation)
            {
                case '+': result += nextNumber; break;
                case '-': result -= nextNumber; break;
                case '*': result *= nextNumber; break;
                case '/': result /= nextNumber; break;
            }
        }

        equationBuilder.Append(" = ?");
        mathEquation = equationBuilder.ToString();
        correctAnswer = result;

        Debug.Log($"Сгенерирован пример: {mathEquation} Ответ: {correctAnswer}");
    }

    char GetRandomOperation()
    {
        System.Collections.Generic.List<char> availableOperations = new System.Collections.Generic.List<char>();

        if (useAddition) availableOperations.Add('+');
        if (useSubtraction) availableOperations.Add('-');
        if (useMultiplication) availableOperations.Add('*');
        if (useDivision) availableOperations.Add('/');

        if (availableOperations.Count == 0)
        {
            availableOperations.Add('+'); // По умолчанию сложение
        }

        return availableOperations[Random.Range(0, availableOperations.Count)];
    }

    void CreatePuzzleUI()
    {
        if (puzzleUI != null) return;

        // Создаем Canvas
        GameObject canvasObj = new GameObject("MathPuzzleCanvas");
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
        containerRect.sizeDelta = new Vector2(600, 150);
        containerRect.anchoredPosition = new Vector2(0, 100);

        // Уравнение
        GameObject equationDisplay = CreateUIElement("Equation", container.transform);
        equationText = equationDisplay.AddComponent<TextMeshProUGUI>();
        equationText.text = mathEquation;

        if (textFont != null)
            equationText.font = textFont;

        equationText.fontSize = fontSize + 4; // Чуть больше для уравнения
        equationText.color = equationColor;
        equationText.alignment = TextAlignmentOptions.Center;
        equationText.fontStyle = FontStyles.Bold;
        equationText.enableAutoSizing = false;

        RectTransform equationRect = equationDisplay.GetComponent<RectTransform>();
        equationRect.anchorMin = new Vector2(0.5f, 0.6f);
        equationRect.anchorMax = new Vector2(0.5f, 0.6f);
        equationRect.pivot = new Vector2(0.5f, 0.5f);
        equationRect.sizeDelta = new Vector2(580, 40);

        // Ввод ответа
        GameObject inputDisplay = CreateUIElement("Input", container.transform);
        inputText = inputDisplay.AddComponent<TextMeshProUGUI>();

        if (textFont != null)
            inputText.font = textFont;

        inputText.fontSize = fontSize;
        inputText.color = inputColor;
        inputText.alignment = TextAlignmentOptions.Center;
        inputText.fontStyle = FontStyles.Bold;
        inputText.enableAutoSizing = false;
        inputText.text = "Ответ: _";

        RectTransform inputRect = inputDisplay.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.5f, 0.4f);
        inputRect.anchorMax = new Vector2(0.5f, 0.4f);
        inputRect.pivot = new Vector2(0.5f, 0.5f);
        inputRect.sizeDelta = new Vector2(580, 35);

        // Подсказка
        GameObject hintDisplay = CreateUIElement("Hint", container.transform);
        TextMeshProUGUI hintText = hintDisplay.AddComponent<TextMeshProUGUI>();
        hintText.text = "Введите ответ и нажмите Enter";

        if (textFont != null)
            hintText.font = textFont;

        hintText.fontSize = fontSize - 4;
        hintText.color = Color.gray;
        hintText.alignment = TextAlignmentOptions.Center;
        hintText.enableAutoSizing = false;

        RectTransform hintRect = hintDisplay.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.5f, 0.2f);
        hintRect.anchorMax = new Vector2(0.5f, 0.2f);
        hintRect.pivot = new Vector2(0.5f, 0.5f);
        hintRect.sizeDelta = new Vector2(580, 25);

        puzzleUI = canvasObj;
        UpdateInputDisplay();
        puzzleUI.SetActive(false);
    }

    GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject element = new GameObject(name);
        element.transform.SetParent(parent);
        element.AddComponent<RectTransform>();
        return element;
    }

    void HandleKeyboardInput()
    {
        if (Input.anyKeyDown)
        {
            string input = Input.inputString;

            // Принимаем только цифры и минус (для отрицательных чисел)
            if (input.Length == 1 && (char.IsDigit(input[0]) || (input == "-" && playerInput.Length == 0)))
            {
                AddToInput(input);
            }
            else if (Input.GetKeyDown(KeyCode.Backspace))
            {
                RemoveLastCharacter();
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                CheckAnswer();
            }
        }
    }

    void AddToInput(string character)
    {
        // Ограничиваем длину ввода (включая возможный минус)
        if (playerInput.Length < 6) // Максимум 6 символов
        {
            playerInput += character;
            UpdateInputDisplay();
        }
    }

    void RemoveLastCharacter()
    {
        if (playerInput.Length > 0)
        {
            playerInput = playerInput.Remove(playerInput.Length - 1);
            UpdateInputDisplay();
        }
    }

    void UpdateInputDisplay()
    {
        if (inputText != null)
        {
            if (string.IsNullOrEmpty(playerInput))
            {
                inputText.text = "Ответ: _";
            }
            else
            {
                inputText.text = "Ответ: " + playerInput;
            }
        }
    }

    public void CheckAnswer()
    {
        if (int.TryParse(playerInput, out int playerAnswer))
        {
            if (playerAnswer == correctAnswer)
            {
                StartCoroutine(PuzzleComplete());
            }
            else
            {
                StartCoroutine(PuzzleFail());
            }
        }
        else
        {
            StartCoroutine(PuzzleFail());
        }
    }

    IEnumerator PuzzleComplete()
    {
        if (inputText != null)
        {
            inputText.color = Color.green;
            inputText.text = "✓ ВЕРНО! Ответ: " + correctAnswer;
        }

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

    IEnumerator PuzzleFail()
    {
        if (inputText != null)
        {
            inputText.color = Color.red;
            inputText.text = "✗ НЕВЕРНО! Ответ: " + correctAnswer;
        }

        yield return new WaitForSeconds(2f);

        // Генерируем новый пример при ошибке
        GenerateMathProblem();
        playerInput = "";
        UpdateInputDisplay();

        if (inputText != null)
        {
            inputText.color = inputColor;
        }

        if (equationText != null)
        {
            equationText.text = mathEquation;
        }

        if (onPuzzleFail != null)
        {
            onPuzzleFail.Invoke();
        }
    }

    public void StartPuzzle()
    {
        Debug.Log("Запуск математического пазла: " + gameObject.name);

        isPuzzleActive = true;
        playerInput = "";

        // Генерируем новый пример при каждом запуске
        GenerateMathProblem();

        if (puzzleUI == null)
        {
            CreatePuzzleUI();
        }

        if (puzzleUI != null)
        {
            puzzleUI.SetActive(true);
        }

        SetPlayerControl(false);
        UpdateInputDisplay();

        if (equationText != null)
        {
            equationText.text = mathEquation;
        }

        if (onPuzzleStart != null)
        {
            onPuzzleStart.Invoke();
        }
    }

    public void ClosePuzzle()
    {
        isPuzzleActive = false;

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
    public void SetDifficulty(int newMin, int newMax)
    {
        minNumber = newMin;
        maxNumber = newMax;
        GenerateMathProblem();
    }

    public void SetOperations(bool add, bool sub, bool mult, bool div)
    {
        useAddition = add;
        useSubtraction = sub;
        useMultiplication = mult;
        useDivision = div;
        GenerateMathProblem();
    }
}