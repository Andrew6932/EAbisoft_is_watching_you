using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MathPuzzle : MonoBehaviour
{
    [Header("Math Puzzle Settings")]
    public int minNumber = 1;
    public int maxNumber = 20;
    public int maxOperations = 3;
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
    private GameObject puzzleContainer;
    private TextMeshProUGUI equationText;
    private TextMeshProUGUI inputText;
    private TextMeshProUGUI hintText;
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
        System.Text.StringBuilder equationBuilder = new System.Text.StringBuilder();
        int result = 0;
        int operationCount = Random.Range(1, maxOperations + 1);

        int currentNumber = Random.Range(minNumber, maxNumber + 1);
        equationBuilder.Append(currentNumber);
        result = currentNumber;

        for (int i = 0; i < operationCount; i++)
        {
            char operation = GetRandomOperation();
            int nextNumber = Random.Range(minNumber, maxNumber + 1);

            if (operation == '/' && (nextNumber == 0 || result % nextNumber != 0))
            {
                operation = '+';
                nextNumber = Random.Range(1, maxNumber + 1);
            }

            equationBuilder.Append(" ");
            equationBuilder.Append(operation);
            equationBuilder.Append(" ");
            equationBuilder.Append(nextNumber);

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
            availableOperations.Add('+');
        }

        return availableOperations[Random.Range(0, availableOperations.Count)];
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

        // Создаем контейнер внутри существующего Canvas
        puzzleContainer = CreateUIElement("MathPuzzleContainer", mainCanvas.transform);
        RectTransform containerRect = puzzleContainer.GetComponent<RectTransform>();

        // Настраиваем контейнер
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(600, 150);
        containerRect.anchoredPosition = new Vector2(0, 100);
        containerRect.localScale = Vector3.one; // Scale = 1

        // Добавляем фон для лучшей читаемости
        GameObject background = CreateUIElement("Background", puzzleContainer.transform);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.8f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.localScale = Vector3.one; // Scale = 1

        // Уравнение
        GameObject equationDisplay = CreateUIElement("Equation", puzzleContainer.transform);
        equationText = equationDisplay.AddComponent<TextMeshProUGUI>();
        equationText.text = mathEquation;

        if (textFont != null)
            equationText.font = textFont;

        equationText.fontSize = fontSize + 4;
        equationText.color = equationColor;
        equationText.alignment = TextAlignmentOptions.Center;
        equationText.fontStyle = FontStyles.Bold;
        equationText.enableAutoSizing = false;

        RectTransform equationRect = equationDisplay.GetComponent<RectTransform>();
        equationRect.anchorMin = new Vector2(0.5f, 0.6f);
        equationRect.anchorMax = new Vector2(0.5f, 0.6f);
        equationRect.pivot = new Vector2(0.5f, 0.5f);
        equationRect.sizeDelta = new Vector2(580, 40);
        equationRect.localScale = Vector3.one; // Scale = 1

        // Ввод ответа
        GameObject inputDisplay = CreateUIElement("Input", puzzleContainer.transform);
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
        inputRect.localScale = Vector3.one; // Scale = 1

        // Подсказка
        GameObject hintDisplay = CreateUIElement("Hint", puzzleContainer.transform);
        hintText = hintDisplay.AddComponent<TextMeshProUGUI>();
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
        hintRect.localScale = Vector3.one; // Scale = 1

        UpdateInputDisplay();
        puzzleContainer.SetActive(false);
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
        if (playerInput.Length < 6)
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
        GenerateMathProblem();

        if (puzzleContainer == null)
        {
            CreatePuzzleUI();
        }

        if (puzzleContainer != null)
        {
            puzzleContainer.SetActive(true);
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

    // Очистка при уничтожении
    void OnDestroy()
    {
        if (puzzleContainer != null)
        {
            Destroy(puzzleContainer);
        }
    }

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