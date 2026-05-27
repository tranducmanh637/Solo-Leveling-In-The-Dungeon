using UnityEngine;

public class MoveCloud : MonoBehaviour
{
    public float speed = 2f;

    // vị trí khi biến mất bên trái
    public float leftLimit = -8.63f;

    // vị trí xuất hiện lại bên phải
    public float rightSpawn = 8.63f;

    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        if (transform.position.x < leftLimit)
        {
            transform.position = new Vector3(
                rightSpawn,
                transform.position.y,
                transform.position.z
            );
        }
    }
}