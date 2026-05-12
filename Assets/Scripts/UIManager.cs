using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    [Header("Game Over")]
    [SerializeField] private GameObject GameOverPanel;

    [Header("Pause")]
    [SerializeField] private GameObject PausePanel;

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("Final Score")]
    [SerializeField] private TextMeshProUGUI finalText;
    [SerializeField] private TextMeshProUGUI finalCoinText;

    private bool shown = false;
    private bool paused = false;

    void Start()
    {
        Time.timeScale = 1f;

        if (PausePanel != null)
            PausePanel.SetActive(false);

        if (GameOverPanel != null)
            GameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        float score = GameManager.Instance.Distance;
        int coins = GameManager.Instance.Coins;

        // UPDATE HUD
        if (scoreText != null)
            scoreText.text = $"Score: {score:0}";

        if (coinText != null)
            coinText.text = coins.ToString();

        // GAME OVER
        if (GameManager.Instance.IsGameOver && !shown)
        {
            Debug.Log("GAME OVER DETECTED");

            if (GameOverPanel != null)
                GameOverPanel.SetActive(true);

            if (finalText != null)
                finalText.text = $"Final Score: {score:0}";

            if (finalCoinText != null)
                finalCoinText.text = $"Final Coins: {coins}";

            shown = true;

            Time.timeScale = 0f;
        }

        // ESC PAUSE TOGGLE
        if (Keyboard.current.escapeKey.wasPressedThisFrame &&
            !GameManager.Instance.IsGameOver)
        {
            if (paused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        if (PausePanel != null)
            PausePanel.SetActive(true);

        Time.timeScale = 0f;
        paused = true;
    }

    public void Resume()
    {
        if (PausePanel != null)
            PausePanel.SetActive(false);

        Time.timeScale = 1f;
        paused = false;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}