using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    [Header("Target UI (optional if on same GO)")]
    [SerializeField] TMP_Text tmpText;
    [SerializeField] string prefix = "Score: ";
    [SerializeField] bool showMax = true;
    [SerializeField] string maxPrefix = " / Max: ";

    void Awake()
    {
        if (tmpText == null) TryGetComponent(out tmpText);
    }

    void OnEnable()
    {
        ScoreService.OnScoreChanged += HandleScoreChanged;
    }

    void OnDisable()
    {
        ScoreService.OnScoreChanged -= HandleScoreChanged;
    }

    void Start()
    {
        HandleScoreChanged(ScoreService.Score);
    }

    void HandleScoreChanged(int score)
    {
        string text = showMax ? ($"{prefix}{score}{maxPrefix}{ScoreService.HighScore}") : ($"{prefix}{score}");
        if (tmpText != null) tmpText.text = text;
    }
}
