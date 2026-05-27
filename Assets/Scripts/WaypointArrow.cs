using UnityEngine;

public class WaypointArrow : MonoBehaviour
{
    public Transform player;
    public Transform target;

    public float distanceFromPlayer = 1.5f;
    public float rotateOffset = -90f;

    void Update()
    {
        if (player == null || target == null) return;

        Vector3 direction = target.position - player.position;
        direction.z = 0;

        transform.position = player.position + direction.normalized * distanceFromPlayer;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + rotateOffset);
    }
}