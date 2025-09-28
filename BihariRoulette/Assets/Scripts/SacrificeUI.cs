using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// I love using a copious amount of fire emojis at 3 am send help im going to actually breakdown
public class SacrificeUI : MonoBehaviour
{
    [Header("References")]
    public GameObject panel;              // dialogue panel
    public TextMeshProUGUI dialogueText;
    public Button sacrificeButton;        // button itself
    public GameObject readyIndicator;     // small image that shows when ready
    public AudioClip readySfx;            // SFX when it's available

    [Header("Typewriter Settings")]
    public float typeSpeed = 0.04f;

    private RouletteManager manager;
    private AudioSource audioSource;

    private int turnsSinceLast = 0;
    private bool unlocked = false;
    private bool canUse = false;

    void Start()
    {
        manager = FindObjectOfType<RouletteManager>();
        audioSource = GetComponent<AudioSource>();

        panel.SetActive(false);
        sacrificeButton.gameObject.SetActive(false);
        readyIndicator.SetActive(false);

        sacrificeButton.onClick.AddListener(OnSacrificePressed);
    }

    public void TriggerIntro()
    {
        StartCoroutine(IntroRoutine());
    }

    IEnumerator IntroRoutine()
    {
        panel.SetActive(true);
        dialogueText.text = "";

        string message = "Dealer: This is taking too long... I have an idea.\nWhat if you ruin your future odds of a jackpot by getting a bigger bonus right now! I'll give you this button if you're open to indulge ;)";

        // Typewriter effect
        foreach (char c in message)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        yield return new WaitForSeconds(1.0f);

        panel.SetActive(false);
        sacrificeButton.gameObject.SetActive(true);
        unlocked = true;
        UpdateAvailability();
    }

    public void OnTurnAdvanced(int turnCount)
    {
        if (!unlocked) return;

        turnsSinceLast++;
        if (turnsSinceLast >= 3 && !canUse)
        {
            canUse = true;
            readyIndicator.SetActive(true);
            sacrificeButton.interactable = true; // 🔥 enable button properly
            if (audioSource && readySfx) audioSource.PlayOneShot(readySfx);
        }
    }

    void OnSacrificePressed()
    {
        if (!canUse || !unlocked) return;

        if (manager != null)
        {
            manager.ApplyTimeSacrifice(20f);
            manager.spinner?.DisplayMessage("You embraced the sacrifice... More power now, less future.");
        }

        // reset cooldown
        canUse = false;
        turnsSinceLast = 0;
        readyIndicator.SetActive(false);
        sacrificeButton.interactable = false; // 🔥 disable button until ready again
    }

    private void UpdateAvailability()
    {
        sacrificeButton.interactable = canUse;
    }
}
