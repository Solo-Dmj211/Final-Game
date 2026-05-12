using Unity.Mathematics;
using UnityEngine;
using System.Collections;

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
    public float wallJumpDuration = 0.2f;
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

    [Header("Particles")]
    public ParticleSystem groundJumpParticles;

    [Header("State")]
    public bool isDisabled = false;

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

    public void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (isDisabled) return;
        moveInput = ctx.ReadValue<Vector2>().x;
    }

    public void OnJump(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (isDisabled) return;
        if (ctx.performed)
            jumpPressed = true;
    }

    public void OnSprint(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (isDisabled) return;
        sprintHeld = ctx.ReadValueAsButton();
    }

    public void SetMovementEnabled(bool enabled)
    {
        isDisabled = !enabled;

        // If disabled, zero out all inputs so the player doesn't drift or
        // carry a buffered jump into the re-enabled state.
        if (isDisabled)
        {
            moveInput       = 0f;
            jumpPressed     = false;
            sprintHeld      = false;
            jumpBufferTimer = 0f;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }


    void Update()
    {
        if (isDisabled) return;

        CheckEnvironment();
        HandleTimers();
        HandleJumpLogic();
        UpdateAnimation();

        jumpPressed = false;
    }

    void FixedUpdate()
    {
        if (isDisabled) return;

        ApplyMovement();
        ApplyWallSlide();
    }


    void CheckEnvironment()
    {
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, environmentLayer);

        Vector2 facingDir = new Vector2(Mathf.Sign(transform.localScale.x), 0);
        RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, facingDir, wallDistance, environmentLayer);
        touchingWall = hit;

        if (touchingWall)
            wallDir = (int)Mathf.Sign(transform.localScale.x);
    }


    void HandleTimers()
    {
        if (grounded) coyoteTimer = coyoteTime;
        else coyoteTimer -= Time.deltaTime;

        if (jumpPressed) jumpBufferTimer = jumpBufferTime;
        else jumpBufferTimer -= Time.deltaTime;

        if (wallJumpTimer > 0)
            wallJumpTimer -= Time.deltaTime;
    }

    void ApplyMovement()
    {
        if (wallJumpTimer > 0) return;

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

        float speedDiff  = targetSpeed - rb.linearVelocity.x;
        float accelRate  = (Mathf.Abs(targetSpeed) > 0.1f) ? accel : decel;
        float movement   = speedDiff * accelRate;

        rb.AddForce(Vector2.right * movement);

        if (moveInput != 0)
            transform.localScale = new Vector3(Mathf.Sign(moveInput), transform.localScale.y, transform.localScale.z);
    }

    void HandleJumpLogic()
    {
        // ground jump
        if (jumpBufferTimer > 0 && coyoteTimer > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferTimer = 0;
            coyoteTimer = 0;
            groundJumpParticles?.Play();
        }

        // wall jump
        if (jumpBufferTimer > 0 && touchingWall && !grounded)
        {
            rb.linearVelocity = new Vector2(-wallDir * wallJumpX, wallJumpY);
            jumpBufferTimer = 0;
            wallJumpTimer = wallJumpDuration;
            transform.localScale = new Vector3(-wallDir, transform.localScale.y, transform.localScale.z);
        }
    }

    void ApplyWallSlide()
    {
        if (touchingWall && !grounded && rb.linearVelocity.y < 0)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
    }

    void UpdateAnimation()
    {
        if (!anim) return;

        if (math.abs(rb.linearVelocityX) > 0.5)
            anim.SetTrigger("Moving");
        else
            anim.SetTrigger("Idle");
    }

}