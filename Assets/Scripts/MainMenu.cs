using UnityEngine;
using UnityEngine.SceneManagement;
// Optional: If implementing a loading screen with an IEnumerator
// using System.Collections; 

public class MainMenu : MonoBehaviour
{
    // If you use a ScriptableObject for scene management, you would reference it here.
    // For this example, the string reference is kept but the method is made async.
    [SerializeField] private string gameSceneName = "Game";

    // Called when the "Play Game" button is clicked
    public void PlayGame()
    {
        // Resume time if it was paused (e.g., in a pause menu context)
        Time.timeScale = 1f;

        // Ensure the cursor is visible and not locked for standard menu behavior.
        // The previous code was setting visible to false and lockState to None which
        // are contradictory (None should make it visible, Locked/Confined hides/locks it).
        // For a main menu, you typically want the cursor visible and free.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Modern approach: use async loading for better performance.
        // For simplicity, we call it directly here. For a proper loading screen,
        // you would start a Coroutine (StartCoroutine(LoadGameAsync())) from another manager script
        // and add the current scene to the build settings.
        SceneManager.LoadSceneAsync(gameSceneName, LoadSceneMode.Single);
    }

    // Called when the "Quit Game" button is clicked
    public void QuitGame()
    {
        // Best practice to handle quitting correctly in both the Editor and a built game.
#if UNITY_EDITOR
            // Exits Play mode in the Unity Editor
            UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quits the application when running as a built executable
        Debug.Log("Quit function called"); // Check the Console for this message
        Application.Quit();
#endif
        // MAKE SURE THAT YOU HAVE AN EVENT SYSTEM IN THE SCENE OF THE CANVAS SO THAT WAY THE BUTTON INTAKE CAN BE REGISTERED BY THE SYSTEM.
    }
}
