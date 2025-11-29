using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [Header("Animation Settings")]
    public float transitionDuration = 0.15f;
    public bool allowAnimationCompletion = true;

    [Header("Debug Controls")]
    public Vector2 debugMovement = Vector2.zero;
    public bool useDebugControls = false;

    private Animator animator;
    private Vector2 lastDirection = Vector2.down;
    private string currentState = "";

    // Названия состояний анимаций
    private readonly string[] walkStates = { "walk down", "walk up", "walk left", "walk right" };
    private readonly string[] idleStates = { "down", "up", "left", "right" };

    void Start()
    {
        animator = GetComponent<Animator>();
        SetupDefaultParameters();

        // Устанавливаем нормальную скорость анимаций
        if (animator != null)
        {
            animator.speed = 1f;
        }
    }

    void Update()
    {
        if (useDebugControls)
        {
            UpdateAnimations(debugMovement);
        }
    }

    void SetupDefaultParameters()
    {
        if (animator == null) return;

        animator.SetFloat("LastMoveX", 0);
        animator.SetFloat("LastMoveY", -1);
        animator.SetBool("IsMoving", false);
    }

    public void UpdateAnimations(Vector2 movementInput)
    {
        if (animator == null) return;

        Vector2 movement = movementInput.normalized;
        bool isMoving = movement.magnitude > 0.1f;

        // Запоминаем последнее направление при движении
        if (isMoving)
        {
            lastDirection = movement;
        }

        // Плавно изменяем параметры вместо резких скачков
        SmoothSetFloat("MoveX", movement.x);
        SmoothSetFloat("MoveY", movement.y);
        SmoothSetFloat("LastMoveX", lastDirection.x);
        SmoothSetFloat("LastMoveY", lastDirection.y);

        animator.SetBool("IsMoving", isMoving);
    }

    // Плавное изменение float параметров
    private void SmoothSetFloat(string paramName, float targetValue)
    {
        float currentValue = animator.GetFloat(paramName);
        float newValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * 10f);
        animator.SetFloat(paramName, newValue);
    }

    // Метод для принудительного завершения текущей анимации
    public void CompleteCurrentAnimation()
    {
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            animator.Play(stateInfo.fullPathHash, 0, 1f); // Перематываем на конец анимации
        }
    }

    // Для отладки
    [ContextMenu("Test Walk Up")]
    void TestWalkUp()
    {
        UpdateAnimations(Vector2.up);
        Invoke("StopMoving", 1f); // Останавливаем через 1 секунду
    }

    [ContextMenu("Test Walk Down")]
    void TestWalkDown()
    {
        UpdateAnimations(Vector2.down);
        Invoke("StopMoving", 1f);
    }

    [ContextMenu("Test Walk Left")]
    void TestWalkLeft()
    {
        UpdateAnimations(Vector2.left);
        Invoke("StopMoving", 1f);
    }

    [ContextMenu("Test Walk Right")]
    void TestWalkRight()
    {
        UpdateAnimations(Vector2.right);
        Invoke("StopMoving", 1f);
    }

    [ContextMenu("Stop Moving")]
    void StopMoving() => UpdateAnimations(Vector2.zero);
}