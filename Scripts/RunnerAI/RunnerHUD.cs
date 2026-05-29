using UnityEngine;
using UnityEngine.UI;

public class RunnerHUD : MonoBehaviour
{
    public Text scoreText;
    public Text countdownText;

    void Update()
    {
        if (GameState.Instance == null || !GameState.Instance.difficultySelected) 
        {
            if (scoreText != null) scoreText.gameObject.SetActive(false);
            if (countdownText != null) countdownText.gameObject.SetActive(false);
            return;
        }

        // Show HUD components once difficulty is selected
        if (scoreText != null && !scoreText.gameObject.activeSelf) scoreText.gameObject.SetActive(true);
        if (countdownText != null && !countdownText.gameObject.activeSelf && GameTimeIsRecent()) countdownText.gameObject.SetActive(true);

        // Update Score (Survival Time)
        if (scoreText != null)
        {
            scoreText.text = "Time: " + GameState.Instance.score.ToString("F3") + "s";
        }

        // Update Countdown
        if (countdownText != null)
        {
            if (GameState.Instance.isCountingDown)
            {
                float remaining = 3f - (Time.time - GameState.Instance.gameStartTime);
                if (remaining > 0)
                {
                    countdownText.text = Mathf.CeilToInt(remaining).ToString();
                }
                else
                {
                    countdownText.text = "GO!";
                }
            }
            else
            {
                // Hide countdown after a short delay
                if (Time.time - GameState.Instance.gameStartTime > 4f)
                {
                    countdownText.gameObject.SetActive(false);
                }
            }
        }
    }

    private bool GameTimeIsRecent()
    {
        return Time.time - GameState.Instance.gameStartTime < 4f;
    }
}

