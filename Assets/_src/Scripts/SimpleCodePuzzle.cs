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
    private Text hintText;
    private GameObject player;
    private Canvas mainCanvas;
    private ObjectHighlighter highlighter;
    [Header("Progress Bar")]
    public GameCompletionBar progressBar;


    void Start()
    {
        if (generateRandomCode)
        {
            GenerateRandomCode();
        }
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

        puzzleContainer = CreateUIElement("CodePuzzleContainer", mainCanvas.transform);
        RectTransform containerRect = puzzleContainer.GetComponent<RectTransform>();


        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(400, 120); 
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

 
        GameObject codeDisplay = CreateUIElement("CodeDisplay", puzzleContainer.transform);
        codeDisplayText = codeDisplay.AddComponent<Text>();
        codeDisplayText.text = "Code: " + targetCode;

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
        codeRect.anchorMin = new Vector2(0f, 0.66f);  
        codeRect.anchorMax = new Vector2(1f, 0.9f);   
        codeRect.offsetMin = new Vector2(10f, 0f);    
        codeRect.offsetMax = new Vector2(-10f, 0f); 
        codeRect.localScale = Vector3.one;


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
        inputRect.anchorMin = new Vector2(0f, 0.33f); 
        inputRect.anchorMax = new Vector2(1f, 0.66f); 
        inputRect.offsetMin = new Vector2(10f, 0f);   
        inputRect.offsetMax = new Vector2(-10f, 0f); 
        inputRect.localScale = Vector3.one;

        // Подсказка - позиционируем относительно контейнера
        GameObject hintDisplay = CreateUIElement("Hint", puzzleContainer.transform);
        hintText = hintDisplay.AddComponent<Text>();
        hintText.text = "(ESC - exit)";

        if (textFont != null)
            hintText.font = textFont;
        else
            hintText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        hintText.fontSize = 14;
        hintText.color = Color.gray;
        hintText.alignment = TextAnchor.MiddleCenter;

        RectTransform hintRect = hintDisplay.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0f, 0f);     
        hintRect.anchorMax = new Vector2(1f, 0.33f);  
        hintRect.offsetMin = new Vector2(10f, 5f);    
        hintRect.offsetMax = new Vector2(-10f, -5f);
        hintRect.localScale = Vector3.one;

        UpdateInputDisplay();
        puzzleContainer.SetActive(true);
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
            inputDisplayText.text = "Input: " + display;
        }
    }

    public void CheckCode()
    {
        if (currentInput == targetCode)
        {
            float progress = 20 + Random.Range(-7, 7);
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

    IEnumerator PuzzleComplete()
    {
        if (inputDisplayText != null)
        {
            inputDisplayText.color = Color.green;
            inputDisplayText.text = "Input: Correct!";
        }

        yield return new WaitForSeconds(0.2f);

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
        if (inputDisplayText != null)
        {
            inputDisplayText.color = Color.red;
            inputDisplayText.text = "Input: Fail!";
        }

        yield return new WaitForSeconds(1.5f);


        if (generateRandomCode)
        {
            GenerateRandomCode();
        }
        currentInput = "";


        CreatePuzzleUI();
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



        if (generateRandomCode)
        {
            GenerateRandomCode();
        }

        isPuzzleActive = true;
        currentInput = "";


        CreatePuzzleUI();

        SetPlayerControl(false);
        UpdateInputDisplay();

        if (codeDisplayText != null)
        {
            codeDisplayText.text = "Code: " + targetCode;
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


        if (mainCanvas != null)
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

    public void SetTargetCode(string newCode)
    {
        targetCode = newCode;
        if (codeDisplayText != null)
        {
            codeDisplayText.text = "Code: " + targetCode;
        }
    }

    public void SetCodeLength(int length)
    {
        codeLength = length;
        if (generateRandomCode)
        {
            GenerateRandomCode();
        }
    }

    public void SetRandomCodeGeneration(bool enabled)
    {
        generateRandomCode = enabled;
    }
}