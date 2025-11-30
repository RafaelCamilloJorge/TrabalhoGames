using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] string winMessage = "Você Venceu!";
    [SerializeField] string loseMessage = "Fim de Jogo";

    [Header("Scenes")]
    [SerializeField] string gameplayScene = "Gameplay";
    [SerializeField] string menuScene = "SampleScene";

    void Start()
    {
        bool won = false;
        int score = 0;

        if (GameStateManager.Instance != null)
        {
            won = GameStateManager.Instance.LastResultWasWin;
            score = GameStateManager.Instance.LastResultScore;
            Debug.Log($"[EndGameUI] Loaded result from GameStateManager win:{won} score:{score}");
        }
        else
        {
            Debug.LogWarning("[EndGameUI] GameStateManager.Instance is null; defaulting lose/score 0");
        }

        // Fallback: try persisted values if runtime singleton não estiver disponível.
        if (score <= 0 && PlayerPrefs.HasKey("LastResultScore"))
        {
            score = PlayerPrefs.GetInt("LastResultScore", score);
            won = PlayerPrefs.GetInt("LastResultWasWin", won ? 1 : 0) == 1;
            Debug.Log($"[EndGameUI] Fallback usando PlayerPrefs win:{won} score:{score}");
        }

        // Fallback: if score veio zerado mas o ScoreService ainda tem valor, usa ele.
        if (score <= 0 && ScoreService.Score > 0)
        {
            score = ScoreService.Score;
            Debug.Log($"[EndGameUI] Fallback usando ScoreService.Score = {score}");
        }

        if (titleText != null)
            titleText.text = won ? winMessage : loseMessage;

        if (scoreText != null)
            scoreText.text = $"{score}";
    }

    public void RestartGame()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.ResetStateForRestart();
        else
            ScoreService.Reset();

        if (!string.IsNullOrEmpty(gameplayScene))
            SceneManager.LoadScene(gameplayScene);
    }

    public void ReturnToMenu()
    {
        if (!string.IsNullOrEmpty(menuScene))
            SceneManager.LoadScene(menuScene);
    }
}
