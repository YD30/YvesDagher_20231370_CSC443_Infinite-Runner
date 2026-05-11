using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Infinite Runner/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Speed")]
    public float startSpeed = 8f;
    public float maxSpeed = 20f;
    public float speedIncreaseRate = 0.1f;
}
