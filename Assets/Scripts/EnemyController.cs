using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Tự động thêm SpriteRenderer nếu chưa có
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyController : MonoBehaviour
{
    [Header("Dữ Liệu Quái Vật")]
    public EnemyData enemyData;

    [Header("Cài Đặt Vũ Khí")]
    public Transform weaponHolder; // Điểm cầm vũ khí của quái vật
    private WeaponController currentWeapon; // Vũ khí quái đang cầm

    [Header("Cài Đặt Animation")]
    public float frameRate = 0.15f; 

    // Các biến AI
    private int currentActionIndex = 0;
    private bool isChasing = false;
    private Coroutine patrolCoroutine;

    // Các biến ngầm phục vụ Animation
    private SpriteRenderer spriteRenderer;
    private float animationTimer;
    private int currentFrame;
    private Vector2 lastPosition;
    private Vector2 currentVelocity;
    private float currentHealth;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastPosition = transform.position; 

        // ĐÚC VŨ KHÍ CHO QUÁI VẬT KHI MỚI VÀO GAME
        if (enemyData != null && enemyData.equippedWeapon != null && enemyData.equippedWeapon.weaponPrefab != null)
        {
            GameObject wepObj = Instantiate(enemyData.equippedWeapon.weaponPrefab, weaponHolder);
            currentWeapon = wepObj.GetComponent<WeaponController>();
        }

        if (enemyData != null && enemyData.actionList.Count > 0)
        {
            patrolCoroutine = StartCoroutine(ExecutePatrol());
        }
        if (enemyData != null) currentHealth = enemyData.maxHealth;
    }

    void Update()
    {
        if (enemyData == null) return;

        currentVelocity = ((Vector2)transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        CheckPlayerDistance();
        
        if (isChasing)
        {
            ChasePlayer();
        }

        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        // Kiểm tra xem quái có đang di chuyển không (vận tốc lớn hơn một chút xíu)
        bool isMoving = currentVelocity.magnitude > 0.05f;

        Sprite[] framesToPlay = null;

        if (isMoving)
        {
            // Trạng thái: ĐANG ĐI
            // So sánh xem quái đi theo chiều ngang (X) nhiều hơn hay chiều dọc (Y) nhiều hơn
            if (Mathf.Abs(currentVelocity.x) > Mathf.Abs(currentVelocity.y))
            {
                // ĐI NGANG (Trái / Phải)
                framesToPlay = enemyData.walkRightFrames;
                
                // Nếu đi sang TRÁI (vận tốc X âm), lật ảnh lại
                spriteRenderer.flipX = (currentVelocity.x < 0); 
            }
            else
            {
                // ĐI DỌC (Lên / Xuống)
                if (currentVelocity.y > 0)
                {
                    framesToPlay = enemyData.walkBackFrames; // Đi Lên
                }
                else
                {
                    framesToPlay = enemyData.walkFrontFrames; // Đi Xuống
                }
                
                // Nhớ tắt lật ảnh khi đi dọc
                spriteRenderer.flipX = false;
            }
        }
        else
        {
            // Trạng thái: ĐỨNG IM (Idle hoặc Wait)
            framesToPlay = enemyData.idleFrames;
        }

        // Chạy vòng lặp đổi Frame hình ảnh
        if (framesToPlay != null && framesToPlay.Length > 0)
        {
            animationTimer += Time.deltaTime;
            if (animationTimer >= frameRate)
            {
                animationTimer -= frameRate; // Reset đồng hồ
                currentFrame = (currentFrame + 1) % framesToPlay.Length; // Chuyển sang ảnh tiếp theo
                spriteRenderer.sprite = framesToPlay[currentFrame];      // Gắn ảnh lên người
            }
        }
    }

    // =========================================================
    // PHẦN LOGIC AI GIỮ NGUYÊN NHƯ BÀI TRƯỚC
    // =========================================================

    private void CheckPlayerDistance()
    {
        if (CharacterSwitcher.currentPlayer == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, CharacterSwitcher.currentPlayer.transform.position);

        if (distanceToPlayer <= enemyData.detectionRange)
        {
            if (!isChasing)
            {
                isChasing = true;
                if (patrolCoroutine != null) StopCoroutine(patrolCoroutine);
            }
        }
        else
        {
            if (isChasing)
            {
                isChasing = false;
                if (enemyData.actionList.Count > 0)
                {
                    patrolCoroutine = StartCoroutine(ExecutePatrol());
                }
            }
        }
    }

    private void ChasePlayer()
    {
        Transform playerPos = CharacterSwitcher.currentPlayer.transform;
        float distanceToPlayer = Vector2.Distance(transform.position, playerPos.position);

        // KIỂM TRA TẦM ĐÁNH
        if (distanceToPlayer <= enemyData.attackRange)
        {
            // Đã đứng đủ gần -> Dừng lại và tung chiêu
            if (currentWeapon != null)
            {
                currentWeapon.TryAttack();
            }
        }
        else
        {
            // Chưa đủ gần -> Tiếp tục chạy lại gần Player
            transform.position = Vector2.MoveTowards(transform.position, playerPos.position, enemyData.speed * Time.deltaTime);
        }
    }

    private IEnumerator ExecutePatrol()
    {
        while (true) 
        {
            if (enemyData.actionList.Count == 0) yield break;

            EnemyAction currentAction = enemyData.actionList[currentActionIndex];

            if (currentAction.actionType == EnemyActionType.Wait)
            {
                yield return new WaitForSeconds(currentAction.duration);
            }
            else if (currentAction.actionType == EnemyActionType.Move)
            {
                float timer = 0f;
                while (timer < currentAction.duration)
                {
                    transform.position = Vector2.MoveTowards(transform.position, currentAction.targetPosition, enemyData.speed * Time.deltaTime);
                    timer += Time.deltaTime;
                    
                    if (Vector2.Distance(transform.position, currentAction.targetPosition) < 0.05f)
                    {
                        break;
                    }
                    yield return null; 
                }
            }
            currentActionIndex = (currentActionIndex + 1) % enemyData.actionList.Count;
        }
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        
        Debug.Log($"[QUÁI VẬT] {enemyData.enemyName} bị chém trúng! Mất {damageAmount} máu. (Còn lại: {currentHealth}/{enemyData.maxHealth})");

        // NẾU QUÁI VẬT HẾT MÁU
        if (currentHealth <= 0)
        {
            Debug.Log($"[QUÁI VẬT] {enemyData.enemyName} đã bị tiêu diệt!");
            
            DropLoot(); // Gọi hàm rớt Vàng, Mana, Máu và Đồ

            Destroy(gameObject); // Xóa xác quái vật
        }
    }

    // --- HÀM CHÍNH ĐỂ RỚT ĐỒ ---
    private void DropLoot()
    {
        if (enemyData == null) return;

        // 1. RỚT VÀNG (Chia nhỏ tổng số vàng ra nhiều cục)
        if (enemyData.goldPrefab != null && enemyData.goldTotalAmount > 0 && enemyData.goldVisualPieces > 0)
        {
            int valuePerPiece = Mathf.Max(1, enemyData.goldTotalAmount / enemyData.goldVisualPieces);
            int remainingValue = enemyData.goldTotalAmount;

            for (int i = 0; i < enemyData.goldVisualPieces; i++)
            {
                int val = (i == enemyData.goldVisualPieces - 1) ? remainingValue : valuePerPiece;
                remainingValue -= val;

                if (val > 0) SpawnPickup(enemyData.goldPrefab, PickupType.Gold, val);
            }
        }

        // 2. RỚT MANA
        if (enemyData.manaPrefab != null && enemyData.manaTotalAmount > 0 && enemyData.manaVisualPieces > 0)
        {
            int valuePerPiece = Mathf.Max(1, enemyData.manaTotalAmount / enemyData.manaVisualPieces);
            int remainingValue = enemyData.manaTotalAmount;

            for (int i = 0; i < enemyData.manaVisualPieces; i++)
            {
                int val = (i == enemyData.manaVisualPieces - 1) ? remainingValue : valuePerPiece;
                remainingValue -= val;

                if (val > 0) SpawnPickup(enemyData.manaPrefab, PickupType.Mana, val);
            }
        }

        // 3. RỚT MÁU (Logic tương tự Vàng và Mana)
        if (enemyData.healthPrefab != null && enemyData.healthTotalAmount > 0 && enemyData.healthVisualPieces > 0)
        {
            int valuePerPiece = Mathf.Max(1, enemyData.healthTotalAmount / enemyData.healthVisualPieces);
            int remainingValue = enemyData.healthTotalAmount;

            for (int i = 0; i < enemyData.healthVisualPieces; i++)
            {
                int val = (i == enemyData.healthVisualPieces - 1) ? remainingValue : valuePerPiece;
                remainingValue -= val;

                if (val > 0) SpawnPickup(enemyData.healthPrefab, PickupType.Health, val);
            }
        }

        // 4. RỚT DANH SÁCH VẬT PHẨM (Theo tỉ lệ)
        if (enemyData.droppedItemPrefab != null && enemyData.dropItems != null)
        {
            foreach (LootDrop loot in enemyData.dropItems)
            {
                if (loot.item != null)
                {
                    // Tung xúc xắc ngẫu nhiên từ 0.00 đến 100.00
                    float randomRoll = Random.Range(0f, 100f);

                    // Nếu số tung được NHỎ HƠN HOẶC BẰNG tỉ lệ rớt -> Cho rớt đồ!
                    if (randomRoll <= loot.dropChance)
                    {
                        // Lấy số lượng ngẫu nhiên dựa trên min/max
                        int dropAmount = Random.Range(loot.minAmount, loot.maxAmount + 1);

                        GameObject itemObj = Instantiate(enemyData.droppedItemPrefab, GetRandomDropPosition(), Quaternion.identity);
                        PickupItem script = itemObj.GetComponent<PickupItem>();
                        
                        if (script != null)
                        {
                            script.SetupItem(loot.item, dropAmount); 
                        }
                        
                        Debug.Log($"[MAY MẮN] Quái vật rớt ra {loot.item.itemName} x{dropAmount} (Tỉ lệ: {loot.dropChance}%)");
                    }
                }
            }
        }
    }

    // Hàm phụ: Lấy 1 tọa độ ngẫu nhiên xung quanh xác quái vật để đồ không bị đè lên nhau
    private Vector2 GetRandomDropPosition()
    {
        // Văng ngẫu nhiên trong khoảng -1 đến 1 trên cả trục X và Y
        return (Vector2)transform.position + new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
    }

    // Hàm phụ: Đúc Vàng/Mana/Máu ra bản đồ cho gọn code
    private void SpawnPickup(GameObject prefab, PickupType type, int amount)
    {
        GameObject obj = Instantiate(prefab, GetRandomDropPosition(), Quaternion.identity);
        PickupItem script = obj.GetComponent<PickupItem>();
        if (script != null)
        {
            script.Setup(type, amount);
        }
    }
}