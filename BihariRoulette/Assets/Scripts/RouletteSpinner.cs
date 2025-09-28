using UnityEngine;
using TMPro;
using System.Collections;

public class RouletteSpinner : MonoBehaviour
{
    [Header("Spinner UI")]
    public TextMeshProUGUI colorSpinnerText;
    public TextMeshProUGUI numberSpinnerText;

    [Header("Result UI")]
    public TextMeshProUGUI chipsText;
    public TextMeshProUGUI multText;
    public TextMeshProUGUI totalText;
    public TextMeshProUGUI messageText;

    [Header("Spin Settings")]
    public float spinDuration = 1.8f; // shorter, snappier
    public float spinSpeed = 0.05f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip spinLoopClip;
    public AudioClip winTickClip;
    public AudioClip bigWinClip;
    public AudioClip loseClip;

    private string[] colors = { "Red", "Black", "Green" };
    private Coroutine clearCoroutine; // handle clearing result text

    public void PlaySpin(int resultNumber, string resultColor, int bet, int finalWinnings, float displayMultiplier)
    {
        // clear old message immediately when spinning again
        ClearResultTexts();

        StartCoroutine(SpinCoroutine(resultNumber, resultColor, bet, finalWinnings, displayMultiplier));
    }

    IEnumerator SpinCoroutine(int finalNumber, string finalColor, int bet, int finalWinnings, float displayMultiplier)
    {
        float timer = 0f;

        if (spinLoopClip != null && audioSource != null)
            audioSource.PlayOneShot(spinLoopClip);

        // suspense flicker
        while (timer < spinDuration)
        {
            colorSpinnerText.text = colors[Random.Range(0, colors.Length)];
            numberSpinnerText.text = Random.Range(0, 37).ToString();

            timer += spinSpeed;
            yield return new WaitForSeconds(spinSpeed);
        }

        // reveal result
        colorSpinnerText.text = finalColor;
        numberSpinnerText.text = finalNumber.ToString();

        // set bet & multiplier
        chipsText.text = $"{bet}";
        multText.text = $"{displayMultiplier:0.##}x";

        // payout animation
        if (finalWinnings <= 0)
        {
            totalText.text = "0";
            messageText.text = "LOSE...";
            messageText.color = Color.red;
            PlaySound(loseClip);
            StartClearTimer();
        }
        else
        {
            StartCoroutine(CountUpWinnings(finalWinnings));
        }
    }

    IEnumerator CountUpWinnings(int finalWinnings)
    {
        int display = 0;
        int steps = Mathf.Max(1, finalWinnings / 40);
        float duration = Mathf.Clamp(Mathf.Log(finalWinnings + 1) * 0.4f, 0.5f, 2f);
        float stepTime = duration / Mathf.Max(1, finalWinnings / steps);

        while (display < finalWinnings)
        {
            display += steps;
            if (display > finalWinnings) display = finalWinnings;

            totalText.text = $"{display}";

            // pop effect
            totalText.transform.localScale = Vector3.one * 1.2f;
            chipsText.transform.localScale = Vector3.one * 1.1f;
            multText.transform.localScale = Vector3.one * 1.1f;

            PlaySound(winTickClip);

            yield return new WaitForSeconds(stepTime);

            totalText.transform.localScale = Vector3.one;
            chipsText.transform.localScale = Vector3.one;
            multText.transform.localScale = Vector3.one;
        }

        // final color feedback
        if (finalWinnings >= 5000)
        {
            messageText.text = "JACKPOT!!!";
            messageText.color = Color.yellow;
            PlaySound(bigWinClip);
        }
        else if (finalWinnings >= 1000)
        {
            messageText.text = "HUGE WIN!";
            messageText.color = new Color(1f, 0.5f, 0f);
        }
        else
        {
            messageText.text = "NICE!";
            messageText.color = Color.green;
        }

        StartClearTimer();
    }

    public void DisplayMessage(string msg)
    {
        if (messageText != null)
        {
            messageText.text = msg;
            messageText.color = Color.white;
            StartClearTimer();
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    // --- Auto clear helpers ---
    void StartClearTimer()
    {
        if (clearCoroutine != null)
            StopCoroutine(clearCoroutine);
        clearCoroutine = StartCoroutine(ClearAfterDelay());
    }

    IEnumerator ClearAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        ClearResultTexts();
    }

    void ClearResultTexts()
    {
        if (clearCoroutine != null)
        {
            StopCoroutine(clearCoroutine);
            clearCoroutine = null;
        }

        chipsText.text = "";
        multText.text = "";
        totalText.text = "";
        messageText.text = "";
    }
}
