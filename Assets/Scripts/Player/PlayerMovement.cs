using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class PlayerMovement : MonoBehaviour
{
    private GameManagerScript gameManager;

    public float speed = 5.0f;
    public float runMultiplier = 1.75f;
    public int maxJumps = 2;
    public float groundPoundForce = 25f;

    [Header("Movement Smoothing")]
    public float movementSmoothing = 10f;

    private Vector2 moveDirection;
    private Rigidbody2D rb;
    private int availableJumps;

    private Animator animator;
    private PlayerCombat playerCombat;

    public enum PlayerState
    {
        Idle,
        Move,
        Jump,
        GroundPound,
        Busy
    }

    public PlayerState currentState;

    public GameObject PlayerShadow;
    private GameObject activeShadow;

    private Vector2 lastNonZeroInput;
    private bool isRunning;

    private Vector2 currentVelocity;

    Gamepad currentGamepad;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerCombat = GetComponent<PlayerCombat>();
        gameManager = FindFirstObjectByType<GameManagerScript>();

        availableJumps = maxJumps;
        currentState = PlayerState.Idle;
    }

    void Update()
    {
        currentGamepad = InputSystem.devices.OfType<Gamepad>().FirstOrDefault();

        moveDirection = GetOnmiInput(currentGamepad);

        if (playerCombat.isShielded || playerCombat.isAttacking)
        {
            SetState(PlayerState.Busy);
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (IsJumpPressed(currentGamepad) && availableJumps > 0 && currentState != PlayerState.GroundPound)
        {
            Jump();
        }

        if (IsGroundPoundPressed(currentGamepad) && currentState == PlayerState.Jump)
        {
            GroundPound();
        }

        HandleMovement();
        UpdateState();
    }

    void UpdateState()
    {
        if (currentState == PlayerState.Jump || currentState == PlayerState.GroundPound)
            return;

        SetState(moveDirection.sqrMagnitude > 0.01f ? PlayerState.Move : PlayerState.Idle);
    }

    void SetState(PlayerState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        if (animator != null)
            animator.SetBool("IsRunning", newState == PlayerState.Move);
    }

    void HandleMovement()
    {
        if (moveDirection.sqrMagnitude < 0.01f)
            moveDirection = Vector2.zero;

        float currentSpeed = speed;

        if (IsRunHeld(currentGamepad))
            currentSpeed *= runMultiplier;

        if (currentState != PlayerState.Busy)
        {
            Vector2 targetVelocity = moveDirection * currentSpeed;

            currentVelocity = Vector2.Lerp(
                currentVelocity,
                targetVelocity,
                movementSmoothing * Time.deltaTime
            );

            rb.linearVelocity = currentVelocity;

            // WALK SOUND (only when actually moving)
            if (moveDirection.sqrMagnitude > 0.01f)
                AudioManager.Instance?.PlayWalk();
        }

        if (animator != null)
        {
            animator.SetFloat("InputX", moveDirection.x);
            animator.SetFloat("InputY", moveDirection.y);

            isRunning = moveDirection.sqrMagnitude > 0.01f;

            if (isRunning)
                lastNonZeroInput = moveDirection;
            else
            {
                animator.SetFloat("LastInputX", lastNonZeroInput.x);
                animator.SetFloat("LastInputY", lastNonZeroInput.y);
            }
        }
    }

    private void Jump()
    {
        availableJumps--;
        SetState(PlayerState.Jump);

        if (activeShadow == null)
            activeShadow = Instantiate(PlayerShadow, transform.position, Quaternion.identity);

        if (animator != null)
        {
            animator.ResetTrigger("IsJumping");
            animator.SetTrigger("IsJumping");
        }
    }

    private void GroundPound()
    {
        SetState(PlayerState.GroundPound);

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(Vector2.down * groundPoundForce, ForceMode2D.Impulse);
    }

    public void Land()
    {
        availableJumps = maxJumps;

        if (activeShadow != null)
        {
            transform.position = activeShadow.transform.position;
            Destroy(activeShadow);
            activeShadow = null;
        }

        SetState(PlayerState.Idle);
    }

    // CALL THIS WHEN PLAYER HITS ENEMY
    public void PlayAttackSound()
    {
        AudioManager.Instance?.PlayAttack();
    }

    // CALL THIS FROM Health.TakeDamage()
    public void PlayHurtSound()
    {
        AudioManager.Instance?.PlayHurt();
    }

    private Vector2 GetOnmiInput(Gamepad currentGamepad)
    {
        float gamepadX = 0f, gamepadY = 0f;
        float keyboardX = 0f, keyboardY = 0f;

        if (currentGamepad != null)
        {
            gamepadX = currentGamepad.leftStick.x.ReadValue();
            gamepadY = currentGamepad.leftStick.y.ReadValue();
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) keyboardY += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) keyboardY -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) keyboardX -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) keyboardX += 1f;
        }

        return (Mathf.Abs(gamepadX) > 0.1f || Mathf.Abs(gamepadY) > 0.1f)
            ? new Vector2(gamepadX, gamepadY)
            : new Vector2(keyboardX, keyboardY);
    }

    private bool IsJumpPressed(Gamepad currentGamepad)
    {
        return (currentGamepad != null && currentGamepad.aButton.wasPressedThisFrame) ||
               (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame);
    }

    private bool IsGroundPoundPressed(Gamepad currentGamepad)
    {
        return (currentGamepad != null && currentGamepad.leftStick.y.ReadValue() < -0.5f && currentGamepad.aButton.wasPressedThisFrame) ||
               (Keyboard.current != null && Keyboard.current.sKey.isPressed && Keyboard.current.spaceKey.wasPressedThisFrame);
    }

    private bool IsRunHeld(Gamepad currentGamepad)
    {
        return (currentGamepad != null &&
               (currentGamepad.leftStickButton.isPressed || currentGamepad.rightTrigger.ReadValue() > 0.1f)) ||
               (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed);
    }
}

// ***************  THIS IS THE END OF THE CODE  ************************


// ***************  KEY TERMS  ************************
//  variable
//  inspector
//  declaring
//  initializing
//  public
//  private
//  debug.log
//  string
//  float
//  integer (aka 'int')
//  GameObject
//  Input
//  KeyCode
//  string
//  Rigidbody2D
//  Vector2
//  Vector3
//  ||
//  &&
//  ++
//  *
//  ==
//  =
// !=






// ***************  IGNORE EVERYTHING BELOW THIS LINE!  ************************