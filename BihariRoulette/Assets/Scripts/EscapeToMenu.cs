using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class EscapeToMenu : MonoBehaviour
{
    public InputAction backAction;     // assign this to the “Back/Escape” action in inspector
    public float doublePressTime = 0.4f;
    private float lastPressTime = -1f;

    void OnEnable()
    {
        backAction.Enable();
        backAction.performed += OnBackPerformed;
    }

    void OnDisable()
    {
        backAction.performed -= OnBackPerformed;
        backAction.Disable();
    }

    private void OnBackPerformed(InputAction.CallbackContext ctx)
    {
        float timeNow = Time.time;
        if (timeNow - lastPressTime <= doublePressTime)
        {
            SceneManager.LoadScene(2); // your menu scene index or name
        }
        else
        {
            lastPressTime = timeNow;
        }
    }
}