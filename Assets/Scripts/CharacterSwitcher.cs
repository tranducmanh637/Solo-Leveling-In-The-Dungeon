using UnityEngine;
using UnityEngine.SceneManagement;
// Yêu cầu phải có Collider2D để có thể click chuột vào
[RequireComponent(typeof(Collider2D))]
public class CharacterSwitcher : MonoBehaviour
{
    public static string targetSpawnPointName = "";
    [Header("Cài Đặt Đổi Nhân Vật")]
    [Tooltip("Tích vào ô này cho nhân vật bạn muốn điều khiển ĐẦU TIÊN khi vào game")]
    public bool isPlayer = false; 
    
    [Tooltip("Khoảng cách tối đa để có thể click đổi nhân vật")]
    public float interactRange = 3f; 

    // Biến static lưu lại nhân vật đang được điều khiển trên toàn bản đồ
    public static CharacterSwitcher currentPlayer;

    private PlayerController movementScript;
    private Rigidbody2D rb;      // THÊM DÒNG NÀY
    private Animator animator;   // THÊM DÒNG NÀY

    [Header("Cài Đặt Vũ Khí")]
    [Tooltip("Kéo một GameObject trống nằm ở vị trí bàn tay nhân vật vào đây")]
    public Transform weaponHolder; 
    
    [Tooltip("Kéo ItemData của Nắm Đấm vào đây")]
    public ItemData defaultWeapon; 

    // Các biến ngầm để quản lý
    public ItemData currentEquippedData;   
    private GameObject currentWeaponObject;
    
    [Header("Dữ Liệu Nhân Vật")]
    public CharacterData myCharacterData; // Kéo thả file data của nhân vật (chứa strength) vào đây
    
    [Header("Chỉ số sinh tồn")]
    public float currentHealth; 
    public HealthBar healthBar;
    
    [Header("Hệ thống Mana")]
    private float maxMana; 
    public float currentMana;
    public ManaBarUI manaBar;

    void Awake()
    {
        // --- CƠ CHẾ BẢO TOÀN NHÂN VẬT KHI QUA MAP (SINGLETON) ---
        if (isPlayer)
        {
            if (currentPlayer == null)
            {
                currentPlayer = this;
                DontDestroyOnLoad(gameObject); // Lệnh bài miễn tử: Giữ nguyên vẹn toàn bộ máu, vũ khí, data!
            }
            else if (currentPlayer != this)
            {
                // Nếu quay lại Map cũ, Map cũ có sẵn 1 thằng Player mặc định -> Tiêu diệt thằng mặc định đó
                // Để nhường chỗ cho thằng Player (với đầy đủ đồ đạc) từ Map khác vừa chạy sang!
                Destroy(gameObject);
                return;
            }
        }
        movementScript = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();      // THÊM DÒNG NÀY
        animator = GetComponent<Animator>();   // THÊM DÒNG NÀY
    }

   void Start()
    {
        // 1. Nạp đầy máu và mana cho tất cả các nhân vật theo Data
        if (myCharacterData != null)
        {
            maxMana = myCharacterData.maxMana;
            currentMana = maxMana;
            currentHealth = myCharacterData.maxHealth; 
            
            // ĐÃ XÓA KHÚC CẬP NHẬT UI Ở ĐÂY ĐỂ TRÁNH NPC GIÀNH GIẬT
        }
        
        // 2. Phân loại lúc mới vào game
        if (isPlayer) 
        {
            // SetAsPlayer() sẽ tự động gọi lệnh vẽ UI chuẩn xác cho riêng người chơi
            SetAsPlayer(); 
            EquipDefaultWeapon(); 
        }
        else 
        {
            // Các NPC sẽ im lặng đứng im, không can thiệp vào UI
            SetAsNPC();
            UnequipWeapon(); // (Hoặc EquipDefaultWeapon() tùy vào bài trước bạn đang dùng lệnh nào)
        }
    }

    void Update()
    {
        RegenerateStats();
    }

