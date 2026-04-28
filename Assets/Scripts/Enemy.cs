using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    // ---------------- STATE MACHINE ----------------
    public enum EnemyState
    {
        Idle,
        Chase,
        Attack
    }

    // ---------------- STATS ----------------
    [Header("Stats")]
    public int damage = 1;
    public float attackCooldown = 2f;

    // ---------------- KNOCKBACK SETTINGS ----------------
    [Header("Knockback Settings")]
    public bool applyKnockback = true;

    [Tooltip("Force applied to player on hit")]
    public Vector2 knockbackForce = new Vector2(2f, 2f);

    // ---------------- MOVEMENT ----------------
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
    public float attackRange = 1.5f;

    // ---------------- REFERENCES ----------------
    [Header("References")]
    public Transform player;

    private Rigidbody2D rb;
    private Rigidbody2D playerRb; // NEW
    private Animator animator;
    private GameManagerScript gameManager;
    private Health playerHealth;

    // ---------------- HIT/ANIMATION ----------------
    [Header("Hit / Animation")]
    public float hitBoolDuration = 0.5f;
    private Coroutine hitResetCoroutine;

    // ---------------- INTERNAL STATE ----------------
    private EnemyState currentState;
    private float attackTimer;
    private bool isDead;

    // ---------------- UNITY START ----------------
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        gameManager = FindFirstObjectByType<GameManagerScript>();

        attackTimer = attackCooldown;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
            playerRb = player.GetComponent<Rigidbody2D>(); // NEW

            if (playerHealth == GetComponent<Health>())
            {
                Debug.LogError("Enemy is targeting itself as player!");
            }
        }

        currentState = EnemyState.Idle;
    }

    // ---------------- MAIN UPDATE LOOP ----------------
    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (animator != null && HasAnimatorParameter("IsIdle") && animator.GetBool("IsIdle"))
        {
            currentState = EnemyState.Idle;
            HandleAttackTimer(distance);
            UpdateAnimations();
            return;
        }

        if (distance <= attackRange)
        {
            if (currentState != EnemyState.Attack)
            {
                currentState = EnemyState.Attack;
                attackTimer = attackCooldown;
            }
        }
        else if (distance <= detectionRange)
        {
            currentState = EnemyState.Chase;
        }
        else
        {
            currentState = EnemyState.Idle;
        }

        HandleAttackTimer(distance);
        UpdateAnimations();

        // DEBUG SCENE CONTROLS
        if (Keyboard.current.nKey.wasPressedThisFrame)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

        if (Keyboard.current.bKey.wasPressedThisFrame)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);

        if (Keyboard.current.rKey.wasPressedThisFrame)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ---------------- FIXED UPDATE ----------------
    void FixedUpdate()
    {
        if (isDead) return;

        switch (currentState)
        {
            case EnemyState.Idle:
                StopMoving();
                break;

            case EnemyState.Chase:
                ChasePlayer();
                break;

            case EnemyState.Attack:
                StopMoving();
                break;
        }
    }

    // ---------------- ATTACK TIMER ----------------
    void HandleAttackTimer(float distance)
    {
        if (currentState != EnemyState.Attack) return;

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0)
        {
            if (distance <= attackRange)
                Attack();

            attackTimer = attackCooldown;
        }
    }

    // ---------------- CHASE ----------------
    void ChasePlayer()
    {
        float deltaX = player.position.x - transform.position.x;
        float deltaY = player.position.y - transform.position.y;

        float directionX = 0f;
        float directionY = 0f;

        float threshold = 0.2f;

        if (Mathf.Abs(deltaX) > threshold)
            directionX = Mathf.Sign(deltaX);

        if (Mathf.Abs(deltaY) > threshold)
            directionY = Mathf.Sign(deltaY);

        transform.position += new Vector3(
            directionX * moveSpeed * Time.fixedDeltaTime,
            directionY * moveSpeed * Time.fixedDeltaTime,
            0
        );

        if (directionX != 0)
            transform.localScale = new Vector3(directionX, 1, 1);

        if (animator != null)
            animator.SetBool("IsRunning", true);
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (animator != null)
            animator.SetBool("IsRunning", false);
    }

    // ---------------- ATTACK ----------------
    void Attack()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > attackRange) return;

        if (animator != null)
            animator.SetTrigger("IsAttacking");

        if (playerHealth != null && playerHealth.gameObject != gameObject)
        {
            playerHealth.TakeDamage(damage);

            // KNOCKBACK
            if (applyKnockback && playerRb != null)
            {
                Vector2 direction = (player.position - transform.position).normalized;

                Vector2 force = new Vector2(
                    Mathf.Sign(direction.x) * Mathf.Abs(knockbackForce.x),
                    knockbackForce.y
                );

                playerRb.AddForce(force, ForceMode2D.Impulse);
            }
        }
    }

    // ---------------- ANIMATION ----------------
    void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetBool("IsRunning", currentState == EnemyState.Chase);
        animator.SetBool("IsIdle", currentState == EnemyState.Idle);
    }

    // ---------------- HIT ----------------
    public void AcknowledgeHit()
    {
        if (animator == null) return;
        if (!HasAnimatorParameter("IsHit")) return;

        animator.SetBool("IsHit", true);

        if (hitResetCoroutine != null)
            StopCoroutine(hitResetCoroutine);

        hitResetCoroutine = StartCoroutine(ResetHitCoroutine());
    }

    IEnumerator ResetHitCoroutine()
    {
        yield return new WaitForSeconds(hitBoolDuration);

        if (animator != null && HasAnimatorParameter("IsHit"))
            animator.SetBool("IsHit", false);

        hitResetCoroutine = null;
    }

    // ---------------- DEATH ----------------
    public void Die()
    {
        if (isDead) return;

        isDead = true;
        StopMoving();

        if (animator != null)
            animator.SetTrigger("IsDead");

        Destroy(gameObject, 1.5f);

        if (gameManager != null)
            gameManager.EnemyKilled();
    }

    // ---------------- HELPERS ----------------
    bool HasAnimatorParameter(string paramName)
    {
        if (animator == null) return false;

        foreach (var param in animator.parameters)
            if (param.name == paramName)
                return true;

        return false;
    }
}