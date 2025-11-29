using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;

public class SimpleCodePuzzle : MonoBehaviour
{
    [Header("Puzzle Settings")]
    public string targetCode = "123456";
    public int codeLength = 6;
    public bool generateRandomCode = true;

    [Header("UI Settings")]
    public Font textFont;
    public int fontSize = 24;
    public Color codeColor = Color.yellow;
    public Color inputColor = Color.green;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onPuzzleStart;
    public UnityEngine.Events.UnityEvent onPuzzleComplete;
    public UnityEngine.Events.UnityEvent onPuzzleFail;

    private string currentInput = "";
    private bool isPuzzleActive = false;
    private GameObject puzzleContainer;
    private Text codeDisplayText;
    private Text inputDisplayText;
    private GameObject player;

    void Start()
    {
        if (generateRandomCode)
        {
            GenerateRandomCode();
        }
        player = GameObject.FindGameObjectWithTag("Player");
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

    void GenerateRandomCode()
    {
        StringBuilder codeBuilder = new StringBuilder();
        string numbers = "0123456789";

        for (int i = 0; i < codeLength; i++)
        {
            int randomIndex = Random.Range(0, numbers.Length);
            codeBuilder.Append(numbers[randomIndex]);
        }

        targetCode = codeBuilder.ToString();
        Debug.Log("Сгенерирован код: " + targetCode);
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
        puzzleContainer = CreateUIElement("CodePuzzleContainer", mainCanvas.transform);
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
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.85f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero; // Важно!
        bgRect.offsetMax = Vector2.zero; // Важно!
        bgRect.localScale = Vector3.one;

        // Строка с кодом - позиционируем относительно контейнера
        GameObject codeDisplay = CreateUIElement("CodeDisplay", puzzleContainer.transform);
        codeDisplayText = codeDisplay.AddComponent<Text>();
        codeDisplayText.text = "КОД: " + targetCode;

        if (textFont != null)
            codeDisplayText.font = textFont;
        else
            codeDisplayText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        codeDisplayText.fontSize = 24;
        codeDisplayText.color = codeColor;
        codeDisplayText.alignment = TextAnchor.MiddleCenter;
        codeDisplayText.fontStyle = FontStyle.Bold;
        codeDisplayText.resizeTextForBestFit = false;
        codeDisplayText.horizontalOverflow = HorizontalWrapMode.Overflow;
        codeDisplayText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform codeRect = codeDisplay.GetComponent<RectTransform>();
        codeRect.anchorMin = new Vector2(0f, 0.66f);  // Слева, 2/3 высоты
        codeRect.anchorMax = new Vector2(1f, 0.9f);   // Справа, 90% высоты
        codeRect.offsetMin = new Vector2(10f, 0f);    // Отступ слева
        codeRect.offsetMax = new Vector2(-10f, 0f);   // Отступ справа
        codeRect.localScale = Vector3.one;

        // Строка с вводом - позиционируем относительно контейнера
        GameObject inputDisplay = CreateUIElement("InputDisplay", puzzleContainer.transform);
        inputDisplayText = inputDisplay.AddComponent<Text>();

        if (textFont != null)
            inputDisplayText.font = textFont;
        else
            inputDisplayText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        inputDisplayText.fontSize = 24;
        inputDisplayText.color = inputColor;
        inputDisplayText.alignment = TextAnchor.MiddleCenter;
        inputDisplayText.fontStyle = FontStyle.Bold;
        inputDisplayText.resizeTextForBestFit = false;
        inputDisplayText.horizontalOverflow = HorizontalWrapMode.Overflow;
        inputDisplayText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform inputRect = inputDisplay.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0f, 0.33f); // Слева, 1/3 высоты
        inputRect.anchorMax = new Vector2(1f, 0.66f); // Справа, 2/3 высоты
        inputRect.offsetMin = new Vector2(10f, 0f);   // Отступ слева
        inputRect.offsetMax = new Vector2(-10f, 0f);  // Отступ справа
        inputRect.localScale = Vector3.one;

        // Подсказка - позиционируем относительно контейнера
        GameObject hintDisplay = CreateUIElement("Hint", puzzleContainer.transform);
        Text hintText = hintDisplay.AddComponent<Text>();
        hintText.text = "Введите код и нажмите Enter (ESC - выход)";

        if (textFont != null)
            hintText.font = textFont;
        else
            hintText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        hintText.fontSize = 14;
        hintText.color = Color.gray;
        hintText.alignment = TextAnchor.MiddleCenter;

        RectTransform hintRect = hintDisplay.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0f, 0f);     // Слева снизу
        hintRect.anchorMax = new Vector2(1f, 0.33f);  // Справа, 1/3 высоты
        hintRect.offsetMin = new Vector2(10f, 5f);    // Отступы
        hintRect.offsetMax = new Vector2(-10f, -5f);
        hintRect.localScale = Vector3.one;

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

            if (input.Length == 1 && IsValidNumber(input[0]))
            {
                AddToInput(input);
            }
            else if (Input.GetKeyDown(KeyCode.Backspace))
            {
                RemoveLastCharacter();
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                CheckCode();
            }
        }
    }

    bool IsValidNumber(char c)
    {
        return c >= '0' && c <= '9';
    }

    void AddToInput(string character)
    {
        if (currentInput.Length < codeLength)
        {
            currentInput += character;
            UpdateInputDisplay();
        }
    }

    void RemoveLastCharacter()
    {
        if (currentInput.Length > 0)
        {
            currentInput = currentInput.Remove(currentInput.Length - 1);
            UpdateInputDisplay();
        }
    }

    void UpdateInputDisplay()
    {
        if (inputDisplayText != null)
        {
            string display = "";
            for (int i = 0; i < codeLength; i++)
            {
                if (i < currentInput.Length)
                {
                    display += currentInput[i];
                }
                else
                {
                    display += "_";
                }
            }
            inputDisplayText.text = "ВВОД: " + display;
        }
    }

    public void CheckCode()
    {
        if (currentInput == targetCode)
        {
            StartCoroutine(PuzzleComplete());
        }
        else
        {
            StartCoroutine(PuzzleFail());
        }
    }

    IEnumerator PuzzleComplete()
    {
        if (inputDisplayText != null)
        {
            inputDisplayText.color = Color.green;
            inputDisplayText.text = "ВВОД: УСПЕХ!";
        }

        yield return new WaitForSeconds(1.5f);

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
        if (inputDisplayText != null)
        {
            inputDisplayText.color = Color.red;
            inputDisplayText.text = "ВВОД: ОШИБКА!";
        }

        yield return new WaitForSeconds(1f);

        currentInput = "";
        UpdateInputDisplay();

        if (inputDisplayText != null)
        {
            inputDisplayText.color = inputColor;
        }

        if (onPuzzleFail != null)
        {
            onPuzzleFail.Invoke();
        }
    }

    public void StartPuzzle()
    {
        Debug.Log("Запуск пазла: " + gameObject.name);

        isPuzzleActive = true;
        currentInput = "";

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

        if (codeDisplayText != null)
        {
            codeDisplayText.text = "КОД: " + targetCode;
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

    public void SetTargetCode(string newCode)
    {
        targetCode = newCode;
        if (codeDisplayText != null)
        {
            codeDisplayText.text = "КОД: " + targetCode;
        }
    }
}