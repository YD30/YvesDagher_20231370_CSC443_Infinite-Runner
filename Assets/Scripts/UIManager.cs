using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject GameOverPanel;

    private bool shown = false;

     void Update()
    {
        if (GameManager.Instance.IsGameOver && !shown)
        {
            Debug.Log("GAME OVER DETECTED");
            GameOverPanel.SetActive(true);
            shown = true;
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
