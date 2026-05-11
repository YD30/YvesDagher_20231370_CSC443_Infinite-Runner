using UnityEngine;

[ExecuteAlways]
public class WorldCurve : MonoBehaviour
{
    [Tooltip("Vertical bend strength. Quadratic in distance, so values are tiny. 0.0008 = visible 'falling horizon' at z ~ 60.")]
    [SerializeField] private float curveY = 0.0008f;

    [Tooltip("Optional lateral sway amplitude. Use a small value (e.g. 0.5) for gentle level curves.")]
    [SerializeField] private float curveX = 0f;

    [Tooltip("Frequency of the lateral sin sway in radians per world unit.")]
    [SerializeField] private float curveFreq = 0.05f;

    private static readonly int CurveYId    = Shader.PropertyToID("_CurveY");
    private static readonly int CurveXId    = Shader.PropertyToID("_CurveX");
    private static readonly int CurveFreqId = Shader.PropertyToID("_CurveFreq");

    void OnValidate() => Apply();
    void OnEnable()   => Apply();
    void Update()     => Apply();

    void OnDisable()
    {
        Shader.SetGlobalFloat(CurveYId, 0f);
        Shader.SetGlobalFloat(CurveXId, 0f);
        Shader.SetGlobalFloat(CurveFreqId, 0f);
    }

    private void Apply()
    {
        Shader.SetGlobalFloat(CurveYId, curveY);
        Shader.SetGlobalFloat(CurveXId, curveX);
        Shader.SetGlobalFloat(CurveFreqId, curveFreq);
    }
}
