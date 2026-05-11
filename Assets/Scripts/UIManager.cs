using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject GameOverPanel;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI finalText;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI finalCoinText;

    private bool shown = false;

     void Update()
    {
        if (GameOverPanel == null) return;
         float score = GameManager.Instance.Distance;
         int coins = GameManager.Instance.Coins;

        scoreText.text = $"Score: {score:0}";
        coinText.text = coins.ToString();

        if (GameManager.Instance.IsGameOver && !shown)
        {
            Debug.Log("GAME OVER DETECTED");
            GameOverPanel.SetActive(true);
            shown = true;
            finalText.text = $"Final Score: {score:0}";
            finalCoinText.text = $"Final Coins: {coins}";
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
