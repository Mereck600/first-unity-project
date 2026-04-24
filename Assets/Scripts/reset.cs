using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class reset : MonoBehaviour
{
    [Tooltip("The input action that triggers the scene reset.")]
    public InputAction resetAction;

    private void OnEnable()
    {
        // Enable the action and subscribe to the performed event
        resetAction.Enable();
        resetAction.performed += OnResetPerformed;
    }

    private void OnDisable()
    {
        // Unsubscribe and disable the action
        resetAction.performed -= OnResetPerformed;
        resetAction.Disable();
    }

    private void OnResetPerformed(InputAction.CallbackContext context)
    {
        // Reload the currently active scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
    