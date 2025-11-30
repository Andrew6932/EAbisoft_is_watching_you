using UnityEngine;
using System.Collections;

public class CatController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 4f;
    public float minMoveTime = 2f;
    public float maxMoveTime = 5f;

    [Header("Resting Settings")]
    public float minSitTime = 3f; // Minimum sitting time
    public float maxSitTime = 8f; // Maximum sitting time
    public float minLieTime = 5f; // Minimum lying time
    public float maxLieTime = 12f; // Maximum lying time
    public float restChance = 0.4f; // 40% chance to rest after moving
    public float lieChance = 0.3f; // 30% chance to lie down instead of sitting (from rest chances)
    public Sprite sittingSprite; // Sprite to show when cat is sitting
    public Sprite lyingSprite; // Sprite to show when cat is lying down

    [Header("Roaming Area")]
    public Vector2 roamAreaCenter = Vector2.zero;
    public Vector2 roamAreaSize = new Vector2(10f, 10f);

    [Header("Animation")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    private Vector2 targetPosition;
    private bool isMoving = false;
    private bool isResting = false;
    private CatRestState restState = CatRestState.None;
    private Rigidbody2D rb;
    private Sprite originalSprite; // Store original sprite for when cat stands up

    // Enum for cat resting states
    public enum CatRestState
    {
        None,
        Sitting,
        Lying
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // If animator is not assigned, try to find it
        if (animator == null)
            animator = GetComponent<Animator>();

        // If SpriteRenderer is not assigned, try to find it
        if (spriteRenderer != null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Store the original sprite
        if (spriteRenderer != null)
            originalSprite = spriteRenderer.sprite;

        // Start wandering behavior
        StartCoroutine(WanderRoutine());
    }

    void Update()
    {
        // Update animation
        UpdateAnimation();
    }

    IEnumerator WanderRoutine()
    {
        while (true)
        {
            // Random waiting time
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            // Choose new target
            SetNewRandomTarget();

            // Move to target
            float moveTime = Random.Range(minMoveTime, maxMoveTime);
            yield return StartCoroutine(MoveToTarget(moveTime));

            // After moving, decide whether to rest
            if (!isResting && Random.value < restChance)
            {
                // Decide whether to sit or lie down
                if (Random.value < lieChance)
                {
                    yield return StartCoroutine(LieDownRoutine());
                }
                else
                {
                    yield return StartCoroutine(SitRoutine());
                }
            }
        }
    }

    IEnumerator SitRoutine()
    {
        isResting = true;
        restState = CatRestState.Sitting;
        isMoving = false;

        // Disable animator and show sitting sprite
        if (animator != null)
            animator.enabled = false;

        if (spriteRenderer != null && sittingSprite != null)
            spriteRenderer.sprite = sittingSprite;

        Debug.Log("Cat is sitting");

        // Sit for random time
        float restTime = Random.Range(minSitTime, maxSitTime);
        yield return new WaitForSeconds(restTime);

        // Stand up
        StandUp();
    }

    IEnumerator LieDownRoutine()
    {
        isResting = true;
        restState = CatRestState.Lying;
        isMoving = false;

        // Disable animator and show lying sprite
        if (animator != null)
            animator.enabled = false;

        if (spriteRenderer != null && lyingSprite != null)
            spriteRenderer.sprite = lyingSprite;

        Debug.Log("Cat is lying down");

        // Lie down for random time (longer than sitting)
        float restTime = Random.Range(minLieTime, maxLieTime);
        yield return new WaitForSeconds(restTime);

        // Stand up
        StandUp();
    }

    void StandUp()
    {
        isResting = false;
        restState = CatRestState.None;

        // Enable animator and restore original sprite
        if (animator != null)
            animator.enabled = true;

        if (spriteRenderer != null && originalSprite != null)
            spriteRenderer.sprite = originalSprite;

        Debug.Log("Cat stood up");
    }

    void SetNewRandomTarget()
    {
        // Choose random position within roaming area
        float randomX = Random.Range(roamAreaCenter.x - roamAreaSize.x / 2, roamAreaCenter.x + roamAreaSize.x / 2);
        float randomY = Random.Range(roamAreaCenter.y - roamAreaSize.y / 2, roamAreaCenter.y + roamAreaSize.y / 2);

        targetPosition = new Vector2(randomX, randomY);
        isMoving = true;

        Debug.Log($"Cat is going to position: {targetPosition}");
    }

    IEnumerator MoveToTarget(float moveTime)
    {
        float elapsedTime = 0f;
        Vector2 startPosition = transform.position;

        while (elapsedTime < moveTime && isMoving && !isResting)
        {
            // Smooth movement to target
            float t = elapsedTime / moveTime;
            Vector2 newPosition = Vector2.Lerp(startPosition, targetPosition, t);

            if (rb != null)
            {
                rb.MovePosition(newPosition);
            }
            else
            {
                transform.position = newPosition;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure we reach the target
        if (!isResting)
        {
            if (rb != null)
            {
                rb.MovePosition(targetPosition);
            }
            else
            {
                transform.position = targetPosition;
            }
        }

        isMoving = false;
    }

    void UpdateAnimation()
    {
        if (animator != null && animator.enabled)
        {
            // Set animation parameters
            animator.SetBool("IsMoving", isMoving);

            if (isMoving)
            {
                // Determine movement direction for sprite rotation
                Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;

                if (spriteRenderer != null)
                {
                    // Rotate sprite in movement direction on X axis
                    if (direction.x > 0)
                        spriteRenderer.flipX = false;
                    else if (direction.x < 0)
                        spriteRenderer.flipX = true;
                }

                // Set direction for animation
                animator.SetFloat("MoveX", direction.x);
                animator.SetFloat("MoveY", direction.y);
            }
        }
    }

    // Visualize roaming area in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(roamAreaCenter, roamAreaSize);

        // Show current target if moving
        if (Application.isPlaying && isMoving)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetPosition);
            Gizmos.DrawWireSphere(targetPosition, 0.2f);
        }
    }

    // Methods for external control

    /// <summary>
    /// Scare the cat - make it run in random direction
    /// </summary>
    public void ScareCat()
    {
        if (isResting)
            StandUp();

        StopAllCoroutines();
        SetNewRandomTarget();
        StartCoroutine(MoveToTarget(1f)); // Run fast for 1 second
        StartCoroutine(WanderRoutine()); // Continue normal wandering
    }

    /// <summary>
    /// Call cat to specific position
    /// </summary>
    public void CallCat(Vector2 position)
    {
        if (isResting)
            StandUp();

        StopAllCoroutines();
        targetPosition = position;
        StartCoroutine(MoveToTarget(3f));
        StartCoroutine(WanderRoutine());
    }

    /// <summary>
    /// Set new roaming area
    /// </summary>
    public void SetRoamArea(Vector2 center, Vector2 size)
    {
        roamAreaCenter = center;
        roamAreaSize = size;
    }

    /// <summary>
    /// Stop cat in place
    /// </summary>
    public void StopCat()
    {
        if (isResting)
            StandUp();

        StopAllCoroutines();
        isMoving = false;
        if (animator != null)
            animator.SetBool("IsMoving", false);
    }

    /// <summary>
    /// Force cat to sit
    /// </summary>
    public void ForceSit()
    {
        if (!isResting)
        {
            StopAllCoroutines();
            StartCoroutine(SitRoutine());
        }
    }

    /// <summary>
    /// Force cat to lie down
    /// </summary>
    public void ForceLieDown()
    {
        if (!isResting)
        {
            StopAllCoroutines();
            StartCoroutine(LieDownRoutine());
        }
    }

    /// <summary>
    /// Force cat to stand up
    /// </summary>
    public void ForceStandUp()
    {
        if (isResting)
        {
            StandUp();
            StartCoroutine(WanderRoutine());
        }
    }

    /// <summary>
    /// Get current rest state of the cat
    /// </summary>
    public CatRestState GetRestState()
    {
        return restState;
    }

    /// <summary>
    /// Check if cat is currently resting
    /// </summary>
    public bool IsResting()
    {
        return isResting;
    }

    /// <summary>
    /// Check if cat is currently moving
    /// </summary>
    public bool IsMoving()
    {
        return isMoving;
    }
}