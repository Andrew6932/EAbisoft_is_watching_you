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
    public string interactionText = "Нажми E чтобы взаимодействовать";
    public Vector3 textOffset = new Vector3(0, 2f, 0);

    [Header("Cooldown Settings")]
    public float interactionCooldown = 10f;
    public bool showCooldownText = true;
    public bool showCooldownVisual = true;
    public Color cooldownColor = Color.gray;

    [Header("Task Manager Settings")]
    public float managerTaskDuration = 10f; // Время на выполнение задачи менеджера

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

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        CreatePromptUI();

        // Особые настройки для таск менеджера
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

        // Добавляем задачи в таскбар для всех объектов КРОМЕ таск менеджера
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
            // Таск менеджер НЕ добавляется изначально - он появится после кулдауна
            default:
                break;
        }
    }

    // Проверяем, является ли этот объект таск менеджером
    private bool IsTaskManager()
    {
        return gameObject.name == "Modern_Office_MV_2_TILESETS_B-C-D-E_36";
    }

    // Настраиваем таск менеджер с особыми параметрами
    private void ConfigureTaskManager()
    {
        // Устанавливаем красный цвет подсветки
        highlightColor = Color.red;

        // Устанавливаем случайный кулдаун от 15 до 40 секунд
        interactionCooldown = Random.Range(15f, 40f);

        // Включаем кулдаун сразу при старте
        StartCooldown();

        Debug.Log($"Таск менеджер настроен: кулдаун {interactionCooldown:F1} секунд, цвет: красный");
    }

    void Update()
    {
        // Пульсация подсветки только если объект включен и не на кулдауне
        if (isHighlighted && spriteRenderer != null && highlightEnabled && !isOnCooldown)
        {
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            Color pulseColor = highlightColor;
            pulseColor.a = alpha;
            spriteRenderer.color = pulseColor;
        }

        // Обработка кулдауна
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;

            // Визуальная индикация кулдауна
            if (showCooldownVisual && spriteRenderer != null)
            {
                float cooldownProgress = cooldownTimer / interactionCooldown;
                Color currentColor = Color.Lerp(highlightColor, cooldownColor, cooldownProgress);
                currentColor.a = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * pulseSpeed * 0.5f) + 1f) / 2f);
                spriteRenderer.color = currentColor;
            }

            // Обновляем текст подсказки если включено
            if (showCooldownText && promptUI != null && playerInRange)
            {
                UpdateCooldownText();
            }

            if (cooldownTimer <= 0f)
            {
                EndCooldown();
            }
        }

        // Взаимодействие только если объект включен, игрок в зоне и нет кулдауна
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && highlightEnabled && !isOnCooldown)
        {
            OnInteract();
        }
    }

    void CreatePromptUI()
    {
        // Удаляем старый UI если есть
        if (promptUI != null)
        {
            Destroy(promptUI.gameObject);
        }

        GameObject promptObj = new GameObject("InteractionPrompt");
        promptObj.transform.SetParent(transform);
        promptObj.transform.localPosition = textOffset;

        promptUI = promptObj.AddComponent<InteractionPromptUI>();

        // Для таск менеджера обновляем текст с учетом красной подсветки
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

        // Восстанавливаем нормальную подсветку если не на кулдауне
        if (!isOnCooldown && spriteRenderer != null)
        {
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            Color pulseColor = highlightColor;
            pulseColor.a = alpha;
            spriteRenderer.color = pulseColor;
        }

        // Показываем подсказку если игрок в зоне
        if (playerInRange && promptUI != null)
        {
            if (isOnCooldown && showCooldownText)
            {
                UpdateCooldownText();
            }
            else
            {
                // Для таск менеджера используем специальный текст
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
                    // Для таск менеджера используем специальный текст
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

        Debug.Log("Взаимодействие с: " + gameObject.name);

        // Если это менеджер и активна задача с обратным отсчетом, останавливаем ее
        if (IsTaskManager() && isManagerTaskActive)
        {
            StopManagerTask();

            // Убираем задачу из таскбара при начале взаимодействия
            taskBarMenu.RemoveTaskBar("Consult with Manager");
        }

        // Запускаем соответствующий пазл
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

    // ЗАПУСК задачи менеджера с обратным отсчетом
    private void StartManagerTask()
    {
        if (managerTaskCoroutine != null)
        {
            StopCoroutine(managerTaskCoroutine);
        }
        isManagerTaskActive = true;
        managerTaskCoroutine = StartCoroutine(ManagerTaskCountdown());
    }

    // Остановка задачи менеджера (при успешном взаимодействии)
    private void StopManagerTask()
    {
        if (managerTaskCoroutine != null)
        {
            StopCoroutine(managerTaskCoroutine);
            managerTaskCoroutine = null;
        }
        isManagerTaskActive = false;

        // Останавливаем обратный отсчет в таскбаре
        TaskBarItem currentManagerTask = FindCurrentManagerTask();
        if (currentManagerTask != null)
        {
            currentManagerTask.StopCountdown();
        }
    }

    // Находим текущую задачу менеджера в таскбаре
    private TaskBarItem FindCurrentManagerTask()
    {
        // Вариант 1: если TaskBarMenu хранит список items
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

        // Вариант 2: найти через поиск по сцене
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

    // Обратный отсчет для задачи менеджера
    private IEnumerator ManagerTaskCountdown()
    {
        float timeLeft = managerTaskDuration;

        while (timeLeft > 0f && isManagerTaskActive)
        {
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        // Если время вышло и задача все еще активна (не было взаимодействия)
        if (isManagerTaskActive && timeLeft <= 0f)
        {
            Debug.Log("Время на консультацию с менеджером вышло! Объект уходит на кулдаун.");

            // Уведомляем GameManager о пропущенном звонке
            if (gameManager != null)
            {
                gameManager.OnManagerCallMissed();
            }

            // Убираем задачу из таскбара
            taskBarMenu.RemoveTaskBar("Consult with Manager");

            // Запускаем кулдаун
            StartCooldown();

            isManagerTaskActive = false;
        }
    }

    void StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimer = interactionCooldown;

        // Останавливаем задачу менеджера если она активна
        if (isManagerTaskActive)
        {
            StopManagerTask();
        }

        // Скрываем подсказку взаимодействия
        if (promptUI != null)
        {
            promptUI.Hide();
        }

        // Убираем задачу из таскбара при начале кулдауна
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

        Debug.Log("Начат кулдаун для: " + gameObject.name);
    }

    void EndCooldown()
    {
        isOnCooldown = false;
        cooldownTimer = 0f;

        // Восстанавливаем нормальную подсветку
        if (isHighlighted && spriteRenderer != null)
        {
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            Color pulseColor = highlightColor;
            pulseColor.a = alpha;
            spriteRenderer.color = pulseColor;
        }

        // Для таск менеджера запускаем задачу с обратным отсчетом
        if (IsTaskManager())
        {
            // Добавляем задачу в таскбар
            taskBarMenu.AddNewTaskBar("Consult with Manager");

            // Запускаем обратный отсчет для задачи
            StartManagerTask(); // ВОТ ТЕПЕРЬ ЭТОТ МЕТОД СУЩЕСТВУЕТ
        }
        else
        {
            // Для других объектов просто добавляем задачу
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

        // Показываем подсказку снова если игрок в зоне
        if (playerInRange && promptUI != null)
        {
            // Для таск менеджера используем специальный текст
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

        Debug.Log("Кулдаун завершен для: " + gameObject.name);
    }

    void UpdateCooldownText()
    {
        if (promptUI != null && isOnCooldown && playerInRange)
        {
            int secondsLeft = Mathf.CeilToInt(cooldownTimer);
            promptUI.UpdateText($"Перезарядка: {secondsLeft}с");
            promptUI.Show();
        }
    }

    // Метод для принудительного обновления настроек таск менеджера
    public void RefreshTaskManagerSettings()
    {
        if (IsTaskManager())
        {
            ConfigureTaskManager();
        }
    }
    // Методы для управления из инспектора и других скриптов

    /// <summary>
    /// Включить объект (подсветку и взаимодействие)
    /// </summary>
    public void EnableObject()
    {
        highlightEnabled = true;
        if (startHighlighted)
        {
            StartHighlight();
        }
    }

    /// <summary>
    /// Выключить объект (подсветку и взаимодействие)
    /// </summary>
    public void DisableObject()
    {
        highlightEnabled = false;
        StopHighlight();
    }

    /// <summary>
    /// Переключить состояние объекта
    /// </summary>
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

    /// <summary>
    /// Временно отключить объект на указанное время
    /// </summary>
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

    /// <summary>
    /// Сбросить кулдаун и восстановить подсветку
    /// </summary>
    public void ResetCooldown()
    {
        EndCooldown();
    }

    /// <summary>
    /// Установить новый кулдаун
    /// </summary>
    public void SetCooldown(float newCooldown)
    {
        interactionCooldown = newCooldown;
        if (isOnCooldown)
        {
            cooldownTimer = Mathf.Min(cooldownTimer, newCooldown);
        }
    }

    /// <summary>
    /// Принудительно начать кулдаун
    /// </summary>
    public void ForceCooldown(float customCooldown = -1f)
    {
        if (customCooldown > 0f)
        {
            interactionCooldown = customCooldown;
        }
        StartCooldown();
    }

    /// <summary>
    /// Пересоздать UI (полезно после кулдауна)
    /// </summary>
    public void RefreshUI()
    {
        CreatePromptUI();
        if (isHighlighted && playerInRange && !isOnCooldown)
        {
            // Для таск менеджера используем специальный текст
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

    // Свойства для проверки состояния
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

    /// <summary>
    /// Получить информацию о состоянии для отладки
    /// </summary>
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

    // Вызывается когда пазл завершается (должен вызываться из пазла)
    public void OnPuzzleCompleted()
    {
        StartCooldown();
    }

    // Вызывается когда пазл закрывается без завершения
    public void OnPuzzleClosed()
    {
        // Ничего не делаем, подсветка восстановится после кулдауна
    }
}