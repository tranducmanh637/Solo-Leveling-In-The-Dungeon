using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BulletPattern
{
    [Tooltip("Góc lệch ban đầu")]
    public float spreadAngle = 0f;
    
    [Tooltip("Khoảng cách từ nòng súng tới Tâm (Chính là Bán kính). Điền 0 để bay thẳng.")]
    public float centerOffsetDistance = 0f; 
    
    [Tooltip("1: Cùng chiều kim đồng hồ | -1: Ngược chiều kim đồng hồ")]
    public int spinDirection = 1; 

    [Tooltip("Tích vào nếu muốn Tâm của đường đạn tịnh tiến theo bước chân của Nhân vật")]
    public bool centerFollowsShooter = false;
}

public class Projectile : MonoBehaviour
{
    [Header("Cài đặt Đường Đạn")]
    public List<BulletPattern> bulletPatterns = new List<BulletPattern>() { new BulletPattern() };

    // --- CÁC BIẾN TOÁN HỌC LƯU TRỮ TÂM BAY ---
    private float myRadius = 0f;
    private int mySpinDir = 0;
    private Vector3 myFixedCenter;        // Tâm đứng yên ngoài môi trường
    private bool myFollowShooter = false; // Cờ kiểm tra xem có đi theo nhân vật không
    private Transform myShooterTransform; // Gốc nhân vật
    private Vector3 centerOffsetFromShooter; // Khoảng cách từ Tâm đến Nhân vật lúc vừa bắn
    private float currentOrbitAngle = 0f;

    [Header("Cài đặt Đòn đánh")]
    public float spawnDelay = 0f; 
    public float moveSpeed = 15f; 
    public bool destroyOnHit = true; 
    public float lifeTime = 2f; 

    private float myDamage;
    private bool isPlayerWeapon;
    private bool isReady = true; 

    private SpriteRenderer spriteRenderer;
    private Collider2D col2D;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col2D = GetComponent<Collider2D>();
    }

    public void Setup(float damage, bool isPlayerWeapon)
    {
        myDamage = damage;
        this.isPlayerWeapon = isPlayerWeapon;

        // Xử lý đếm ngược độ trễ nếu có
        if (spawnDelay > 0)
        {
            StartCoroutine(DelaySpawnRoutine());
        }
        else
        {
            Destroy(gameObject, lifeTime); 
        }
    }

    public void SetCircularMotion(int direction, Vector3 initialCenter, bool followShooter, Transform shooterTransform)
    {
        mySpinDir = direction;
        myFixedCenter = initialCenter;
        myFollowShooter = followShooter;
        myShooterTransform = shooterTransform;

        // Nếu Tâm cần di chuyển theo người, chốt lại khoảng cách lúc vừa bắn
        if (myFollowShooter && myShooterTransform != null)
        {
            centerOffsetFromShooter = initialCenter - myShooterTransform.position;
        }

        myRadius = Vector3.Distance(transform.position, initialCenter);

        // Chốt góc xuất phát để tính toán Lượng giác (Sin/Cos)
        if (myRadius > 0.01f)
        {
            Vector3 dir = (transform.position - initialCenter).normalized;
            currentOrbitAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }
    }

    private IEnumerator DelaySpawnRoutine()
    {
        isReady = false; // Bắt đầu tàng hình
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (col2D != null) col2D.enabled = false;

        yield return new WaitForSeconds(spawnDelay);

        isReady = true;  // Hiện nguyên hình
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (col2D != null) col2D.enabled = true;

        Destroy(gameObject, lifeTime); // Bắt đầu tính giờ tự hủy từ lúc hiện ra
    }

    void Update()
    {
        // Lưu ý: KHÔNG dùng "if (!isReady) return;" ở đây nữa
        // Để đạn luôn âm thầm tính toán vị trí bám theo nhân vật ngay cả khi đang tàng hình!

        if (myRadius > 0.01f && mySpinDir != 0)
        {
            // Tốc độ xoay
            float angularSpeed = (moveSpeed / myRadius) * Mathf.Rad2Deg;
            currentOrbitAngle += -angularSpeed * mySpinDir * Time.deltaTime;
            
            // Tính toán khoảng cách (offset) từ Tâm ra dựa trên Sin/Cos
            float radian = currentOrbitAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0) * myRadius;

            // Xác định Tâm hiện tại đang nằm ở đâu
            Vector3 currentCenterPos = myFixedCenter; 
            if (myFollowShooter && myShooterTransform != null)
            {
                currentCenterPos = myShooterTransform.position + centerOffsetFromShooter;
            }

            // Ép đạn xoay quanh Tâm
            transform.position = currentCenterPos + offset;
            transform.rotation = Quaternion.Euler(0, 0, currentOrbitAngle + (mySpinDir == 1 ? -90f : 90f));
        }
        else
        {
            // NẾU BÁN KÍNH = 0 THÌ BAY THẲNG (Có áp dụng tàng hình lúc đầu)
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Đạn chỉ gây sát thương nếu đã hiện hình xong
        if (!isReady) return; 

        if (isPlayerWeapon)
        {
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(myDamage); 
                if (destroyOnHit) Destroy(gameObject); 
            }
        }
        else
        {
            CharacterSwitcher player = collision.GetComponent<CharacterSwitcher>();
            if (player != null && player.gameObject.CompareTag("Player")) 
            {
                player.TakeDamage(myDamage); 
                if (destroyOnHit) Destroy(gameObject); 
            }
        }
        
        if (collision.CompareTag("Wall"))
        {
            if (destroyOnHit) Destroy(gameObject);
        }
    }
}