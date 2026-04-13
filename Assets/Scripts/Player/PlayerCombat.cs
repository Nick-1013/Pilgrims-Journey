using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    // ---------------- ATTACK SETTINGS ----------------
    [Header("Attack")] // Creates a header in the Unity Inspector
    public float attackRange = 2.5f; // Radius of attack hit detection
    public int attackDamage = 1; // Damage dealt per attack
    public Transform attackPoint;
    private Animator animator; // Reference to Animator for animations

    public LayerMask enemyLayer; // Layer mask to specify which layers are considered enemies for attack detection


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


            /*
            // Only apply damage if enemy was found
            if (health != null && !health.isPlayer)
            {
                Debug.Log("Applying damage to: " + health.gameObject.name);
                health.TakeDamage(attackDamage);
            }
            */
        }
    }
    void AttackAnimation()
    {
        if (animator != null)
            animator.SetTrigger("IsAttacking"); // Trigger attack animation
    }
}
