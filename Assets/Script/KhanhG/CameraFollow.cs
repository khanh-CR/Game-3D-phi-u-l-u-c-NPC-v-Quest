using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Smooth")]
    public float smoothTime = 0.08f;

    private Vector3 offset;
    private Vector3 velocity;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Chưa gắn Target cho Camera!");
            enabled = false;
            return;
        }

        // TỰ lấy khoảng cách hiện tại giữa Camera và Player
        offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );
    }
}