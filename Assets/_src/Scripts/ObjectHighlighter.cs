using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectHighlighter : MonoBehaviour
{
    [Header("Highlight Settings")]
    public bool highlightEnabled = true;
    public Color highlightColor = Color.yellow;
    public float pulseSpeed = 2f;
    public float minAlpha = 0.3f;
    public float maxAlpha = 1f;
    public bool startHighlighted = false;

    public GameManager gameManager;

    [Header("Interaction Prompt")]
    public string interactionText = "Click E";
    public Vector3 textOffset = new Vector3(0, 2f, 0);

    [Header("Cooldown Settings")]
    public float interactionCooldown = 10f;
    public bool showCooldownText = true;
    public bool showCooldownVisual = true;
    public Color cooldownColor = Color.gray;

    [Header("Task Manager Settings")]
    public float managerTaskDuration = 10f;

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
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    private Coroutine managerTaskCoroutine;
    private bool isManagerTaskActive = false;
    public TaskBarMenu taskBarMenu;

    [Header("Audio")]
    public AudioClip interactAudioClip;
    public AudioClip puzzleCompleteAudioClip;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        CreatePromptUI();

        if (IsTaskManager())
        {
            ConfigureTaskManager();
        }

        if (highlightEnabled && startHighlighted)
        {
            StartHighlight();
        }
        else
        {
            StopHighlight();
        }

        switch (gameObject.name)
        {
            case "Modern_Office_MV_2_TILESETS_B-C-D-E_48":
                taskBarMenu.AddNewTaskBar("Program Combat AI");
                break;
            case "Modern_Office_MV_2_TILESETS_B-C-D-E_38":
                taskBarMenu.AddNewTaskBar("Edit Graphics");
                break;
            case "Modern_Office_MV_2_TILESETS_B-C-D-E_45":
                taskBarMenu.AddNewTaskBar("Program Game");
                break;
            default:
                break;
        }
    }

    private bool IsTaskManager()
    {
        return gameObject.name == "Modern_Office_MV_2_TILESETS_B-C-D-E_36";
    }

    private void ConfigureTaskManager()
    {
        highlightColor = Color.red;
        interactionCooldown = Random.Range(15f, 40f);
        StartCooldown();
    }

    void Update()
    {
        if (isHighlighted && spriteRenderer != null && highlightEnabled && !isOnCooldown)
        {
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            Color pulseColor = highlightColor;
            pulseColor.a = alpha;
            spriteRenderer.color = pulseColor;
        }

        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;

            if (showCooldownVisual && spriteRenderer != null)
            {
                float cooldownProgress = cooldownTimer / interactionCooldown;
                Color currentColor = Color.Lerp(highlightColor, cooldownColor, cooldownProgress);
                currentColor.a = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * pulseSpeed * 0.5f) + 1f) / 2f);
                spriteRenderer.color = currentColor;
            }

            if (showCooldownText && promptUI != null && playerInRange)
            {
                UpdateCooldownText();
            }

            if (cooldownTimer <= 0f)
            {
                EndCooldown();
            }
        }

        if (playerInRange && Input.GetKeyDown(KeyCode.E) && highlightEnabled && !isOnCooldown)
        {
            OnInteract();
        }
    }

    void CreatePromptUI()
    {
        if (promptUI != null)
        {
            Destroy(promptUI.gameObject);
        }

        GameObject promptObj = new GameObject("InteractionPrompt");
        promptObj.transform.SetParent(transform);
        promptObj.transform.localPosition = textOffset;

        promptUI = promptObj.AddComponent<InteractionPromptUI>();

        if (IsTaskManager())
        {
            promptUI.Setup("Consult with Manager (Red Priority)");
        }
        else
        {
            promptUI.Setup(interactionText);
        }

        promptUI.Hide();
    }

    public void StartHighlight()
    {
        if (!highlightEnabled) return;

        isHighlighted = true;

        if (!isOnCooldown && spriteRenderer != null)
        {
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            Color pulseColor = highlightColor;
            pulseColor.a = alpha;
            spriteRenderer.color = pulseColor;
        }

        if (playerInRange && promptUI != null)
        {
            if (isOnCooldown && showCooldownText)
            {
                UpdateCooldownText();
            }
            else
            {
                if (IsTaskManager())
                {
                    promptUI.UpdateText("Consult with Manager (Red Priority)");
                }
                else
                {
                    promptUI.UpdateText(interactionText);
                }
            }
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
        if (other.CompareTag("Player") && highlightEnabled)
        {
            playerInRange = true;
            if (isHighlighted && promptUI != null)
            {
                if (isOnCooldown && showCooldownText)
                {
                    UpdateCooldownText();
                }
                else
                {
                    if (IsTaskManager())
                    {
                        promptUI.UpdateText("Consult with Manager (Red Priority)");
                    }
                }
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
        if (!highlightEnabled || isOnCooldown) return;

        if (interactAudioClip != null)
        {
            AudioManager.Instance.PlaySound(interactAudioClip);
        }
        else
        {
            AudioManager.Instance.PlayInteractSound();
        }

        if (IsTaskManager())
        {
            AudioManager.Instance.StopPhoneRing();

            if (isManagerTaskActive)
            {
                StopManagerTask();
                taskBarMenu.RemoveTaskBar("Consult with Manager");
            }
        }

        if (codePuzzle != null)
        {
            codePuzzle.StartPuzzle();
            StartCooldown();
        }
        else if (waitPuzzle != null)
        {
            waitPuzzle.StartPuzzle();
            StartCooldown();
        }
        else if (mathPuzzle != null)
        {
            mathPuzzle.GenerateMathProblem();
            mathPuzzle.StartPuzzle();
            StartCooldown();
        }
        else if (colorSequencePuzzle != null)
        {
            colorSequencePuzzle.StartPuzzle();
            StartCooldown();
        }
    }

    private void StartManagerTask()
    {
        if (managerTaskCoroutine != null)
        {
            StopCoroutine(managerTaskCoroutine);
        }
        isManagerTaskActive = true;
        managerTaskCoroutine = StartCoroutine(ManagerTaskCountdown());
    }

    private void StopManagerTask()
    {
        if (managerTaskCoroutine != null)
        {
            StopCoroutine(managerTaskCoroutine);
            managerTaskCoroutine = null;
        }
        isManagerTaskActive = false;

        TaskBarItem currentManagerTask = FindCurrentManagerTask();
        if (currentManagerTask != null)
        {
            currentManagerTask.StopCountdown();
        }
    }

    private TaskBarItem FindCurrentManagerTask()
    {
        if (taskBarMenu != null)
        {
            foreach (var item in taskBarMenu.GetItems())
            {
                if (item.GetLabel().StartsWith("Consult with Manager"))
                {
                    return item;
                }
            }
        }

        TaskBarItem[] allItems = FindObjectsOfType<TaskBarItem>();
        foreach (var item in allItems)
        {
            if (item.GetLabel().StartsWith("Consult with Manager"))
            {
                return item;
            }
        }

        return null;
    }

    private IEnumerator ManagerTaskCountdown()
    {
        float timeLeft = managerTaskDuration;

        while (timeLeft > 0f && isManagerTaskActive)
        {
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        if (isManagerTaskActive && timeLeft <= 0f)
        {
            if (gameManager != null)
            {
                gameManager.OnManagerCallMissed();
            }

            taskBarMenu.RemoveTaskBar("Consult with Manager");
            StartCooldown();
            isManagerTaskActive = false;
        }
    }

    void StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimer = interactionCooldown;

        if (isManagerTaskActive)
        {
            StopManagerTask();
        }

        if (promptUI != null)
        {
            promptUI.Hide();
        }

        switch (gameObject.name)
        {
            case "Modern_Office_MV_2_TILESETS_B-C-D-E_48":
                taskBarMenu.RemoveTaskBar("Program Combat AI");
                break;
            case "Modern_Office_MV_2_TILESETS_B-C-D-E_38":
                taskBarMenu.RemoveTaskBar("Edit Graphics");
                break;
            case "Modern_Office_MV_2_TILESETS_B-C-D-E_45":
                taskBarMenu.RemoveTaskBar("Program Game");
                break;
            case "Modern_Office_MV_2_TILESETS_B-C-D-E_36":
                taskBarMenu.RemoveTaskBar("Consult with Manager");
                break;
            default:
                break;
        }
    }

    void EndCooldown()
    {
        isOnCooldown = false;
        cooldownTimer = 0f;

        if (isHighlighted && spriteRenderer != null)
        {
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            Color pulseColor = highlightColor;
            pulseColor.a = alpha;
            spriteRenderer.color = pulseColor;
        }

        if (IsTaskManager())
        {
            taskBarMenu.AddNewTaskBar("Consult with Manager");
            StartManagerTask();
            AudioManager.Instance.TriggerPhoneCall();
        }
        else
        {
            switch (gameObject.name)
            {
                case "Modern_Office_MV_2_TILESETS_B-C-D-E_48":
                    taskBarMenu.AddNewTaskBar("Program Combat AI");
                    break;
                case "Modern_Office_MV_2_TILESETS_B-C-D-E_38":
                    taskBarMenu.AddNewTaskBar("Edit Graphics");
                    break;
                case "Modern_Office_MV_2_TILESETS_B-C-D-E_45":
                    taskBarMenu.AddNewTaskBar("Program Game");
                    break;
                default:
                    break;
            }
        }

        if (playerInRange && promptUI != null)
        {
            if (IsTaskManager())
            {
                promptUI.UpdateText("Consult with Manager (Red Priority)");
            }
            else
            {
                promptUI.UpdateText(interactionText);
            }
            promptUI.Show();
        }
    }

    void UpdateCooldownText()
    {
        if (promptUI != null && isOnCooldown && playerInRange)
        {
            int secondsLeft = Mathf.CeilToInt(cooldownTimer);
            promptUI.UpdateText($"cooldown: {secondsLeft}s");
            promptUI.Show();
        }
    }

    public void RefreshTaskManagerSettings()
    {
        if (IsTaskManager())
        {
            ConfigureTaskManager();
        }
    }

    public void EnableObject()
    {
        highlightEnabled = true;
        if (startHighlighted)
        {
            StartHighlight();
        }
    }

    public void DisableObject()
    {
        highlightEnabled = false;
        StopHighlight();
    }

    public void ToggleObject()
    {
        highlightEnabled = !highlightEnabled;
        if (highlightEnabled && startHighlighted)
        {
            StartHighlight();
        }
        else
        {
            StopHighlight();
        }
    }

    public void DisableTemporarily(float seconds)
    {
        StartCoroutine(DisableForSeconds(seconds));
    }

    private IEnumerator DisableForSeconds(float seconds)
    {
        bool wasEnabled = highlightEnabled;
        DisableObject();

        yield return new WaitForSeconds(seconds);

        if (wasEnabled)
        {
            EnableObject();
        }
    }

    public void ResetCooldown()
    {
        EndCooldown();
    }

    public void SetCooldown(float newCooldown)
    {
        interactionCooldown = newCooldown;
        if (isOnCooldown)
        {
            cooldownTimer = Mathf.Min(cooldownTimer, newCooldown);
        }
    }

    public void ForceCooldown(float customCooldown = -1f)
    {
        if (customCooldown > 0f)
        {
            interactionCooldown = customCooldown;
        }
        StartCooldown();
    }

    public void RefreshUI()
    {
        CreatePromptUI();
        if (isHighlighted && playerInRange && !isOnCooldown)
        {
            if (IsTaskManager())
            {
                promptUI.UpdateText("Consult with Manager (Red Priority)");
            }
            else
            {
                promptUI.UpdateText(interactionText);
            }
            promptUI.Show();
        }
    }

    public void SetInteractionText(string text)
    {
        interactionText = text;
        if (promptUI != null && !isOnCooldown)
        {
            promptUI.UpdateText(text);
        }
    }

    public bool IsPlayerInRange()
    {
        return playerInRange;
    }

    public bool IsEnabled
    {
        get { return highlightEnabled; }
    }

    public bool IsOnCooldown
    {
        get { return isOnCooldown; }
    }

    public float CooldownProgress
    {
        get { return isOnCooldown ? 1f - (cooldownTimer / interactionCooldown) : 1f; }
    }

    public float CooldownTimeLeft
    {
        get { return isOnCooldown ? cooldownTimer : 0f; }
    }

    public string GetStatusInfo()
    {
        return $"Object: {gameObject.name}\n" +
               $"Enabled: {highlightEnabled}\n" +
               $"Highlighted: {isHighlighted}\n" +
               $"Player in range: {playerInRange}\n" +
               $"On cooldown: {isOnCooldown}\n" +
               $"Cooldown time left: {cooldownTimer:F1}s\n" +
               $"Cooldown progress: {CooldownProgress:P0}";
    }

    public void OnPuzzleCompleted()
    {
        if (puzzleCompleteAudioClip != null)
        {
            AudioManager.Instance.PlaySound(puzzleCompleteAudioClip);
        }
        else
        {
            AudioManager.Instance.PlayPuzzleCompleteSound();
        }
        StartCooldown();
    }

    public void OnPuzzleClosed()
    {
    }
}