    // Hàm này tự động chạy khi bạn CLICK CHUỘT TRÁI vào vùng Collider của nhân vật này
    void OnMouseDown()
    {
        if (isPlayer) return;

        if (currentPlayer != null)
        {
            float distance = Vector2.Distance(transform.position, currentPlayer.transform.position);
            if (distance <= interactRange)
            {
                ItemData weaponToTransfer = currentPlayer.currentEquippedData;

                
                currentPlayer.UnequipWeapon();

                // 2. BẬT vũ khí cho nhân vật mới
                if (weaponToTransfer != null)
                {
                    this.EquipWeapon(weaponToTransfer);
                }
                else
                {
                    this.EquipDefaultWeapon(); // Đề phòng trường hợp lỗi tay không
                }

                currentPlayer.SetAsNPC();
                this.SetAsPlayer();
            }
        }
    }

public void SetAsPlayer()
    {
        isPlayer = true;
        currentPlayer = this; 

        // ĐÃ XÓA DÒNG TẮT BẬT MOVEMENT SCRIPT Ở ĐÂY
        gameObject.tag = "Player"; 
        
        // --- RÃ ĐÔNG NHÂN VẬT ---
        if (animator != null) animator.enabled = true; 
        if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic; 
        
        // --- CẬP NHẬT LẠI TOÀN BỘ GIAO DIỆN ---
        if (healthBar != null && myCharacterData != null) 
        {
            healthBar.UpdateHealth(currentHealth, myCharacterData.maxHealth);
        }
        if (manaBar != null)
        {
            manaBar.SetMaxMana(maxMana);      
            manaBar.UpdateMana(currentMana);  
        }

        Debug.Log("Đã đổi sang nhân vật: " + gameObject.name);
    }

   public void SetAsNPC()
    {
        isPlayer = false;
        
        // ĐÃ XÓA DÒNG TẮT MOVEMENT SCRIPT ĐỂ ANIMATION IDLE ĐƯỢC PHÉP CHẠY
        gameObject.tag = "Untagged"; 

        // --- ĐÓNG BĂNG VẬT LÝ ---
        if (animator != null) animator.enabled = true; // Vẫn cho phép Animator chạy nếu có dùng
        if (rb != null) 
        {
            rb.linearVelocity = Vector2.zero; 
            rb.bodyType = RigidbodyType2D.Kinematic;   
        }
    }
    
    // ==========================================
    // CỤM HÀM TRANG BỊ VŨ KHÍ (ĐÃ ĐƯỢC CẬP NHẬT)
    // ==========================================

    public void EquipWeapon(ItemData weaponData)
    {
        if (weaponData == null) return;

        // Nếu nhân vật đang cầm ĐÚNG cây vũ khí này rồi thì không cần tạo lại nữa (đỡ lag)
        if (currentEquippedData == weaponData) return;

        // Xóa hình ảnh vũ khí cũ trên tay đi
        UnequipWeapon(); 

        // Nếu vũ khí mới có hình ảnh (Prefab), sinh nó ra tại vị trí bàn tay (weaponHolder)
        if (weaponData.weaponPrefab != null && weaponHolder != null)
        {
            currentWeaponObject = Instantiate(weaponData.weaponPrefab, weaponHolder);
            
            // Ép vũ khí nằm đúng vị trí 0,0,0 của bàn tay (Mẹo nhỏ giúp tránh lỗi vũ khí bị lệch)
            currentWeaponObject.transform.localPosition = Vector3.zero; 
            currentWeaponObject.transform.localRotation = Quaternion.identity;
            
            currentWeaponObject.name = weaponData.weaponPrefab.name; 
        }
        
        // Ghi nhớ dữ liệu món đồ đang cầm
        currentEquippedData = weaponData; 
        Debug.Log(">> [NHÂN VẬT] Đã cầm lên tay: " + currentEquippedData.name);
    }

    public void UnequipWeapon()
    {
        // Xóa GameObject vũ khí đang gắn trên tay
        if (currentWeaponObject != null)
        {
            Destroy(currentWeaponObject);
        }
        currentEquippedData = null; 
    }

    // Hàm gọi nhanh để chuyển về tay không (Nắm đấm)
    public void EquipDefaultWeapon()
    {
        if (defaultWeapon != null)
        {
            EquipWeapon(defaultWeapon); // Gọi hàm EquipWeapon để nó sinh ra prefab Nắm đấm
        }
        else
        {
            UnequipWeapon(); 
            Debug.LogWarning(">> [NHÂN VẬT] Bạn chưa cài Default Weapon (Nắm Đấm) cho " + gameObject.name);
        }
    }

    // ==========================================
    // CỤM HÀM SINH TỒN (GIỮ NGUYÊN)
    // ==========================================

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log($"[NHÂN VẬT] {gameObject.name} dính đòn! Mất {damageAmount} máu. (Còn lại: {currentHealth})");

        if (currentHealth <= 0)
        {
            Debug.Log($"[NHÂN VẬT] {gameObject.name} đã GỤC NGÃ!");
        }
        if (healthBar != null && myCharacterData != null)
        {
            healthBar.UpdateHealth(currentHealth, myCharacterData.maxHealth);
        }
    }

