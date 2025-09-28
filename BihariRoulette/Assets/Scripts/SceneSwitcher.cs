using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // Call this function from a button or another script
    public void LoadScene(int i)
    {
        SceneManager.LoadScene(i);
    }

}