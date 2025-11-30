using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    public static event Action<int, int> OnLivesChanged;

    [Header("Vidas")]
    [SerializeField] int maxLives = 3;
    [SerializeField] bool resetStarsOnDeath;

    [Header("UI / Finais")]
    [SerializeField] GameObject winScreen;
    [SerializeField] GameObject loseScreen;
    [SerializeField] TMP_Text finalScoreText;
    [SerializeField] string fallbackWinScene = "Win";
    [SerializeField] string fallbackLoseScene = "GameOver";
    [SerializeField] string endGameScene = "EndGame";

    int _lives;
    bool _gameEnded;
    bool _lastResultWasWin;
    int _lastResultScore;

    public bool GameEnded => _gameEnded;
    public int Lives => _lives;
    public int MaxLives => maxLives;
    public bool LastResultWasWin => _lastResultWasWin;
    public int LastResultScore => _lastResultScore;

    public void ResetStateForRestart()
    {
        _gameEnded = false;
        maxLives = Mathf.Max(1, maxLives);
        _lives = maxLives;
        _lastResultWasWin = false;
        _lastResultScore = 0;
        NotifyLivesChanged();
        ScoreService.Reset();
        CollectibleStar.ResetAll();
        Debug.Log("[GameStateManager] ResetStateForRestart - lives and score reset");
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureExists()
    {
        if (Instance != null) return;
        var go = new GameObject("GameStateManager");
        go.AddComponent<GameStateManager>();
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        maxLives = Mathf.Max(1, maxLives);
        _lives = maxLives;
        _gameEnded = false;
    }

    void OnEnable()
    {
        ScoreService.OnScoreChanged += HandleScoreChanged;
        NotifyLivesChanged();
    }

    void OnDisable()
    {
        ScoreService.OnScoreChanged -= HandleScoreChanged;
    }

    void Start()
    {
        CheckForWin(ScoreService.Score);
    }

    public void HandlePlayerFall(Vector3 respawnPosition, Rigidbody playerRb)
    {
        if (_gameEnded) return;

        RespawnPlayer(playerRb, respawnPosition);
        LoseLife();
    }

    public void ManualReset(Vector3 respawnPosition, Rigidbody playerRb)
    {
        _gameEnded = false;
        maxLives = Mathf.Max(1, maxLives);
        _lives = maxLives;
        NotifyLivesChanged();

        CollectibleStar.ResetAll();
        ScoreService.Reset();

        RespawnPlayer(playerRb, respawnPosition);
    }

    void RespawnPlayer(Rigidbody playerRb, Vector3 respawnPosition)
    {
        playerRb.linearVelocity = Vector3.zero;
        playerRb.angularVelocity = Vector3.zero;
        playerRb.position = respawnPosition;
    }

    void LoseLife()
    {
        if (_lives <= 0) return;

        _lives = Mathf.Max(0, _lives - 1);
        Debug.Log($"[GameStateManager] LoseLife -> lives now {_lives}/{maxLives}");
        NotifyLivesChanged();

        if (resetStarsOnDeath)
        {
            CollectibleStar.ResetAll();
            ScoreService.Reset();
        }

        if (_lives <= 0)
            EndGame(false);
    }

    void HandleScoreChanged(int score)
    {
        CheckForWin(score);
    }

    void CheckForWin(int score)
    {
        if (_gameEnded) return;

        int targetScore = 2;
        if (targetScore <= 0) return;

        if (score >= targetScore)
            EndGame(true);
    }

    void EndGame(bool won)
    {
        _gameEnded = true;
        Debug.Log($"[GameStateManager] EndGame triggered - won:{won} score:{ScoreService.Score}");
        ShowEndScreen(won);
    }

    void ShowEndScreen(bool won)
    {
        _lastResultWasWin = won;
        _lastResultScore = ScoreService.Score;
        PlayerPrefs.SetInt("LastResultWasWin", _lastResultWasWin ? 1 : 0);
        PlayerPrefs.SetInt("LastResultScore", _lastResultScore);
        PlayerPrefs.Save();

        if (finalScoreText != null)
            finalScoreText.text = $"{ScoreService.Score}";

        if (won && winScreen != null)
        {
            winScreen.SetActive(true);
            return;
        }

        if (!won && loseScreen != null)
        {
            loseScreen.SetActive(true);
            return;
        }

        string targetScene = !string.IsNullOrEmpty(endGameScene) ? endGameScene : (won ? fallbackWinScene : fallbackLoseScene);
        Debug.Log($"[GameStateManager] EndGame ({(won ? "WIN" : "LOSE")}) loading scene '{targetScene}'");

        if (!string.IsNullOrEmpty(targetScene) && Application.CanStreamedLevelBeLoaded(targetScene))
        {
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            Debug.LogError($"{(won ? "Vitória" : "Derrota")} - Cena '{targetScene}' não encontrada ou não está no Build Settings.");
        }
    }

    void NotifyLivesChanged()
    {
        OnLivesChanged?.Invoke(_lives, maxLives);
    }
}
