using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -8f);
    [SerializeField] private Vector3 lookOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private float positionSmoothing = 8f;
    [SerializeField] private float rotationSmoothing = 6f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + target.rotation * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, positionSmoothing * Time.deltaTime);

        Vector3 lookAt = target.position + target.rotation * lookOffset;
        Quaternion desiredRot = Quaternion.LookRotation(lookAt - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationSmoothing * Time.deltaTime);
    }
}
