using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class PlayerMovement : MonoBehaviour
{
    private GameManagerScript gameManager; // Helps communicate with Game Manager script for pause menu/gameover screen
    public float speed = 5.0f; //How fast the player character is at normal movement
    public float runMultiplier = 1.75f; // How much faster running is
    public float jumpForce = 10.0f;
    public int maxJumps = 2; // Public variable to set the total number of jumps allowed
    public float groundPoundForce = 25f;

    private Vector2 moveDirection;
    private Rigidbody2D rb;
    private bool isGrounded;
    private int availableJumps; // Private variable to track jumps remaining
    private bool isGroundPounding;
    private Animator animator; // Reference to the Animator component

    // Animator movement tracking
    private Vector2 lastNonZeroInput;
    private bool wasMoving;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // Get the Animator component
        availableJumps = maxJumps; // Initialize available jumps on start

        gameManager = FindFirstObjectByType<GameManagerScript>();
    }


    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (transform.position.y < -10f)
        {
            gameManager.GameOver();
            enabled = false; // disable player movement
        }
        Gamepad currentGamepad = InputSystem.devices.OfType<Gamepad>().FirstOrDefault();

        moveDirection = GetOnmiInput(currentGamepad);

        // Deadzone clamp
        if (moveDirection.magnitude < 0.1f) moveDirection = Vector2.zero;

        // --- Animator: update movement-based parameters ---
        if (animator != null)
        {
            animator.SetFloat("InputX", moveDirection.x);
            animator.SetFloat("InputY", moveDirection.y);

            bool isMoving = moveDirection.magnitude >= 0.1f;

            if (isMoving && !wasMoving)
            {
                // started moving
                animator.SetBool("IsRunning", true);
            }
            else if (!isMoving && wasMoving)
            {
                // stopped moving
                animator.SetBool("IsRunning", false);
                animator.SetFloat("LastInputX", lastNonZeroInput.x);
                animator.SetFloat("LastInputY", lastNonZeroInput.y);
            }

            if (isMoving)
                lastNonZeroInput = moveDirection;

            wasMoving = isMoving;
        }
        // ---------------------------------------------------

        float currentSpeed = speed;

        if (IsRunHeld(currentGamepad))
        {
            currentSpeed *= runMultiplier;
        }

        if (!isGroundPounding)
        {
            rb.linearVelocity = moveDirection * currentSpeed;
        }

        //Before your normal jump check, include Ground Pound (only in air:)
        if (!isGrounded && !isGroundPounding && IsGroundPoundPressed(currentGamepad))
        {
            GroundPound();
            return; // Stop other movement this frame
        }

        // Check for jump input. The condition now checks if we have jumps available
        if (IsJumpPressed(currentGamepad) && availableJumps > 0)
        {
            Jump();
        }
    }

    private Vector2 GetOnmiInput(Gamepad currentGamepad)
    {
        float gamepadXInput = 0f;
        float gamepadYInput = 0f;
        float keyboardXInput = 0f;
        float keyboardYInput = 0f;

        // Gamepad
        if (currentGamepad != null)
        {
            gamepadXInput = currentGamepad.leftStick.x.ReadValue();
            gamepadYInput = currentGamepad.leftStick.y.ReadValue();
        }

        // Keyboard (A/D + Left/Right Arrows)

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) keyboardYInput += 1f;

            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) keyboardXInput -= 1f;

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) keyboardYInput -= 1f;

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) keyboardXInput += 1f;
        }

        // Prefer gamepad if active
        if (Mathf.Abs(gamepadXInput) > 0.1f || Mathf.Abs(gamepadYInput) > 0.1f)
            return new Vector2(gamepadXInput, gamepadYInput);


        return new Vector2(keyboardXInput, keyboardYInput);
    }

    private void Jump()
    {
        // When jumping, we reset the vertical velocity before applying force
        // This ensures the second jump always has the same force, regardless of gravity
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);

        availableJumps--; // Decrease the number of available jumps
    }

    private void GroundPound()
    {
        isGroundPounding = true;

        // Cancel current motion
        rb.linearVelocity = Vector2.zero;

        // Slam downward
        rb.AddForce(Vector2.down * groundPoundForce, ForceMode2D.Impulse);
    }

    private bool IsJumpPressed(Gamepad currentGamepad)
    {
        bool gamepadJump = currentGamepad != null && currentGamepad.aButton.wasPressedThisFrame;
        bool keyboardJump = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        return gamepadJump || keyboardJump;
    }

    private bool IsGroundPoundPressed(Gamepad currentGamepad)
    {
        bool gamepadPound =
            currentGamepad != null && currentGamepad.leftStick.y.ReadValue() < -0.5f && currentGamepad.aButton.wasPressedThisFrame;

        bool keyboardPound =
            Keyboard.current != null && (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) && Keyboard.current.spaceKey.wasPressedThisFrame;

        return gamepadPound || keyboardPound;
    }

    private bool IsRunHeld(Gamepad currentGamepad)
    {
        bool gamepadRun =
            currentGamepad != null && (currentGamepad.leftStickButton.isPressed || currentGamepad.rightTrigger.ReadValue() > 0.1f);

        bool keyboardRun =
            Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

        return gamepadRun || keyboardRun;
    }
    // New Unity function to detect collisions with the ground
    void OnCollisionEnter2D(Collision2D collision)
    {
        // When the player lands on the ground, reset the available jumps
        // to the maximum allowed value.
        if (collision.gameObject.CompareTag("Ground")) // Ensure your ground object has the "Ground" tag
        {
            // The following "if" statement gives the Ground Pound impact some bounce to it
            if (isGroundPounding)
            {
                rb.AddForce(Vector2.up * 2f, ForceMode2D.Impulse);
            }

            availableJumps = maxJumps;
            isGrounded = true; // Retained for ground detection logic, though less critical now
            isGroundPounding = false; // Reset ground pound
        }
    }

    // Optional: Add OnCollisionExit2D to update isGrounded status when leaving the ground
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
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