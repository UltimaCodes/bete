using UnityEngine;
using TMPro;
using System.Collections;

public class TypewriterCutscene : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dialogueText;

    [Header("Settings")]
    public float typeSpeed = 0.05f;   // time between letters
    public float sentenceDelay = 1.5f; // wait after sentence finishes

    [Header("Dialogue")]
    [TextArea(2, 10)]
    public string[] sentences;

    [Header("Scene Switcher")]
    public SceneSwitcher sceneSwitcher; // reference to SceneSwitcher in scene

    private int currentSentence = 0;

    void Start()
    {
        if (sentences.Length > 0)
            StartCoroutine(RunCutscene());
    }

    private IEnumerator RunCutscene()
    {
        while (currentSentence < sentences.Length)
        {
            // Type the sentence
            yield return StartCoroutine(TypeSentence(sentences[currentSentence]));

            // Wait after sentence is done
            yield return new WaitForSeconds(sentenceDelay);

            // Clear text
            dialogueText.text = "";

            currentSentence++;
        }

        EndCutscene();
    }

    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    private void EndCutscene()
    {
        dialogueText.text = ""; // ensure text is cleared
        Debug.Log("Cutscene ended.");

        if (sceneSwitcher != null)
        {
            sceneSwitcher.LoadScene(1); // load scene 1
        }
        else
        {
            Debug.LogWarning("SceneSwitcher reference not set!");
        }
    }
}