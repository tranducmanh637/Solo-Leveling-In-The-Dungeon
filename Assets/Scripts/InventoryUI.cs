using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Gắn các thành phần UI vào đây")]
    public GameObject inventoryPanel; 
    private bool isInventoryOpen = false;
    public Transform slotContainer;  
    public GameObject slotPrefab;     

    private InventorySlot[] slots;   
    public static InventoryUI Instance;
    [Header("Ảnh Nền Độ Hiếm (Kéo theo thứ tự Common->Mythic)")]
    public Sprite[] rarityBackgrounds;
    private void Awake()
    {
        Instance = this;
    }
void Start()
    {
        int max = InventoryManager.Instance.maxSlots;
        slots = new InventorySlot[max];
        for (int i = 0; i < max; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, slotContainer);
            slots[i] = newSlot.GetComponent<InventorySlot>();
            slots[i].ClearSlot();
        }

        InventoryManager.Instance.onItemChangedCallback += UpdateUI;
        UpdateUI(); 
        inventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.B))
        {
            ToggleInventory();
        }
    }

  public void UpdateUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            // BẮT BUỘC THÊM DÒNG NÀY: Dạy cho từng ô biết số thứ tự của nó
            slots[i].slotIndex = i; 

            if (i < InventoryManager.Instance.items.Count)
            {
                slots[i].AddItem(InventoryManager.Instance.items[i].item, InventoryManager.Instance.items[i].quantity);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }
    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen; // Đảo ngược trạng thái
        
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isInventoryOpen); // Hiện hoặc Ẩn UI
        }

        // Xử lý ngưng đọng thời gian
        if (isInventoryOpen)
        {
            Time.timeScale = 0f; // PAUSE GAME: Mọi vật lý, di chuyển, animation sẽ dừng lại
        }
        else
        {
            Time.timeScale = 1f; // TIẾP TỤC GAME: Thời gian trôi bình thường
        }
    }

    // --- HÀM DÀNH RIÊNG CHO NÚT "X" CLICK CHUỘT ---
    public void CloseInventory()
    {
        // Nếu túi đang mở thì mới chạy lệnh tắt
        if (isInventoryOpen)
        {
            ToggleInventory(); 
        }
    }
}