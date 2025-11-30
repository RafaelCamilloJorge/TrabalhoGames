using TMPro;
using UnityEngine;

public class LivesUI : MonoBehaviour
{
    [SerializeField] TMP_Text tmpText;
    [SerializeField] string prefix = "Vidas: ";

    void Awake()
    {
        if (tmpText == null) TryGetComponent(out tmpText);
    }

    void OnEnable()
    {
        Debug.Log("[LivesUI] OnEnable - subscribing to OnLivesChanged");
        GameStateManager.OnLivesChanged += HandleLivesChanged;
    }

    void OnDisable()
    {
        Debug.Log("[LivesUI] OnDisable - unsubscribing from OnLivesChanged");
        GameStateManager.OnLivesChanged -= HandleLivesChanged;
    }

    void Start()
    {
        if (GameStateManager.Instance != null)
        {
            Debug.Log($"[LivesUI] Start - init with {GameStateManager.Instance.Lives}/{GameStateManager.Instance.MaxLives}");
            HandleLivesChanged(GameStateManager.Instance.Lives, GameStateManager.Instance.MaxLives);
        }
        else
        {
            Debug.LogWarning("[LivesUI] Start - GameStateManager.Instance is null");
        }
    }

    void Update()
    {
        // Fallback in caso o evento não seja disparado (ordem de execução ou objeto instanciado depois).
        if (GameStateManager.Instance != null)
        {
            Debug.Log($"[LivesUI] Update fallback - {GameStateManager.Instance.Lives}/{GameStateManager.Instance.MaxLives}");
            HandleLivesChanged(GameStateManager.Instance.Lives, GameStateManager.Instance.MaxLives);
        }
    }

    void HandleLivesChanged(int current, int max)
    {
        Debug.Log($"[LivesUI] HandleLivesChanged -> {current}/{max}");
        if (tmpText != null)
            tmpText.text = $"{prefix}{current}/{max}";
        else
            Debug.LogWarning("[LivesUI] HandleLivesChanged - tmpText is null");
    }
}
