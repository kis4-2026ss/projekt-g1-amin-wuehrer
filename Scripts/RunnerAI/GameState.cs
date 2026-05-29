using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    public enum DifficultyLevel { Easy, Medium, Hard, Impossible }
    public DifficultyLevel currentDifficulty = DifficultyLevel.Easy;
    public bool difficultySelected = false;

    [Header("Settings")]
    public float initialSpeed = 10f;
    public float speedIncreaseRate = 0.05f;
    public float maxSpeed = 30f;

    [Header("Current State")]
    public float currentSpeed;
    public float score;
    public float difficultyMetric; // 0 to 1
    public float gameStartTime;
    public bool isCountingDown = true;

    private void Awake()
    {
        Debug.Log("GameState Awake called on " + gameObject.name);
        Instance = this;
    }

    public void StartGame(DifficultyLevel level)
    {
        currentDifficulty = level;
        difficultySelected = true;
        
        // Uniform speed settings for all difficulties
        initialSpeed = 7.2f; 
        maxSpeed = 50f;
        speedIncreaseRate = 0.025f; // 2.5% increase per second

        currentSpeed = initialSpeed;
        gameStartTime = Time.time;
        isCountingDown = true;
    }

    private void Update()
    {
        if (!difficultySelected) return;

        if (isCountingDown)
        {
            float elapsed = Time.time - gameStartTime;
            if (elapsed >= 3f)
            {
                isCountingDown = false;
                Debug.Log("COUNTDOWN FINISHED - Starting Game Logic");
            }
            return;
        }

        // Increment speed by 2% per second, capped at maxSpeed
        if (currentSpeed < maxSpeed)
        {
            currentSpeed += currentSpeed * speedIncreaseRate * Time.deltaTime;
            if (currentSpeed > maxSpeed) currentSpeed = maxSpeed;
        }
        
        // Score based on survival time
        score += Time.deltaTime;
        difficultyMetric = Mathf.Clamp01((currentSpeed - initialSpeed) / (maxSpeed - initialSpeed));
    }
}


