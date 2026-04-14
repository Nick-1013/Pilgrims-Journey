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

    public LayerMask enemyLayer; // Layer mask to specify which layers are considered enemies for attack detection

    // ---------------- SHIELD ----------------
    // Public flag other scripts (like Health) can check
    public bool IsShielded = false;

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
            if (!IsShielded)
                AttackAnimation(); // Trigger attack
        }

        // -------- SHIELD INPUT (right mouse) --------
        HandleShieldInput();
    }

    void AttackAnimation()
    {
        if (animator != null)
            animator.SetBool("IsAttacking", true); // Trigger attack animation (now uses "IsAttacking")
    }

    // Called by an Animation Event at the end of the attack animation
    public void StopAttackAnimation()
    {
        if (animator != null)
            animator.SetBool("IsAttacking", false); // Turn off isAttacking so animation returns to idle
    }

    // Toggle shield on right-click (wasPressedThisFrame)
    private void HandleShieldInput()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (!IsShielded)
                StartShield();
            else
                StopShield();
        }
    }

    private void StartShield()
    {
        IsShielded = true;
        if (animator != null)
            animator.SetBool("IsShielded", true); // Trigger shield animation
        Debug.Log("[Player] Shield ON");
    }

    private void StopShield()
    {
        IsShielded = false;
        if (animator != null)
            animator.SetBool("IsShielded", false); // Turn off shield animation so player returns to idle
        Debug.Log("[Player] Shield OFF");
    }
}
