using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class RouletteManager : MonoBehaviour
{
    // ---------- UI ----------
    [Header("UI References")]
    public TMP_Dropdown colorDropdown;
    public TMP_Dropdown numberDropdown;
    public TMP_InputField betInput;
    public Button spinButton;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI turnText;
	public TextMeshProUGUI messageText;

    [Header("Roulette Spinner")]
    public RouletteSpinner spinner;

    [Header("Clock System")]
    public TextMeshProUGUI clockText;
    public float totalMinutes = 360f; // 6 minutes = 360 seconds
    private float timeLeft;

    [Header("Sacrifice UI")]
    public SacrificeUI sacrificeUI;
    private bool introTriggered = false;

    // ---------- Game state ----------
    public int money = 2500;
    public int targetMoney = 50000;
    private int turnCount = 0;

    // ---------- Sounds ----------
    [Header("Audio (optional)")]
    public AudioSource audioSource;      // Single audio source
    public AudioClip errorClip;
    public AudioClip spinClip;
    public AudioClip jackpotClip;
    public AudioClip timeWarpClip;

    // ---------- European roulette sets ----------
    private readonly List<int> redNumbers = new List<int> { 1,3,5,7,9,12,14,16,18,19,21,23,25,27,30,32,34,36 };
    private readonly List<int> blackNumbers = new List<int> { 2,4,6,8,10,11,13,15,17,20,22,24,26,28,29,31,33,35 };

    // Money counter coroutine
    private Coroutine moneyCoroutine = null;

    // ---------- Unity lifecycle ----------
    void Start()
    {
        SetupDropdowns();
        UpdateNumberDropdown();
        UpdateUI();

        spinButton.onClick.AddListener(SpinRoulette);
        colorDropdown.onValueChanged.AddListener((_) => UpdateNumberDropdown());

        // start clock
        timeLeft = totalMinutes;
        StartCoroutine(TimeRoutine());
    }

    // ---------- Dropdowns ----------
    void SetupDropdowns()
    {
        colorDropdown.ClearOptions();
        colorDropdown.AddOptions(new List<string> { "Red", "Black", "Green" });
    }

    void UpdateNumberDropdown()
    {
        numberDropdown.ClearOptions();
        List<string> nums = new List<string>();
        string color = colorDropdown.options[colorDropdown.value].text;

        if (color == "Red") foreach (int n in redNumbers) nums.Add(n.ToString());
        else if (color == "Black") foreach (int n in blackNumbers) nums.Add(n.ToString());
        else nums.Add("0"); // Green

        numberDropdown.AddOptions(nums);
        numberDropdown.value = 0;
        numberDropdown.RefreshShownValue();
    }

    // ---------- Roulette spin ----------
    public void SpinRoulette()
    {
        if (!int.TryParse(betInput.text, out int bet) || bet <= 0)
        {
            PlaySfx(errorClip);
            spinner?.DisplayMessage("Enter a valid bet amount.");
            return;
        }

        if (bet > money)
        {
            PlaySfx(errorClip);
            spinner?.DisplayMessage("You don't have enough money.");
            return;
        }

        spinButton.interactable = false;
        money -= bet;

        int chosenNumber = int.Parse(numberDropdown.options[numberDropdown.value].text);
        string chosenColor = colorDropdown.options[colorDropdown.value].text;

        int resultNumber = Random.Range(0, 37);
        string resultColor = GetColor(resultNumber);

        bool numberMatch = (resultNumber == chosenNumber);
        bool colorMatch = (resultColor == chosenColor);

        int baseMultiplier = numberMatch && colorMatch ? 25 :
                             numberMatch ? 10 :
                             colorMatch ? 2 : 0;

        int finalWinnings = bet * baseMultiplier;

        // If jackpot (number + color), sacrifice time!
        if (numberMatch && colorMatch && finalWinnings > 0)
        {
            ApplyTimeSacrifice(30f); // burn 30 minutes of life
            PlaySfx(jackpotClip);
        }

        int prevMoney = money;
        money = Mathf.Max(0, money + finalWinnings);
        turnCount++;

        // Sacrifice intro trigger
        if (!introTriggered && turnCount >= Random.Range(2, 4))
        {
            introTriggered = true;
            sacrificeUI?.TriggerIntro();
        }

        // Notify sacrifice system of turn progression
        sacrificeUI?.OnTurnAdvanced(turnCount);

        UpdateUI(animated: true, prevMoney: prevMoney);

        float displayMultiplier = (bet > 0 && finalWinnings > 0) ? (float)finalWinnings / bet : 0f;
        spinner?.PlaySpin(resultNumber, resultColor, bet, finalWinnings, displayMultiplier);

        if (money <= 0)
        {
            money = 0;
			StartCoroutine(HandleLoseSequence());
        }
        else if (money >= targetMoney)
        {
			StartCoroutine(HandleWinSequence());
        }
        else
        {
            StartCoroutine(ReEnableButtonAfter(spinner != null ? spinner.spinDuration + 0.12f : 0.5f));
        }
    }

	private IEnumerator HandleWinSequence()
	{
		ResultTXT("You paid your debt! Your soul is yours again!");

		yield return new WaitForSeconds(1.5f);

		SceneManager.LoadScene(3);
	}

	private IEnumerator HandleLoseSequence()
	{
		ResultTXT("You lost! Your soul now belongs to me!");

		yield return new WaitForSeconds(1.5f);

		SceneManager.LoadScene(4);
	}
	
	public void ResultTXT(string msg)
    {
        if (messageText != null)
        {
            messageText.text = msg;
            messageText.color = Color.white;
        }
    }

    IEnumerator ReEnableButtonAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (money > 0 && money < targetMoney && timeLeft > 0)
        {
            spinButton.interactable = true;
        }
    }

    string GetColor(int num)
    {
        if (num == 0) return "Green";
        if (redNumbers.Contains(num)) return "Red";
        return "Black";
    }

    // ---------- Clock ----------
    IEnumerator TimeRoutine()
    {
        while (timeLeft > 0)
        {
            float tick = Random.Range(0.5f, 2f);   // minutes to subtract
            timeLeft -= tick;

            UpdateClockUI();

            yield return new WaitForSeconds(Random.Range(2f, 6f));
        }

        
		StartCoroutine(HandleLoseSequence());
        spinButton.interactable = false;
    }

    void UpdateClockUI()
    {
        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60);
        clockText.text = $"{minutes:00}:{seconds:00} left";
    }

    // ---------- Sacrifice ----------
    public void ApplyTimeSacrifice(float penaltyMinutes)
    {
        timeLeft = Mathf.Max(0, timeLeft - penaltyMinutes);
        PlaySfx(timeWarpClip);
        spinner?.DisplayMessage($"Time sacrificed! -{penaltyMinutes} minutes");
        UpdateClockUI();

        StartCoroutine(FlashClockRed());
    }

    IEnumerator FlashClockRed()
    {
        Color original = clockText.color;
        clockText.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        clockText.color = original;
    }

    // ---------- UI updates ----------
    void UpdateUI(bool animated = false, int prevMoney = 0)
    {
        turnText.text = $"Turn {turnCount}";

        if (animated)
        {
            if (moneyCoroutine != null) StopCoroutine(moneyCoroutine);
            moneyCoroutine = StartCoroutine(CountUpMoney(prevMoney, money));
        }
        else
        {
            moneyText.text = $"${money}";
        }
    }

    IEnumerator CountUpMoney(int start, int end)
    {
        if (start == end)
        {
            moneyText.text = $"${end}";
            yield break;
        }

        float duration = end < start ? 0.8f : 1.0f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            int val = Mathf.RoundToInt(Mathf.Lerp(start, end, t));
            moneyText.text = $"${val}";
            yield return null;
        }
        moneyText.text = $"${end}";
        moneyCoroutine = null;
    }

    // ---------- Audio helper ----------
    void PlaySfx(AudioClip clip)
    {
        if (audioSource && clip)
            audioSource.PlayOneShot(clip);
    }
}
