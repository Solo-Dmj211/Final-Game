using UnityEngine;
using Unity.Cinemachine;

public class CameraFallOffset : MonoBehaviour
{
    [Header("References")]
    public CinemachineFollow cinemachineFollow;
    public Rigidbody2D playerRb;

    [Header("Fall Offset")]
    public float maxFallOffset = 3f;
    public float fallVelocityThreshold = -1f;
    public float terminalVelocity = -20f;
    public float offsetSpeed = 5f;
    public float resetSpeed = 2f;

    float baseYOffset;

    void Start()
    {
        baseYOffset = cinemachineFollow.FollowOffset.y;
    }

    void Update()
    {
        float vy = playerRb.linearVelocity.y;

        float fallRatio = Mathf.InverseLerp(fallVelocityThreshold, terminalVelocity, vy);
        float targetY   = baseYOffset - (fallRatio * maxFallOffset);

        float speed = (vy < fallVelocityThreshold) ? offsetSpeed : resetSpeed;

        Vector3 offset = cinemachineFollow.FollowOffset;
        offset.y = Mathf.Lerp(offset.y, targetY, Time.deltaTime * speed);
        cinemachineFollow.FollowOffset = offset;
    }
}