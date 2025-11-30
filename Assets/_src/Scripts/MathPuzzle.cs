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
    private Canvas mainCanvas;
    private ObjectHighlighter highlighter;


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        highlighter = GetComponent<ObjectHighlighter>();
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

    public void GenerateMathProblem()
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

        if (puzzleContainer != null)
        {
            Destroy(puzzleContainer);
            puzzleContainer = null;
        }


        mainCanvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("Canvas не найден на сцене!");
            return;
        }

        mainCanvas.sortingOrder = 10;

        puzzleContainer = CreateUIElement("MathPuzzleContainer", mainCanvas.transform);
        RectTransform containerRect = puzzleContainer.GetComponent<RectTransform>();


        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(500, 150);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.localScale = Vector3.one;


        GameObject background = CreateUIElement("Background", puzzleContainer.transform);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.85f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        bgRect.localScale = Vector3.one;


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
        equationText.overflowMode = TextOverflowModes.Overflow;

        RectTransform equationRect = equationDisplay.GetComponent<RectTransform>();
        equationRect.anchorMin = new Vector2(0f, 0.66f);  
        equationRect.anchorMax = new Vector2(1f, 0.9f);   
        equationRect.offsetMin = new Vector2(10f, 0f);    
        equationRect.offsetMax = new Vector2(-10f, 0f);   
        equationRect.localScale = Vector3.one;


        GameObject inputDisplay = CreateUIElement("Input", puzzleContainer.transform);
        inputText = inputDisplay.AddComponent<TextMeshProUGUI>();

        if (textFont != null)
            inputText.font = textFont;

        inputText.fontSize = fontSize;
        inputText.color = inputColor;
        inputText.alignment = TextAlignmentOptions.Center;
        inputText.fontStyle = FontStyles.Bold;
        inputText.enableAutoSizing = false;
        inputText.overflowMode = TextOverflowModes.Overflow;
        inputText.text = "Ответ: _";

        RectTransform inputRect = inputDisplay.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0f, 0.33f); 
        inputRect.anchorMax = new Vector2(1f, 0.66f); 
        inputRect.offsetMin = new Vector2(10f, 0f);   
        inputRect.offsetMax = new Vector2(-10f, 0f); 
        inputRect.localScale = Vector3.one;


        GameObject hintDisplay = CreateUIElement("Hint", puzzleContainer.transform);
        hintText = hintDisplay.AddComponent<TextMeshProUGUI>();
        hintText.text = "Input and click Enter (ESC - exit)";

        if (textFont != null)
            hintText.font = textFont;

        hintText.fontSize = fontSize - 4;
        hintText.color = Color.gray;
        hintText.alignment = TextAlignmentOptions.Center;
        hintText.enableAutoSizing = false;

        RectTransform hintRect = hintDisplay.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0f, 0f);     
        hintRect.anchorMax = new Vector2(1f, 0.33f); 
        hintRect.offsetMin = new Vector2(10f, 5f);    
        hintRect.offsetMax = new Vector2(-10f, -5f);
        hintRect.localScale = Vector3.one;

        UpdateInputDisplay();
        puzzleContainer.SetActive(true); 
    }

    void CreateNewCanvas()
    {
        GameObject canvasObj = new GameObject("Canvas");
        mainCanvas = canvasObj.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();


        mainCanvas.sortingOrder = 100;
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
                inputText.text = "Solution: _";
            }
            else
            {
                inputText.text = "Solution: " + playerInput;
            }
        }
    }

    [Header("Progress Bar")]
    public GameCompletionBar progressBar;

    public void CheckAnswer()
    {
        if (int.TryParse(playerInput, out int playerAnswer))
        {
            if (playerAnswer == correctAnswer)
            {
                float progress = 30 + Random.Range(-7, 7);
                progressBar.addProgress(progress);
                StartCoroutine(PuzzleComplete());
            }
            else
            {
                float progress = Random.Range(-7, -2);
                progressBar.addProgress(progress);

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
            inputText.text = "Nice!: " + correctAnswer;
        }

        yield return new WaitForSeconds(0.1f);

        if (onPuzzleComplete != null)
        {
            onPuzzleComplete.Invoke();
        }

        ClosePuzzle();


        if (highlighter != null)
        {
            highlighter.OnPuzzleCompleted();
        }
    }

    IEnumerator PuzzleFail()
    {
        if (inputText != null)
        {
            inputText.color = Color.red;
            inputText.text = "Fail!: " + correctAnswer;
        }

        yield return new WaitForSeconds(1.5f);


        GenerateMathProblem();
        playerInput = "";


        CreatePuzzleUI();
        UpdateInputDisplay();

        if (onPuzzleFail != null)
        {
            onPuzzleFail.Invoke();
        }
    }

    public void StartPuzzle()
    {



        GenerateMathProblem();

        isPuzzleActive = true;
        playerInput = "";


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


        if (puzzleContainer != null)
        {
            Destroy(puzzleContainer);
            puzzleContainer = null;
        }


        if (mainCanvas != null && mainCanvas.gameObject.name == "Canvas")
        {
            mainCanvas.sortingOrder = 0;
        }

        SetPlayerControl(true);


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