using UnityEngine;

public enum FacingFlipMode
{
    TransformScale,
    SpriteRendererFlipX
}

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private PlayerInputReader inputReader;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float facingInputThreshold = 0.01f;
    [SerializeField] private FacingFlipMode facingFlipMode = FacingFlipMode.TransformScale;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private Vector3 initialScale;

    public int FacingSign { get; private set; } = 1;
    public FacingFlipMode FlipMode => facingFlipMode;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initialScale = transform.localScale;

        if (inputReader == null)
        {
            inputReader = GetComponent<PlayerInputReader>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        ApplyFacingVisuals();
    }

    private void OnEnable()
    {
        if (inputReader != null)
        {
            inputReader.OnJumpPressed += HandleJumpPressed;
        }
    }

    private void OnDisable()
    {
        if (inputReader != null)
        {
            inputReader.OnJumpPressed -= HandleJumpPressed;
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Update()
    {
        if (inputReader == null) return;

        UpdateFacing(inputReader.MoveInput.x);
    }

    private void Move()
    {
        Vector2 moveInput = inputReader.MoveInput;

        rb.linearVelocity = new Vector2(
            moveInput.x * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private void UpdateFacing(float horizontalInput)
    {
        if (Mathf.Abs(horizontalInput) <= facingInputThreshold) return;

        FacingSign = horizontalInput > 0f ? 1 : -1;
        ApplyFacingVisuals();
    }

    private void ApplyFacingVisuals()
    {
        switch (facingFlipMode)
        {
            case FacingFlipMode.SpriteRendererFlipX:
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = FacingSign < 0;
                }

                Vector3 scale = initialScale;
                scale.x = Mathf.Abs(initialScale.x);
                transform.localScale = scale;
                break;

            default:
                Vector3 transformScale = initialScale;
                transformScale.x = Mathf.Abs(initialScale.x) * FacingSign;
                transform.localScale = transformScale;
                break;
        }
    }

    private void HandleJumpPressed()
    {
        if (!IsGrounded()) return;

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
