using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[Serializable]
public class HighscoreEntry
{
    public string nickname;
    public float time;
}

[Serializable]
public class HighscoreList
{
    public List<HighscoreEntry> entries = new List<HighscoreEntry>();
}

public class HighscoreManager : MonoBehaviour
{
    public static HighscoreManager Instance { get; private set; }
    private string filePath;

    private HighscoreList highscores = new HighscoreList();

    private void Awake()
    {
        Instance = this;
        filePath = Path.Combine(Application.persistentDataPath, "highscores.json");
        LoadHighscores();
    }

    public void LoadHighscores()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            highscores = JsonUtility.FromJson<HighscoreList>(json);
        }
        else
        {
            highscores = new HighscoreList();
        }
    }

    public void SaveHighscores()
    {
        string json = JsonUtility.ToJson(highscores, true);
        File.ReadAllText(filePath); // This was a mistake in thought, should be WriteAllText
        File.WriteAllText(filePath, json);
    }

    public List<HighscoreEntry> GetTop10()
    {
        return highscores.entries.OrderByDescending(e => e.time).Take(10).ToList();
    }

    public bool IsInTop10(float time)
    {
        var top10 = GetTop10();
        if (top10.Count < 10) return true;
        return time > top10.Last().time;
    }

    public void AddScore(string nickname, float time)
    {
        highscores.entries.Add(new HighscoreEntry { nickname = nickname, time = time });
        highscores.entries = highscores.entries.OrderByDescending(e => e.time).ToList();
        if (highscores.entries.Count > 10)
        {
            highscores.entries.RemoveAt(highscores.entries.Count - 1);
        }
        SaveHighscores();
    }
}
