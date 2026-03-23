using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class WeaponController : MonoBehaviour
{
    [System.Serializable]
    public class AttackConfig
    {
        [Tooltip("Prefab của đòn đánh (Đạn, Vết chém...)")]
        public GameObject attackPrefab;
        
        [Tooltip("Vị trí xuất hiện riêng của đòn này (Để trống sẽ tự lấy vị trí của vũ khí)")]
        public Transform customSpawnPoint; 
    }

    [Header("Cài đặt Chuỗi Đòn đánh")]
    [Tooltip("Danh sách các đòn đánh sẽ được bắn ra cùng lúc")]
    public List<AttackConfig> attackSequence = new List<AttackConfig>();

    [Space(10)]
    [Tooltip("Khoảng cách xoay mặc định (Dành cho Player)")]
    public float defaultOrbitRadius = 1.2f;

    [Header("Cài Đặt Nhịp Độ (Delay)")]
    public float attackDelay = 0.5f; 
    
    private float nextAttackTime = 0f; 
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    private CharacterSwitcher playerOwner;
    private EnemyController enemyOwner;
    private float currentOrbitRadius;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        
        playerOwner = GetComponentInParent<CharacterSwitcher>();
        enemyOwner = GetComponentInParent<EnemyController>();

        if (enemyOwner != null && enemyOwner.enemyData != null)
        {
            currentOrbitRadius = enemyOwner.enemyData.orbitRadius;
        }
        else
        {
            currentOrbitRadius = defaultOrbitRadius; 
        }
    }

    void Update()
    {
        RotateAndOrbit();

        if (playerOwner != null && Input.GetMouseButtonDown(1)) 
        {
            TryAttack();
        }
    }

    private void RotateAndOrbit()
    {
        Vector3 targetPos = Vector3.zero;

        if (playerOwner != null)
        {
            targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
        }
        else if (enemyOwner != null && CharacterSwitcher.currentPlayer != null)
        {
            targetPos = CharacterSwitcher.currentPlayer.transform.position; 
        }
        else return;

        targetPos.z = 0f; 

        Transform centerPoint = transform.parent;
        if (centerPoint == null) centerPoint = transform;

        Vector3 aimDirection = (targetPos - centerPoint.position).normalized;
        transform.position = centerPoint.position + aimDirection * currentOrbitRadius;

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Giữ đúng tỷ lệ gốc của vũ khí và lật theo chiều chuột
        Vector3 weaponScale = transform.localScale; 
        if (angle > 90 || angle < -90)
        {
            weaponScale.y = -Mathf.Abs(weaponScale.y); 
        }
        else
        {
            weaponScale.y = Mathf.Abs(weaponScale.y);  
        }
        transform.localScale = weaponScale; 
    }

    public void TryAttack()
    {
        if (Time.time >= nextAttackTime)
        {
            if (playerOwner != null && playerOwner.currentEquippedData != null)
            {
                float cost = playerOwner.currentEquippedData.manaCost;
                
                if (cost > 0 && !playerOwner.UseMana(cost))
                {
                    return; 
                }
            }

            PerformAttack(); 
            nextAttackTime = Time.time + attackDelay; 
        }
    }

    private void PerformAttack()
    {
        if (attackSequence == null || attackSequence.Count == 0) return;

        float finalDamage = 0f; 
        bool isPlayerWeapon = false; 

        if (playerOwner != null)
        {
            isPlayerWeapon = true; 
            if (playerOwner.myCharacterData != null) finalDamage += playerOwner.myCharacterData.strength;
            if (playerOwner.currentEquippedData != null) finalDamage += playerOwner.currentEquippedData.damage;
        }
        else if (enemyOwner != null)
        {
            isPlayerWeapon = false; 
            if (enemyOwner.enemyData != null) finalDamage += enemyOwner.enemyData.strength;
            if (enemyOwner.enemyData.equippedWeapon != null) finalDamage += enemyOwner.enemyData.equippedWeapon.damage;
        }

        if (anim != null)
        {
            anim.SetTrigger("Attack"); 
        }

        foreach (AttackConfig config in attackSequence)
        {
            if (config.attackPrefab == null) continue; 

            Vector3 spawnPos = config.customSpawnPoint != null ? config.customSpawnPoint.position : transform.position;
            Quaternion baseRotation = config.customSpawnPoint != null ? config.customSpawnPoint.rotation : transform.rotation;

            Projectile prefabScript = config.attackPrefab.GetComponent<Projectile>();
            
            List<BulletPattern> patternsToShoot = new List<BulletPattern>();
            patternsToShoot.Add(new BulletPattern()); 

            if (prefabScript != null && prefabScript.bulletPatterns != null && prefabScript.bulletPatterns.Count > 0)
            {
                patternsToShoot = prefabScript.bulletPatterns; 
            }

            foreach (BulletPattern pattern in patternsToShoot)
            {
                Quaternion spreadRotation = baseRotation * Quaternion.Euler(0, 0, pattern.spreadAngle);

                // Tính toán Tâm (Static Center) ngay lúc bắn
                Vector3 initialCenter = spawnPos; 
                
                if (pattern.centerOffsetDistance > 0)
                {
                    Vector3 shootDirection = spreadRotation * Vector3.right; 
                    initialCenter = spawnPos + (shootDirection * pattern.centerOffsetDistance);
                }

                GameObject bullet = Instantiate(config.attackPrefab, spawnPos, spreadRotation);
                Projectile projScript = bullet.GetComponent<Projectile>();
                
                if (projScript != null)
                {
                    projScript.Setup(finalDamage, isPlayerWeapon); 
                    
                    // Truyền lệnh bay vòng tròn (Hỗ trợ theo sát nhân vật)
                    Transform shooterTransform = transform.parent != null ? transform.parent : transform;
                    projScript.SetCircularMotion(pattern.spinDirection, initialCenter, pattern.centerFollowsShooter, shooterTransform);
                }
            }
        }
    }
}