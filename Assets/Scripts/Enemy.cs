using UnityEngine; // Provides access to Unity engine core functionality

public class Enemy : MonoBehaviour // Enemy behavior script attached to enemy GameObject
{
    // ---------------- STATE MACHINE ----------------
    public enum EnemyState // Defines all possible enemy states
    {
        Idle,   // Enemy stands still
        Chase,  // Enemy moves toward player
        Attack  // Enemy attacks player
    }

    // ---------------- STATS ----------------
    [Header("Stats")] // Inspector header for stats
    public int damage = 1; // Damage dealt to player per attack
    public float attackCooldown = 2f; // Time between attacks

    // ---------------- MOVEMENT ----------------
    [Header("Movement")] // Inspector header for movement settings
    public float moveSpeed = 2f; // Movement speed of enemy
    public float detectionRange = 5f; // Distance at which enemy starts chasing
    public float attackRange = 1.5f; // Distance at which enemy attacks

    // ---------------- REFERENCES ----------------
    [Header("References")] // Inspector header for references
    public Transform player; // Reference to player transform

    private Rigidbody2D rb; // Rigidbody for movement physics
    private Animator animator; // Animator for handling animations
    private GameManagerScript gameManager; // Reference to game manager
    private Health playerHealth; // Reference to player's health script

    // ---------------- INTERNAL STATE ----------------
    private EnemyState currentState; // Current AI state
    private float attackTimer; // Timer controlling attack cooldown
    private bool isDead; // Tracks whether enemy is dead

    // ---------------- UNITY START ----------------
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Get Rigidbody2D component
        animator = GetComponent<Animator>(); // Get Animator component
        gameManager = FindFirstObjectByType<GameManagerScript>(); // Find GameManager in scene

        attackTimer = attackCooldown; // Initialize attack timer

        // -------- FIND PLAYER AUTOMATICALLY --------
        if (player == null) // If player not assigned in Inspector
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player"); // Find player by tag
            if (playerObj != null)
                player = playerObj.transform; // Assign player transform
        }

        // -------- CACHE PLAYER HEALTH --------
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>(); // Get player's Health script

            if (playerHealth == GetComponent<Health>())
            {
                Debug.LogError("Enemy is targeting itself as player!");
            }
        }

        currentState = EnemyState.Idle; // Start in Idle state
        Debug.Log("[Enemy] Initialized."); // Debug log
    }

    // ---------------- MAIN UPDATE LOOP ----------------
    void Update()
    {
        if (isDead || player == null) return; // Stop logic if dead or no player

        float distance = Vector2.Distance(transform.position, player.position); // Calculate distance to player

        // -------- STATE MANAGEMENT --------
        if (distance <= attackRange) // If player is in attack range
        {
            if (currentState != EnemyState.Attack) // Only switch if not already attacking
            {
                currentState = EnemyState.Attack; // Switch to Attack state
                attackTimer = attackCooldown; // Reset attack timer
                //Debug.Log("[Enemy] Entering ATTACK state."); // Debug log
            }
        }
        else if (distance <= detectionRange) // If player is within detection range
        {
            if (currentState != EnemyState.Chase) // Only switch if not already chasing
            {
                currentState = EnemyState.Chase; // Switch to Chase state
                //Debug.Log("[Enemy] Entering CHASE state."); // Debug log
            }
        }
        else // If player is out of range
        {
            if (currentState != EnemyState.Idle) // Only switch if not already idle
            {
                currentState = EnemyState.Idle; // Switch to Idle state
                //Debug.Log("[Enemy] Entering IDLE state."); // Debug log
            }
        }

        HandleAttackTimer(distance); // Handle attack timing logic
        UpdateAnimations(); // Update animation states
    }

    // ---------------- FIXED UPDATE (PHYSICS) ----------------
    void FixedUpdate()
    {
        if (isDead) return; // Stop movement if dead

        switch (currentState) // Check current state
        {
            case EnemyState.Idle:
                StopMoving(); // Stop movement
                break;

            case EnemyState.Chase:
                ChasePlayer(); // Move toward player
                break;

            case EnemyState.Attack:
                StopMoving(); // Stop moving while attacking
                break;
        }
    }

    // ---------------- ATTACK TIMER ----------------
    void HandleAttackTimer(float distance)
    {
        if (currentState != EnemyState.Attack) return; // Only run if attacking

        attackTimer -= Time.deltaTime; // Decrease timer

        if (attackTimer <= 0) // If cooldown finished
        {
            if (distance <= attackRange) // Double-check player is still in range
            {
                Attack(); // Perform attack
            }
            else
            {
                Debug.Log("[Enemy] Player moved out of attack range."); // Debug log
            }

            attackTimer = attackCooldown; // Reset cooldown
        }
    }

    // ---------------- CHASE LOGIC ----------------
    void ChasePlayer()
    {
        float deltaX = player.position.x - transform.position.x; // Horizontal distance to player
        float direction = 0f; // Movement direction (-1 or 1)

        float flipThreshold = 0.2f; // Prevents jitter when very close

        if (Mathf.Abs(deltaX) > flipThreshold) // Only move if beyond threshold
        {
            direction = Mathf.Sign(deltaX); // Determine direction (-1 or 1)
        }

        // Move enemy manually toward player
        transform.position += new Vector3(direction * moveSpeed * Time.fixedDeltaTime, 0, 0);

        // Flip sprite to face movement direction
        if (direction != 0)
        {
            transform.localScale = new Vector3(direction, 1, 1);
        }

        // Trigger movement animation
        if (animator != null)
            animator.SetBool("IsRunning", true);
    }

    // ---------------- STOP MOVEMENT ----------------
    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Stop horizontal velocity

        if (animator != null)
            animator.SetBool("IsRunning", false); // Stop movement animation
    }

    // ---------------- ATTACK ----------------
    void Attack()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > attackRange) return;

        if (animator != null)
            animator.SetTrigger("IsAttacking");

        if (playerHealth != null && playerHealth.gameObject != gameObject) // Prevent self-damage
        {
            Debug.Log("Enemy hitting: " + playerHealth.gameObject.name);
            playerHealth.TakeDamage(damage);
        }
    }

    // ---------------- ANIMATION HANDLER ----------------
    void UpdateAnimations()
    {
        if (animator == null) return; // Safety check

        animator.SetBool("IsRunning", currentState == EnemyState.Chase); // True when chasing
        animator.SetBool("IsIdle", currentState == EnemyState.Idle); // True when idle
    }

    // ---------------- EXTERNAL ATTACK STATE (OPTIONAL) ----------------
    public void SetAttackState(bool value)
    {
        if (animator != null)
        {
            animator.SetBool("Attack", value); // Sets attack animation bool
        }
    }

    // ---------------- DEATH ----------------
    public void Die()
    {
        if (isDead) return; // Prevent multiple death calls

        isDead = true; // Mark as dead

        StopMoving(); // Stop all movement

        if (animator != null)
            animator.SetTrigger("IsDead"); // Play death animation

        //if (gameManager != null)
        //    gameManager.EnemyKilled(); // Notify GameManager

        Destroy(gameObject, 1.5f); // Destroy enemy after delay
    }
}