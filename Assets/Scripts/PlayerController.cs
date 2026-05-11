using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Lanes")]
    [SerializeField] private float laneOffset = 2f;
    [SerializeField, Min(1)] private int laneCount = 3;
    [SerializeField] private float laneSwitchSpeed = 14f;

    [Header("Jump")]
    [SerializeField] private float jumpVelocity = 8f;
    [SerializeField] private float gravity = -25f;

    private int _laneIndex;
    private float _y;
    private float _yVel;
    private Vector2 _prevMove;

    void Awake()
    {
        // We drive position directly, so any rigidbody on this object must be kinematic.
        if (TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public void Move(InputAction.CallbackContext ctx)
    {
        Vector2 v = ctx.ReadValue<Vector2>();
        if (v.x > 0.5f && _prevMove.x <= 0.5f) ChangeLane(+1);
        else if (v.x < -0.5f && _prevMove.x >= -0.5f) ChangeLane(-1);
        if (v.y > 0.5f && _prevMove.y <= 0.5f && _y <= 0f) _yVel = jumpVelocity;
        _prevMove = v;
    }

    private void ChangeLane(int delta)
    {
        int half = laneCount / 2;
        _laneIndex = Mathf.Clamp(_laneIndex + delta, -half, half);
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver) return;
        _yVel += gravity * Time.deltaTime;
        _y += _yVel * Time.deltaTime;
        if (_y < 0f) { _y = 0f; _yVel = 0f; }

        Vector3 pos = transform.position;
        pos.x = Mathf.MoveTowards(pos.x, _laneIndex * laneOffset, laneSwitchSpeed * Time.deltaTime);
        pos.y = _y;
        pos.z = 0f;
        transform.position = pos;
    }
}
