using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ColorSequencePuzzle : MonoBehaviour
{
    [Header("Sequence Settings")]
    public int sequenceLength = 4;
    public float showTime = 1f; 
    public float delayBetweenColors = 0.5f; 

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

        if (puzzleUI != null)
        {
            Destroy(puzzleUI);
            puzzleUI = null;
        }


        mainCanvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("Canvas не найден на сцене!");
            return;
        }

        mainCanvas.sortingOrder = 10;


        puzzleUI = CreateUIElement("ColorSequenceContainer", mainCanvas.transform);
        RectTransform containerRect = puzzleUI.GetComponent<RectTransform>();

        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(500, 300); 
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.localScale = Vector3.one;


        GameObject background = CreateUIElement("Background", puzzleUI.transform);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.85f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        bgRect.localScale = Vector3.one;

 
        GameObject messageDisplay = CreateUIElement("Message", puzzleUI.transform);
        messageText = messageDisplay.AddComponent<TextMeshProUGUI>();

        if (textFont != null)
            messageText.font = textFont;

        messageText.fontSize = fontSize; 
        messageText.color = textColor;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.fontStyle = FontStyles.Bold;
        messageText.text = "Remember  Sequence!";
        messageText.enableAutoSizing = false;
        messageText.overflowMode = TextOverflowModes.Overflow;

        RectTransform messageRect = messageDisplay.GetComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0f, 0.75f);   
        messageRect.anchorMax = new Vector2(1f, 0.95f);   
        messageRect.offsetMin = new Vector2(10f, 0f);     
        messageRect.offsetMax = new Vector2(-10f, 0f);    
        messageRect.localScale = Vector3.one;


        GameObject buttonsContainer = CreateUIElement("ButtonsContainer", puzzleUI.transform);
        RectTransform buttonsRect = buttonsContainer.GetComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0f, 0.3f);    
        buttonsRect.anchorMax = new Vector2(1f, 0.75f);  
        buttonsRect.offsetMin = new Vector2(20f, 10f);    
        buttonsRect.offsetMax = new Vector2(-20f, -10f);
        buttonsRect.localScale = Vector3.one;

        // Создаем кнопки цветов
        colorButtons = new GameObject[availableColors.Length];
        colorButtonImages = new Image[availableColors.Length];

        float buttonSize = 70f; 
        float spacing = 15f;    
        float totalWidth = (availableColors.Length * buttonSize) + ((availableColors.Length - 1) * spacing);
        float startX = -totalWidth / 2 + buttonSize / 2;

        for (int i = 0; i < availableColors.Length; i++)
        {

            GameObject buttonObj = CreateUIElement("ColorButton_" + i, buttonsContainer.transform);
            buttonObj.AddComponent<RectTransform>();
            buttonObj.AddComponent<CanvasRenderer>();

            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = availableColors[i];
            buttonImage.type = Image.Type.Sliced;


            Button button = buttonObj.AddComponent<Button>();


            ColorBlock colors = button.colors;
            colors.normalColor = availableColors[i];
            colors.highlightedColor = Color.Lerp(availableColors[i], Color.white, 0.3f);
            colors.pressedColor = Color.Lerp(availableColors[i], Color.black, 0.3f);
            colors.selectedColor = availableColors[i];
            colors.disabledColor = Color.Lerp(availableColors[i], Color.gray, 0.7f);
            button.colors = colors;

            int colorIndex = i; 
            button.onClick.AddListener(() => OnColorButtonClick(colorIndex));

            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);  
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f); 
            buttonRect.pivot = new Vector2(0.5f, 0.5f);      
            buttonRect.sizeDelta = new Vector2(buttonSize, buttonSize);

            buttonRect.anchoredPosition = new Vector2(startX + i * (buttonSize + spacing), 0);
            buttonRect.localScale = Vector3.one;

            colorButtons[i] = buttonObj;
            colorButtonImages[i] = buttonImage;
        }


        GameObject hintDisplay = CreateUIElement("Hint", puzzleUI.transform);
        TextMeshProUGUI hintText = hintDisplay.AddComponent<TextMeshProUGUI>();
        hintText.text = "Repeat Sequence  (ESC - exit)";

        if (textFont != null)
            hintText.font = textFont;

        hintText.fontSize = fontSize - 4; 
        hintText.color = Color.gray;
        hintText.alignment = TextAlignmentOptions.Center;
        hintText.enableAutoSizing = false;

        RectTransform hintRect = hintDisplay.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0f, 0f);     
        hintRect.anchorMax = new Vector2(1f, 0.3f);   
        hintRect.offsetMin = new Vector2(10f, 5f);    
        hintRect.offsetMax = new Vector2(-10f, -5f);
        hintRect.localScale = Vector3.one;

        puzzleUI.SetActive(true); 
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


        CheckSequence();
    }

    IEnumerator HighlightButton(int colorIndex)
    {
        Color originalColor = colorButtonImages[colorIndex].color;
        colorButtonImages[colorIndex].color = Color.white; 
        yield return new WaitForSeconds(0.3f);
        colorButtonImages[colorIndex].color = originalColor;
    }

    void CheckSequence()
    {

        for (int i = 0; i < playerSequence.Count; i++)
        {
            if (playerSequence[i] != correctSequence[i])
            {

                StartCoroutine(PuzzleFail());
                return;
            }
        }


        if (playerSequence.Count == correctSequence.Count)
        {
            StartCoroutine(PuzzleComplete());
        }
        else
        {

            if (messageText != null)
            {
                messageText.text = $"Correct! : {correctSequence.Count - playerSequence.Count}";
                messageText.color = Color.green;
            }
        }
    }

    IEnumerator ShowSequence()
    {
        isShowingSequence = true;

        if (messageText != null)
        {
            messageText.text = "Remember Sequence .";
            messageText.color = Color.yellow;
        }

        SetButtonsInteractable(false);

        yield return new WaitForSeconds(1f);


        for (int i = 0; i < correctSequence.Count; i++)
        {
            int colorIndex = correctSequence[i];
            Color originalColor = colorButtonImages[colorIndex].color;


            colorButtonImages[colorIndex].color = Color.white;
            yield return new WaitForSeconds(showTime);


            colorButtonImages[colorIndex].color = originalColor;
            yield return new WaitForSeconds(delayBetweenColors);
        }


        SetButtonsInteractable(true);
        isShowingSequence = false;

        if (messageText != null)
        {
            messageText.text = "Repeat Sequence!";
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
            messageText.text = " Correct!";
            messageText.color = Color.green;
        }

        yield return StartCoroutine(ShowSuccessAnimation());

        yield return new WaitForSeconds(0.5f);

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
            messageText.text = "Fail! try again";
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


        playerSequence.Clear();
        GenerateSequence(); 
        StartCoroutine(ShowSequence());

        if (onPuzzleFail != null)
        {
            onPuzzleFail.Invoke();
        }
    }

    public void StartPuzzle()
    {

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
        if (puzzleUI != null)
        {
            Destroy(puzzleUI);
        }
    }


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