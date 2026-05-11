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
    public static bool ConnectsTo(this LaneMask exit, LaneMask nextEntry) => (exit & nextEntry) != 0;
}

public class Chunk : MonoBehaviour
{
    [SerializeField] private float length = 30f;

    [Tooltip("Lanes that are open at the START of this chunk. Player must enter via one of these.")]
    [SerializeField] private LaneMask entry = LaneMask.All;

    [Tooltip("Lanes that are open at the END of this chunk. Next chunk's Entry must overlap.")]
    [SerializeField] private LaneMask exit = LaneMask.All;

    [Tooltip("Units at the START of this chunk that are guaranteed obstacle-free on the safe lane.")]
    [SerializeField] private float entryBuffer = 3f;

    [Tooltip("Units at the END of this chunk that are guaranteed obstacle-free on the safe lane.")]
    [SerializeField] private float exitBuffer = 3f;

    public float Length => length;
    public LaneMask Entry => entry;
    public LaneMask Exit => exit;
    public float EntryBuffer => entryBuffer;
    public float ExitBuffer => exitBuffer;

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
        if ((mask & LaneMask.Left) != 0) Gizmos.DrawWireCube(center + Vector3.left * laneOffset, size);
        if ((mask & LaneMask.Middle) != 0) Gizmos.DrawWireCube(center, size);
        if ((mask & LaneMask.Right) != 0) Gizmos.DrawWireCube(center + Vector3.right * laneOffset, size);
    }
}