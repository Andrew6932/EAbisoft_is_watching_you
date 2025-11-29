using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private AnimationManager animationManager;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animationManager = GetComponent<AnimationManager>();
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        movement = Vector2.zero;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.wKey.isPressed) movement.y = 1;
        if (keyboard.sKey.isPressed) movement.y = -1;
        if (keyboard.aKey.isPressed) movement.x = -1;
        if (keyboard.dKey.isPressed) movement.x = 1;

        // Обновляем анимации
        if (animationManager != null)
        {
            animationManager.UpdateAnimations(movement);
        }
    }

    void FixedUpdate()
    {
        if (movement.magnitude > 0.1f)
        {
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        }
    }
}