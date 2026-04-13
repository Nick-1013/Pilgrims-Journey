using UnityEngine;
using UnityEngine.UI; // Optional: needed if you want to use the Unity UI components (like a Slider or Image fill) for a health bar.

public class Health : MonoBehaviour
{
    // These fields are public/SerializeField so they can be set in the Unity Inspector
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    // Reference to the UI element (e.g., a Slider or Image) to visually represent health
    // Make sure to add 'using UnityEngine.UI;' at the top of your script for this.
    // [SerializeField] private Slider healthSlider; // Use if you have a Slider
    // [SerializeField] private Image healthBarFill; // Use if you have an Image with type set to 'Filled'

    void Start()
    {
        // When the game starts, set the current health to the maximum health
        currentHealth = maxHealth;
        // UpdateHealthUI(); // Call this to set the initial state of the UI
    }

    // Public function to allow other scripts to deal damage
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        // Use Mathf.Clamp to ensure health stays between 0 and maxHealth
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // UpdateHealthUI(); // Update the UI when health changes

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Public function to allow other scripts to heal the entity
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // UpdateHealthUI(); // Update the UI when health changes
    }

    // Optional: Function to update the health bar UI
    /*
    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth / maxHealth;
        }
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }
    */

    private void Die()
    {
        // Handle death logic here (e.g., play death animation, respawn, destroy object, reload scene)
        Debug.Log(gameObject.name + " has died!");
        // For example, disable the game object:
        // gameObject.SetActive(false);
    }
}
