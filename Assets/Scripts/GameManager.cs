using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameConfig config;

    public float ScrollSpeed { get; private set; }
    public float Distance { get; private set; }
    public int Coins { get; private set; }

    public bool IsGameOver = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        ScrollSpeed = config.startSpeed;
    }

    void Update()
    {
        if (IsGameOver) return;
        ScrollSpeed = Mathf.Min(ScrollSpeed + config.speedIncreaseRate * Time.deltaTime, config.maxSpeed);
        Distance += ScrollSpeed * Time.deltaTime;
    }

    public void AddCoin() => Coins++;
}
