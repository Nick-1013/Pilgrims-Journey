using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    // ---------------- ATTACK SETTINGS ----------------
    [Header("Attack")] // Creates a header in the Unity Inspector
    public float attackRange = 2.5f; // Radius of attack hit detection
    public int attackDamage = 1; // Damage dealt per attack
    public Transform attackPoint;
    public Animator animator; // Reference to Animator for animations
    public bool isAttacking = false; // Public flag other scripts (like PlayerMovement) can check to see if player is currently attacking

    public LayerMask enemyLayer; // Layer mask to specify which layers are considered enemies for attack detection

    // ---------------- SHIELD ----------------
    // Public flag other scripts (like Health) can check
    public bool isShielded = false;
    public bool canShield = true;
    public float shieldCooldown = 2f; // Time in seconds before shield can be used again
    public float shieldTimer = 0f; // Time in seconds that the shield stays active

    // ---------------- ATTACK SYSTEM ----------------
    void Attack() // FIXED: removed unnecessary parameter
    {
        Debug.Log("[Player] ATTACK!"); // Debug log for attack trigger

        // Detect all colliders within attack range
        Vector2 attackCenter = attackPoint.position;

        // Draw this always
        Debug.DrawLine(transform.position, attackCenter, Color.red, 1f);
        Debug.DrawRay(attackCenter, Vector2.up * attackRange, Color.green, 1f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, attackRange, enemyLayer);
        Debug.Log("Hits found: " + hits.Length);
        Debug.Log("Enemy LayerMask value: " + enemyLayer.value);

        // Loop through all detected colliders
        foreach (Collider2D col in hits)
        {
            Debug.Log("Hit object: " + col.name);

            hits[0].GetComponent<Health>().TakeDamage(attackDamage);
        }
    }

    private void Update()
    {
        // -------- ATTACK INPUT --------
        // Check for mouse click (left click)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // If shielding, ignore attack input
            if (!isShielded)
                AttackAnimation(); // Trigger attack
        }

        // -------- SHIELD INPUT (right mouse) --------
        if(canShield)
            HandleShieldInput();
        else if (!canShield)
        {
            shieldTimer += Time.deltaTime;
            if (shieldTimer >= shieldCooldown)
            {
                canShield = true;
                shieldTimer = 0f;
            }
        }
    }

    void AttackAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsRunning", false); // Stop running animation if active
            animator.SetBool("IsAttacking", true); // Trigger attack animation (now uses "IsAttacking")
        }
        isAttacking = true; // Set the public flag to indicate the player is attacking
    }

    // Called by an Animation Event at the end of the attack animation
    public void StopAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsRunning", true); // Ensure running is off
            animator.SetBool("IsAttacking", false); // Turn off isAttacking so animation returns to idle
        }
        isAttacking = false; // Reset the public flag to indicate the player has stopped attacking
    }

    // Toggle shield on right-click (wasPressedThisFrame)
    private void HandleShieldInput()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.rightButton.isPressed)
        {
            if (!isShielded)
                StartShield(); 
        }
        else
        {
            if (isShielded)
            StopShield();
        }
    }

    private void StartShield()
    {
        isShielded = true;
        if (animator != null)
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsShielded", true); // Trigger shield animation
        Debug.Log("[Player] Shield ON");
    }

    private void StopShield()
    {
        isShielded = false;
        if (animator != null)
            animator.SetBool("IsRunning", true);
            animator.SetBool("IsShielded", false); // Turn off shield animation so player returns to idle
        Debug.Log("[Player] Shield OFF");
    }
}