// ==========================================
    // CỤM HÀM SINH TỒN (ĐÃ CẬP NHẬT CHỐNG LỖI UI)
    // ==========================================
    
    private void RegenerateStats()
    {
        if (myCharacterData == null) return;

        // --- 1. HỒI MÁU ---
        if (currentHealth < myCharacterData.maxHealth && myCharacterData.hpRegen > 0)
        {
            currentHealth += myCharacterData.hpRegen * Time.deltaTime;
            if (currentHealth > myCharacterData.maxHealth) currentHealth = myCharacterData.maxHealth;
            
            // CHỈ CẬP NHẬT UI NẾU LÀ PLAYER
            if (isPlayer && healthBar != null) healthBar.UpdateHealth(currentHealth, myCharacterData.maxHealth); 
        }

        // --- 2. HỒI MANA ---
        if (currentMana < maxMana && myCharacterData.manaRegen > 0)
        {
            currentMana += myCharacterData.manaRegen * Time.deltaTime;
            if (currentMana > maxMana) currentMana = maxMana;
            
            // CHỈ CẬP NHẬT UI NẾU LÀ PLAYER
            if (isPlayer && manaBar != null) manaBar.UpdateMana(currentMana);
        }
    }

    public void RestoreMana(float amount)
    {
        currentMana += amount;
        if (currentMana > maxMana) currentMana = maxMana;
        
        // CHỈ CẬP NHẬT UI NẾU LÀ PLAYER
        if (isPlayer && manaBar != null) manaBar.UpdateMana(currentMana);
        
        Debug.Log($"[MANA] Đã hồi phục. Hiện tại: {currentMana}/{maxMana}");
    }

    public void RestoreHealth(float amount)
    {
        float maxHp = myCharacterData != null ? myCharacterData.maxHealth : 100f;
        currentHealth += amount;
        if (currentHealth > maxHp) currentHealth = maxHp;
        
        // CHỈ CẬP NHẬT UI NẾU LÀ PLAYER
        if (isPlayer && healthBar != null) healthBar.UpdateHealth(currentHealth, maxHp);
        
        Debug.Log($"[MÁU] Đã uống bình hồi {amount} Máu. Hiện tại: {currentHealth}/{maxHp}");
    }

    public bool UseMana(float amount)
    {
        if (amount <= 0) return true; 

        if (currentMana >= amount)
        {
            currentMana -= amount;
            
            // CHỈ CẬP NHẬT UI NẾU LÀ PLAYER
            if (isPlayer && manaBar != null) manaBar.UpdateMana(currentMana); 
            
            return true; 
        }
        
        Debug.Log($"[LỖI MANA] Vũ khí đòi {amount} Mana, nhưng chỉ còn {currentMana}!");
        return false; 
    }
    // Bật bộ lắng nghe sự kiện khi Load Map
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Tắt bộ lắng nghe khi nhân vật bị xóa
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Hàm này tự động chạy ngay khoảnh khắc Map 2 vừa load xong
    // Hàm này tự động chạy ngay khoảnh khắc Map mới vừa load xong
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Chỉ áp dụng cho nhân vật đang được điều khiển
        if (isPlayer) 
        {
            // ==========================================
            // 1. DỊCH CHUYỂN ĐẾN ĐÚNG CỬA HANG/ĐIỂM HỒI SINH
            // ==========================================
            if (!string.IsNullOrEmpty(targetSpawnPointName))
            {
                GameObject spawnPoint = GameObject.Find(targetSpawnPointName);
                if (spawnPoint != null)
                {
                    transform.position = spawnPoint.transform.position;
                }
            }

            // ==========================================
            // 2. KẾT NỐI LẠI VỚI GIAO DIỆN Ở MAP MỚI
            // ==========================================
            // Tự động tìm các script UI trên màn hình Map 2 và gán lại vào biến
            healthBar = Object.FindAnyObjectByType<HealthBar>();
            manaBar = Object.FindAnyObjectByType<ManaBarUI>();

            // ==========================================
            // 3. ĐẨY DỮ LIỆU HIỆN TẠI LÊN GIAO DIỆN MỚI
            // ==========================================
            if (healthBar != null && myCharacterData != null)
            {
                healthBar.UpdateHealth(currentHealth, myCharacterData.maxHealth);
            }
            
            if (manaBar != null)
            {
                manaBar.SetMaxMana(maxMana);
                manaBar.UpdateMana(currentMana);
            }

            Debug.Log($"[HỆ THỐNG] Đã cập nhật lại UI cho {gameObject.name} tại {scene.name}");
        }
    }
}