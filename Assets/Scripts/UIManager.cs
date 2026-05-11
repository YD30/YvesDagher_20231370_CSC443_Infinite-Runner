using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject GameOverPanel;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI finalText;

    private bool shown = false;

     void Update()
    {
        if (GameOverPanel == null) return;
         float score = GameManager.Instance.Distance;
         scoreText.text = $"Score: {score:0}";

        if (GameManager.Instance.IsGameOver && !shown)
        {
            Debug.Log("GAME OVER DETECTED");
            GameOverPanel.SetActive(true);
            shown = true;
            finalText.text = $"Final Score: {score:0}";
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
