using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;   // default menu
    public GameObject creditsPanel;
    public GameObject infoPanel;

    [Header("Game Settings")]
    public int gameSceneIndex = 1; // where "Play" goes
    public int menuSceneIndex = 2; // menu scene itself (safety)

	public void Start()
	{
		mainPanel.SetActive(true);
		creditsPanel.SetActive(false);
		infoPanel.SetActive(false);
	}

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneIndex);
    }

    public void ShowCredits()
    {
        mainPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void ShowInfo()
    {
        mainPanel.SetActive(false);
        infoPanel.SetActive(true);
    }

    public void BackToMenu()
    {
        creditsPanel.SetActive(false);
        infoPanel.SetActive(false);
        mainPanel.SetActive(true);
    }
}