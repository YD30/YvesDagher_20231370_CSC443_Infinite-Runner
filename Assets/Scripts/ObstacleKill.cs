using UnityEngine;

public class ObstacleKill : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        GameManager.Instance.IsGameOver = true;
    }
}