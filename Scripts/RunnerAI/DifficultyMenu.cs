using UnityEngine;
using UnityEngine.UI;

public class DifficultyMenu : MonoBehaviour
{
    public GameObject menuPanel;
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;
    public Button impossibleButton;

    void Start()
    {
        if (easyButton != null) easyButton.onClick.AddListener(() => SelectDifficulty(GameState.DifficultyLevel.Easy));
        if (mediumButton != null) mediumButton.onClick.AddListener(() => SelectDifficulty(GameState.DifficultyLevel.Medium));
        if (hardButton != null) hardButton.onClick.AddListener(() => SelectDifficulty(GameState.DifficultyLevel.Hard));
        if (impossibleButton != null) impossibleButton.onClick.AddListener(() => SelectDifficulty(GameState.DifficultyLevel.Impossible));

        // Show menu at start
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    void SelectDifficulty(GameState.DifficultyLevel level)
    {
        if (GameState.Instance != null)
        {
            GameState.Instance.StartGame(level);
        }
        
        if (menuPanel != null) menuPanel.SetActive(false);
    }
}
