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

    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    private Material highlightMaterial;
    private Color originalColor;
    private bool isHighlighted = false;
    private bool playerInRange = false;

    // Ссылка на UI для показа текста
    private InteractionPromptUI promptUI;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
            originalColor = spriteRenderer.color;

            // Создаем материал для подсветки
            highlightMaterial = new Material(Shader.Find("Sprites/Default"));
            highlightMaterial.color = highlightColor;
        }

        // Создаем или находим UI для подсказок
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
        if (isHighlighted)
        {
            // Пульсация альфа-канала
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            Color pulseColor = highlightColor;
            pulseColor.a = alpha;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = pulseColor;
            }
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

        if (spriteRenderer != null)
        {
            spriteRenderer.material = highlightMaterial;
        }

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
            spriteRenderer.material = originalMaterial;
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

        // Вызываем событие взаимодействия
        // Можно добавить UnityEvent здесь

        // Останавливаем подсветку после взаимодействия (опционально)
        StopHighlight();
    }

    // Методы для внешнего управления
    public void SetHighlightColor(Color color)
    {
        highlightColor = color;
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

    void OnDestroy()
    {
        // Чистим материалы
        if (highlightMaterial != null)
        {
            DestroyImmediate(highlightMaterial);
        }
    }
}