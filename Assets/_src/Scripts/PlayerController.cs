using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;
    private Vector2 lastMovement = Vector2.down; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleWASDInput();
        UpdateAnimations();
    }

    void HandleWASDInput()
    {
        movement = Vector2.zero;


        if (Input.GetKey(KeyCode.W)) movement.y = 1;     
        if (Input.GetKey(KeyCode.S)) movement.y = -1;     
        if (Input.GetKey(KeyCode.A)) movement.x = -1;     
        if (Input.GetKey(KeyCode.D)) movement.x = 1;      

        movement = movement.normalized;

        if (movement.magnitude > 0.1f)
        {
            lastMovement = movement;
        }
    }

    void UpdateAnimations()
    {
        animator.SetFloat("MoveX", movement.x);
        animator.SetFloat("MoveY", movement.y);
        animator.SetFloat("LastMoveX", lastMovement.x);
        animator.SetFloat("LastMoveY", lastMovement.y);
        animator.SetBool("IsMoving", movement.magnitude > 0.1f);
    }

    void FixedUpdate()
    {
        if (movement.magnitude > 0.1f)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }
}