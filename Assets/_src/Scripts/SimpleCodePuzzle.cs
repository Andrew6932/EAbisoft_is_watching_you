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

    [Header("UI Container")]
    public Transform uiContainer; // Перетащи сюда специальный объект для UI

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onPuzzleStart;
    public UnityEngine.Events.UnityEvent onPuzzleComplete;
    public UnityEngine.Events.UnityEvent onPuzzleFail;

    private string currentInput = "";
    private bool isPuzzleActive = false;
    private GameObject puzzleUI;
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

            // УБРАТЬ для Overlay: UpdateTextPosition();

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
        if (puzzleUI != null) return;

        // Создаем Canvas в Overlay режиме
        GameObject canvasObj = new GameObject("PuzzleCanvas");
        canvasObj.layer = LayerMask.NameToLayer("UI");

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Контейнер для центрирования
        GameObject container = CreateUIElement("Container", canvasObj.transform);
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(400, 100);
        containerRect.anchoredPosition = new Vector2(0, 150); // Выше по центру

        // Строка с кодом
        GameObject codeDisplay = CreateUIElement("CodeDisplay", container.transform);
        codeDisplayText = codeDisplay.AddComponent<Text>();
        codeDisplayText.text = "КОД: " + targetCode;

        if (textFont != null) codeDisplayText.font = textFont;
        else codeDisplayText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Увеличиваем шрифт и настраиваем для четкости
        codeDisplayText.fontSize = 22; // Чуть больше
        codeDisplayText.color = codeColor;
        codeDisplayText.alignment = TextAnchor.MiddleCenter;
        codeDisplayText.fontStyle = FontStyle.Bold;
        codeDisplayText.resizeTextForBestFit = false;
        codeDisplayText.horizontalOverflow = HorizontalWrapMode.Overflow;
        codeDisplayText.verticalOverflow = VerticalWrapMode.Overflow;
        codeDisplayText.supportRichText = false; // Отключаем rich text для четкости

        RectTransform codeRect = codeDisplay.GetComponent<RectTransform>();
        codeRect.anchorMin = new Vector2(0.5f, 0.6f);
        codeRect.anchorMax = new Vector2(0.5f, 0.6f);
        codeRect.pivot = new Vector2(0.5f, 0.5f);
        codeRect.sizeDelta = new Vector2(380, 30);

        // Строка с вводом
        GameObject inputDisplay = CreateUIElement("InputDisplay", container.transform);
        inputDisplayText = inputDisplay.AddComponent<Text>();

        if (textFont != null) inputDisplayText.font = textFont;
        else inputDisplayText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        inputDisplayText.fontSize = 22; // Чуть больше
        inputDisplayText.color = inputColor;
        inputDisplayText.alignment = TextAnchor.MiddleCenter;
        inputDisplayText.fontStyle = FontStyle.Bold;
        inputDisplayText.resizeTextForBestFit = false;
        inputDisplayText.horizontalOverflow = HorizontalWrapMode.Overflow;
        inputDisplayText.verticalOverflow = VerticalWrapMode.Overflow;
        inputDisplayText.supportRichText = false;

        RectTransform inputRect = inputDisplay.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.5f, 0.4f);
        inputRect.anchorMax = new Vector2(0.5f, 0.4f);
        inputRect.pivot = new Vector2(0.5f, 0.5f);
        inputRect.sizeDelta = new Vector2(380, 30);

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

    void UpdateTextPosition()
    {
        if (player == null || puzzleUI == null) return;

        Vector3 headPosition = player.transform.position + Vector3.up * 1.8f;
        puzzleUI.transform.position = headPosition;

        // Смотрит на камеру, но без переворота
        Camera cam = Camera.main;
        if (cam != null)
        {
            puzzleUI.transform.LookAt(puzzleUI.transform.position + cam.transform.forward);
        }
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

        // Создаем UI только когда игрок взаимодействует
        if (puzzleUI == null)
        {
            CreatePuzzleUI();
        }

        if (puzzleUI != null)
        {
            puzzleUI.SetActive(true);

            // УБРАТЬ World Space позиционирование для Overlay
            // if (player != null)
            // {
            //     Vector3 playerPosition = player.transform.position;
            //     puzzleUI.transform.position = playerPosition + new Vector3(0, 2f, 0);
            //
            //     Camera mainCamera = Camera.main;
            //     if (mainCamera != null)
            //     {
            //         puzzleUI.transform.rotation = mainCamera.transform.rotation;
            //     }
            // }
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

        if (puzzleUI != null)
        {
            // Вариант 1: Просто скрываем (можно использовать снова)
            puzzleUI.SetActive(false);

            // Вариант 2: Полностью уничтожаем (создаем заново при следующем взаимодействии)
            // Destroy(puzzleUI);
            // puzzleUI = null;
        }

        SetPlayerControl(true);
    }

    void SetPlayerControl(bool enabled)
    {
        // Заменяем устаревший метод
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            player.enabled = enabled;
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