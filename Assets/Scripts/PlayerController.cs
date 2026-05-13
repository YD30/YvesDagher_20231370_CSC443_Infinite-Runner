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

    private float _groundY;
    private bool _isGrounded;

    private float _lastGroundTime;
    private const float groundGrace = 0.08f;

    // ✅ FIX: stabilize ground value
    private float _smoothedGroundY;

    void Awake()
    {
        if (TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void Start()
    {
        _y = 0f;
        _smoothedGroundY = 0f;
        _groundY = 0f;
    }

    public void Move(InputAction.CallbackContext ctx)
    {
        Vector2 v = ctx.ReadValue<Vector2>();

        if (v.x > 0.5f && _prevMove.x <= 0.5f)
            TryChangeLane(+1);
        else if (v.x < -0.5f && _prevMove.x >= -0.5f)
            TryChangeLane(-1);

        if (v.y > 0.5f && _prevMove.y <= 0.5f && _isGrounded)
        {
            _yVel = jumpVelocity;
            _isGrounded = false;
        }

        _prevMove = v;
    }

    private void TryChangeLane(int delta)
    {
        int half = laneCount / 2;
        int targetLane = Mathf.Clamp(_laneIndex + delta, -half, half);

        if (!LaneBlocked(targetLane))
        {
            _laneIndex = targetLane;
        }
    }

    private bool LaneBlocked(int targetLane)
    {
        float targetX = targetLane * laneOffset;

        Vector3 checkPos = new Vector3(
            targetX,
            transform.position.y + 1.0f,
            transform.position.z + 0.3f
        );

        Vector3 halfExtents = new Vector3(0.4f, 0.6f, 0.3f);

        int mask = LayerMask.GetMask("Obstacle");

        Collider[] hits = Physics.OverlapBox(
            checkPos,
            halfExtents,
            Quaternion.identity,
            mask
        );

        foreach (var hit in hits)
        {
            if (hit.bounds.max.y < transform.position.y - 0.2f)
                continue;

            float xDist = Mathf.Abs(hit.bounds.center.x - targetX);

            if (xDist < 0.9f)
                return true;
        }

        return false;
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver)
            return;

        // -------------------------
        // GRAVITY
        // -------------------------
        _yVel += gravity * Time.deltaTime;
        _y += _yVel * Time.deltaTime;

        // -------------------------
        // GROUND DETECTION
        // -------------------------
        bool hitGround = false;

        if (Physics.Raycast(
            transform.position + Vector3.up * 0.5f,
            Vector3.down,
            out RaycastHit hit,
            5f))
        {
            // ✅ FIX: smooth ground to remove jitter
            _smoothedGroundY = Mathf.Lerp(_smoothedGroundY, hit.point.y, 0.25f);

            _groundY = _smoothedGroundY;

            if (_y <= _groundY + 0.05f)
            {
                hitGround = true;
                _lastGroundTime = Time.time;
            }
        }

        _isGrounded = (Time.time - _lastGroundTime) < groundGrace;

        // -------------------------
        // LANDING
        // -------------------------
        if (_y < _groundY)
        {
            _y = _groundY;
            _yVel = 0f;
        }

        // -------------------------
        // MOVE PLAYER
        // -------------------------
        Vector3 pos = transform.position;

        pos.x = Mathf.MoveTowards(
            pos.x,
            _laneIndex * laneOffset,
            laneSwitchSpeed * Time.deltaTime
        );

        pos.y = _y;
        pos.z = 0f;

        transform.position = pos;
    }
}