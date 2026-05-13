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

    [Header("Ground")]
    [SerializeField] private float defaultGroundY = 0f;

    private int _laneIndex;

    private float _y;
    private float _yVel;

    private Vector2 _prevMove;

    private float _groundY;
    private bool _isGrounded;

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
        _y = defaultGroundY;
    }

    public void Move(InputAction.CallbackContext ctx)
    {
        Vector2 v = ctx.ReadValue<Vector2>();

        if (v.x > 0.5f && _prevMove.x <= 0.5f)
            TryChangeLane(+1);
        else if (v.x < -0.5f && _prevMove.x >= -0.5f)
            TryChangeLane(-1);

        // SINGLE JUMP ONLY
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

        // IMPORTANT: check at BODY HEIGHT ONLY (not full capsule)
        Vector3 checkPos = new Vector3(
            targetX,
            transform.position.y + 1.0f, // body height, not feet
            transform.position.z + 0.2f  // slightly forward to avoid edge hits
        );

        Vector3 halfExtents = new Vector3(
            0.35f, // narrower = no side extremity blocking
            0.5f,  // only body zone
            0.25f  // small forward depth
        );

        int mask = LayerMask.GetMask("Obstacle");

        Collider[] hits = Physics.OverlapBox(
            checkPos,
            halfExtents,
            Quaternion.identity,
            mask
        );

        foreach (var hit in hits)
        {
            // IGNORE OBSTACLES BELOW PLAYER (standing on top)
            if (hit.bounds.max.y < transform.position.y - 0.2f)
                continue;

            // IGNORE SIDE EXTREMITY GLITCH:
            // if player is already almost aligned, don't false-block
            float centerDistance = Mathf.Abs(hit.bounds.center.x - targetX);

            if (centerDistance > 0.8f)
                continue;

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
        // GROUND DETECTION (STABLE)
        // -------------------------
        _groundY = defaultGroundY;
        _isGrounded = false;

        if (Physics.Raycast(
            transform.position + Vector3.up * 0.5f,
            Vector3.down,
            out RaycastHit hit,
            5f))
        {
            ObstacleSurface surface =
                hit.collider.GetComponent<ObstacleSurface>();

            if (surface != null)
            {
                _groundY = hit.collider.bounds.max.y;
            }
            else
            {
                _groundY = hit.point.y;
            }

            // grounded stability (no flicker)
            if (_y <= _groundY + 0.05f)
                _isGrounded = true;
        }

        // -------------------------
        // LANDING
        // -------------------------
        if (_y < _groundY)
        {
            _y = _groundY;
            _yVel = 0f;
            _isGrounded = true;
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