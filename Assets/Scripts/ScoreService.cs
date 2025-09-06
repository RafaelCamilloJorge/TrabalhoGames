using System;
using UnityEngine;

public class ScoreService : MonoBehaviour
{
    public static event Action<int> OnScoreChanged;
    public static int Score { get; private set; }
    public static int HighScore { get; private set; }
    static ScoreService _inst;

    void Awake()
    {
        if (_inst != null) { Destroy(gameObject); return; }
        _inst = this;
        DontDestroyOnLoad(gameObject);
        HighScore = PlayerPrefs.GetInt("HighScore", 0);
        OnScoreChanged?.Invoke(Score);
    }

    public static void Add(int v)
    {
        Score += v;
        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt("HighScore", HighScore);
            PlayerPrefs.Save();
        }
        OnScoreChanged?.Invoke(Score);
    }

    public static void Reset()
    {
        Score = 0;
        OnScoreChanged?.Invoke(Score);
    }
}
