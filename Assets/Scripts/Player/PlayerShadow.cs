using UnityEngine;

// Attach to the PlayerShadow prefab. When the player collides with this shadow. The shadow calls PlayerMovement.ResetJumps() to restore jump availability.
public class PlayerShadow : MonoBehaviour
{
    [Tooltip("If true, destroy the shadow after the player lands on it.")]
    public bool destroyOnLand = true;

    // Accept both trigger and collision setups
    void OnTriggerEnter2D(Collider2D other)
    {
        HandleLanding(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleLanding(collision.gameObject);
    }

    void HandleLanding(GameObject other)
    {
        if (other == null) return;

        var pm = other.GetComponent<PlayerMovement>();
        if (pm == null)
        {
            // try parent (in case collider is on child)
            pm = other.GetComponentInParent<PlayerMovement>();
        }

        if (pm != null)
        {
            //pm.ResetJumps();
            if (destroyOnLand)
                Destroy(gameObject);
        }
    }
}