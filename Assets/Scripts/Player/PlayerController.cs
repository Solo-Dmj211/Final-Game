using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float accel = 12f;
    public float decel = 16f;
    public float sprintMultiplier = 1.5f;

    [Header("Jump")]
    public float jumpForce = 14f;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;

    [Header("Wall")]
    public float wallSlideSpeed = 2f;
    public float wallJumpX = 10f;
    public float wallJumpY = 14f;
    public float wallJumpDuration = 0.2f; // locks input after wall jump
    float wallJumpTimer;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float staminaDrain = 25f;
    public float staminaRegen = 15f;

    [Header("Checks")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public Transform wallCheck;
    public float wallDistance = 0.5f;
    public LayerMask environmentLayer;

    Rigidbody2D rb;
    Animator anim;

    float moveInput;
    bool jumpPressed;
    bool sprintHeld;

    float coyoteTimer;
    float jumpBufferTimer;
    float stamina;

    bool grounded;
    bool touchingWall;
    int wallDir;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        stamina = maxStamina;
    }

    // input callbacks
    public void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>().x;
        
    }

    public void OnJump(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            jumpPressed = true;
    }

    public void OnSprint(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        sprintHeld = ctx.ReadValueAsButton();
    }

    // loop
    void Update()
    {
        CheckEnvironment();
        HandleTimers();
        HandleJumpLogic();
        UpdateAnimation();

        jumpPressed = false; // consume input
    }

    void FixedUpdate()
    {
        ApplyMovement();
        ApplyWallSlide();
    }

    // environment checks
    void CheckEnvironment()
    {
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, environmentLayer);

        // fix: transform.right ignores scale. calculate actual facing direction.
        Vector2 facingDir = new Vector2(Mathf.Sign(transform.localScale.x), 0);
        RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, facingDir, wallDistance, environmentLayer);
        touchingWall = hit;

        if (touchingWall)
            wallDir = (int)Mathf.Sign(transform.localScale.x);
    }

    // manage timers
    void HandleTimers()
    {
        if (grounded) coyoteTimer = coyoteTime;
        else coyoteTimer -= Time.deltaTime;

        if (jumpPressed) jumpBufferTimer = jumpBufferTime;
        else jumpBufferTimer -= Time.deltaTime;

        if (wallJumpTimer > 0)
            wallJumpTimer -= Time.deltaTime;
    }

    // apply physics movement
    void ApplyMovement()
    {
        if (wallJumpTimer > 0) 
            return; // ignore input during wall jump

        float targetSpeed = moveInput * moveSpeed;

        if (sprintHeld && stamina > 0)
        {
            targetSpeed *= sprintMultiplier;
            stamina -= staminaDrain * Time.fixedDeltaTime;
        }
        else
        {
            stamina += staminaRegen * Time.fixedDeltaTime;
        }

        stamina = Mathf.Clamp(stamina, 0, maxStamina);

        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.1f) ? accel : decel;
        float movement = speedDiff * accelRate;

        rb.AddForce(Vector2.right * movement);

        // flip sprite
        if (moveInput != 0)
            transform.localScale = new Vector3(Mathf.Sign(moveInput), 1, 1);
    }

    // apply jumping
    void HandleJumpLogic()
    {
        // ground jump
        if (jumpBufferTimer > 0 && coyoteTimer > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferTimer = 0;
            coyoteTimer = 0; // prevent double jumping instantly
        }

        // wall jump
        if (jumpBufferTimer > 0 && touchingWall && !grounded)
        {
            rb.linearVelocity = new Vector2(-wallDir * wallJumpX, wallJumpY);
            jumpBufferTimer = 0;
            wallJumpTimer = wallJumpDuration; // lock horizontal control
            transform.localScale = new Vector3(-wallDir, 1, 1); // face away from wall
        }
    }

    // slide down walls
    void ApplyWallSlide()
    {
        if (touchingWall && !grounded && rb.linearVelocity.y < 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }
    }

    // handle animator
    void UpdateAnimation()
    {
        if (!anim) return;

        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetFloat("YVel", rb.linearVelocity.y);
        anim.SetBool("Grounded", grounded);
        anim.SetBool("WallSlide", touchingWall && !grounded);
        anim.SetBool("Sprint", sprintHeld);
    }
}