using UnityEngine;
using System.Collections;

public class ObjectHighlighter : MonoBehaviour
{
    [Header("Highlight Settings")]
    public Color highlightColor = Color.yellow;
    public float pulseSpeed = 2f;
    public float minAlpha = 0.3f;
    public float maxAlpha = 1f;
    public bool startHighlighted = false;

    [Header("Interaction Prompt")]
    public string interactionText = "Нажми E чтобы взаимодействовать";
    public Vector3 textOffset = new Vector3(0, 2f, 0);

    [Header("Puzzle Settings")]
    public SimpleCodePuzzle codePuzzle;
    public WaitTimePuzzle waitPuzzle;
    public MathPuzzle mathPuzzle;
    public ColorSequencePuzzle colorSequencePuzzle;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isHighlighted = false;
    private bool playerInRange = false;
    private InteractionPromptUI promptUI;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        CreatePromptUI();

        if (startHighlighted)
        {
            StartHighlight();
        }
        else
        {
            StopHighlight();
        }
    }

    void Update()
    {
        // Пульсация подсветки
        if (isHighlighted && spriteRenderer != null)
        {
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            Color pulseColor = highlightColor;
            pulseColor.a = alpha;
            spriteRenderer.color = pulseColor;
        }

        // Проверка взаимодействия
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            OnInteract();
        }

    }

    void CreatePromptUI()
    {
        // Создаем простой UI для подсказки
        GameObject promptObj = new GameObject("InteractionPrompt");
        promptObj.transform.SetParent(transform);
        promptObj.transform.localPosition = textOffset;

        promptUI = promptObj.AddComponent<InteractionPromptUI>();
        promptUI.Setup(interactionText);
        promptUI.Hide();
    }

    public void StartHighlight()
    {
        isHighlighted = true;

        // Показываем подсказку если игрок в зоне
        if (playerInRange && promptUI != null)
        {
            promptUI.Show();
        }
    }

    public void StopHighlight()
    {
        isHighlighted = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        if (promptUI != null)
        {
            promptUI.Hide();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (isHighlighted && promptUI != null)
            {
                promptUI.Show();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptUI != null)
            {
                promptUI.Hide();
            }
        }
    }

    void OnInteract()
    {
        Debug.Log("Взаимодействие с: " + gameObject.name);

        // Запускаем код-пазл если он есть
        if (codePuzzle != null)
        {
            codePuzzle.StartPuzzle();
        }
        // Или запускаем пазл ожидания если он есть
        else if (waitPuzzle != null)
        {
            waitPuzzle.StartPuzzle();
        }
        else if (mathPuzzle != null) // Добавляем проверку для математического пазла
        {
            mathPuzzle.StartPuzzle();
        }
        else if (colorSequencePuzzle != null) // Добавляем проверку для пазла с последовательностью
        {
            colorSequencePuzzle.StartPuzzle();
        }
    }

    public void SetInteractionText(string text)
    {
        interactionText = text;
        if (promptUI != null)
        {
            promptUI.UpdateText(text);
        }
    }

    public bool IsPlayerInRange()
    {
        return playerInRange;
    }
}