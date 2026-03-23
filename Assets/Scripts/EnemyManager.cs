using UnityEngine;
using System.Collections.Generic;

// 1. CẤU TRÚC LƯU TRỮ THÔNG TIN SINH QUÁI
[System.Serializable]
public class EnemySpawnConfig
{
    [Tooltip("Kéo Prefab của quái vật vào đây (VD: Slime, Goblin...)")]
    public GameObject enemyPrefab;

    [Tooltip("Số lượng quái vật muốn sinh ra")]
    public int spawnCount = 5;

    [Header("Khu vực sinh sản (Tọa độ)")]
    [Tooltip("Tọa độ X nhỏ nhất (Cạnh Trái)")]
    public float minX;
    [Tooltip("Tọa độ X lớn nhất (Cạnh Phải)")]
    public float maxX;
    
    [Tooltip("Tọa độ Y nhỏ nhất (Cạnh Dưới)")]
    public float minY;
    [Tooltip("Tọa độ Y lớn nhất (Cạnh Trên)")]
    public float maxY;
}

// 2. NGƯỜI QUẢN LÝ QUÁI VẬT
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("Danh sách Cấu hình Sinh Quái")]
    public List<EnemySpawnConfig> spawnList = new List<EnemySpawnConfig>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Tự động rải quái vật ngay khi bắt đầu game
        SpawnAllEnemies();
    }

    public void SpawnAllEnemies()
    {
        foreach (EnemySpawnConfig config in spawnList)
        {
            if (config.enemyPrefab == null)
            {
                Debug.LogWarning("Có một cấu hình sinh quái đang bị bỏ trống Prefab!");
                continue;
            }

            // Vòng lặp sinh đúng số lượng yêu cầu
            for (int i = 0; i < config.spawnCount; i++)
            {
                // Tung xúc xắc ngẫu nhiên tìm tọa độ X và Y trong vùng giới hạn
                float randomX = Random.Range(config.minX, config.maxX);
                float randomY = Random.Range(config.minY, config.maxY);
                Vector2 randomPosition = new Vector2(randomX, randomY);

                // Đúc quái vật ra bản đồ
                GameObject enemy = Instantiate(config.enemyPrefab, randomPosition, Quaternion.identity);

                // Tùy chọn: Gom tất cả quái vật vào làm con của EnemyManager cho mục Hierarchy gọn gàng
                enemy.transform.SetParent(this.transform);
            }
        }
        
        Debug.Log(">> [ENEMY MANAGER] Đã hoàn tất rải quái vật lên bản đồ!");
    }

    // ========================================================
    // TÍNH NĂNG MỞ RỘNG: VẼ KHUNG NHÌN TRƯỚC TRONG SCENE UNITY
    // ========================================================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // Màu đỏ hơi trong suốt
        
        foreach (var config in spawnList)
        {
            // Tính toán tâm và kích thước của hình chữ nhật
            float centerX = (config.maxX + config.minX) / 2f;
            float centerY = (config.maxY + config.minY) / 2f;
            float sizeX = config.maxX - config.minX;
            float sizeY = config.maxY - config.minY;

            // Vẽ một khung hình chữ nhật đỏ để bạn dễ hình dung vùng sinh quái
            Gizmos.DrawWireCube(new Vector3(centerX, centerY, 0), new Vector3(sizeX, sizeY, 0));
        }
    }
}