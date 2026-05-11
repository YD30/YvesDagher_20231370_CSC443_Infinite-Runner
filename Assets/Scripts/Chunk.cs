using UnityEngine;

[System.Flags]
public enum LaneMask
{
    None = 0,
    Left = 1 << 0,
    Middle = 1 << 1,
    Right = 1 << 2,
    All = Left | Middle | Right,
}

public static class LaneMaskExtensions
{
    public static bool ConnectsTo(this LaneMask exit, LaneMask nextEntry)
    {
        return (exit & nextEntry) != 0;
    }
}

public class Chunk : MonoBehaviour
{
    [Header("Chunk Size")]
    [SerializeField] private float length = 30f;

    [Header("Lane Connectivity")]
    [SerializeField] private LaneMask entry = LaneMask.All;
    [SerializeField] private LaneMask exit = LaneMask.All;

    [Header("Gameplay Flow")]
    [Tooltip("Recommended lane player should naturally end in.")]
    [SerializeField] private LaneMask recommendedLane = LaneMask.Middle;

    [Tooltip("How much free space exists near the START of the chunk.")]
    [SerializeField] private float entryBuffer = 3f;

    [Tooltip("How much free space exists near the END of the chunk.")]
    [SerializeField] private float exitBuffer = 3f;

    public float Length => length;
    public LaneMask Entry => entry;
    public LaneMask Exit => exit;

    public LaneMask RecommendedLane => recommendedLane;

    public float EntryBuffer => entryBuffer;
    public float ExitBuffer => exitBuffer;

    [SerializeField] private LaneMask safeCoinLanes = LaneMask.All;

    public LaneMask SafeCoinLanes => safeCoinLanes;

    public bool HasObstacleInLane(int lane)
    {
        // optional if you want manual control later
        return false;
    }

    // Visual gizmos
    void OnDrawGizmos()
    {
        Vector3 back = transform.position + Vector3.back * (length * 0.5f);
        Vector3 fwd = transform.position + Vector3.forward * (length * 0.5f);

        DrawLaneGizmos(back, entry, Color.green);
        DrawLaneGizmos(fwd, exit, Color.red);
    }

    private static void DrawLaneGizmos(Vector3 origin, LaneMask mask, Color color)
    {
        const float laneOffset = 2f;

        Gizmos.color = color;

        Vector3 size = new Vector3(0.4f, 1.2f, 0.4f);
        Vector3 center = origin + Vector3.up * 0.6f;

        if ((mask & LaneMask.Left) != 0)
            Gizmos.DrawWireCube(center + Vector3.left * laneOffset, size);

        if ((mask & LaneMask.Middle) != 0)
            Gizmos.DrawWireCube(center, size);

        if ((mask & LaneMask.Right) != 0)
            Gizmos.DrawWireCube(center + Vector3.right * laneOffset, size);
    }
}