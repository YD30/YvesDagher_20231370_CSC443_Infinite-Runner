using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 180f;

    private void Update()
    {
        transform.Rotate(
            Vector3.up,
            rotationSpeed * Time.deltaTime,
            Space.World);
    }

    private void OnEnable()
    {
        Collider col = GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = false;
            col.enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Coin Collected");
        if (!other.CompareTag("Player"))
            return;

        GameManager.Instance.AddCoin();

        gameObject.SetActive(false);
    }
}