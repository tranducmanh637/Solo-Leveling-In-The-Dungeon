using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Cài đặt Chuyển Map")]
    [Tooltip("Tên của Map thứ 2 (VD: Map_02)")]
    public string sceneToLoad;

    [Tooltip("Tên của GameObject làm điểm xuất hiện ở Map 2 (VD: SpawnPoint_KhuRung)")]
    public string destinationSpawnPointName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. Lưu cái tên điểm đến vào bộ nhớ dùng chung của nhân vật
            CharacterSwitcher.targetSpawnPointName = destinationSpawnPointName;
            
            // 2. Chuyển Map
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}