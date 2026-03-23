using UnityEngine;

public class ToolbarManager : MonoBehaviour
{
    public static ToolbarManager Instance; // Mẫu Singleton để dễ gọi từ nơi khác

    [Tooltip("Kéo 5 ô ToolbarSlot vào đây theo thứ tự")]
    public ToolbarSlot[] hotkeySlots;

    [Header("Cài đặt Màu Khung (Frame)")]
    public Color activeColor = Color.green; // Đang cầm trên tay
    public Color inactiveColor = new Color(1f, 0.5f, 0f); // Màu cam (Có đồ nhưng cất trong túi)
    public Color emptyColor = Color.white;  // Ô trống

    [HideInInspector] public int currentSelectedIndex = -1;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Đánh số thứ tự cho các ô để chúng tự biết mình là ô số mấy
        for (int i = 0; i < hotkeySlots.Length; i++)
        {
            hotkeySlots[i].slotIndex = i;
        }
        
        // Mặc định chọn ô số 1 lúc mới vào game
        SelectSlot(0); 
    }

    void Update()
    {
        // Lắng nghe phím số 1-5
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectSlot(4);
    }

    // --- HÀM XỬ LÝ KHI BẤM CHỌN MỘT Ô ---
    public void SelectSlot(int index)
    {
        // YÊU CẦU 1: Bấm lại vào ô đang chọn -> Hủy chọn (Đổi xanh thành cam)
        if (currentSelectedIndex == index)
        {
            currentSelectedIndex = -1; 
        }
        else
        {
            currentSelectedIndex = index;
        }
        
        UpdateAllSlotsVisuals();
        EquipCurrentSlot();
    }
// --- HÀM 2: HỦY CHỌN TẤT CẢ (Dùng khi bấm vào túi đồ) ---
    public void DeselectAll()
    {
        currentSelectedIndex = -1;
        UpdateAllSlotsVisuals();
        EquipCurrentSlot();
    }
    // --- HÀM 5: TỰ CHỌN KHI KÉO ĐỒ VÀO ---
    public void AutoSelectAfterDrop(int droppedSlotIndex)
    {
        if (currentSelectedIndex == -1 || hotkeySlots[currentSelectedIndex].assignedItem == null || hotkeySlots[currentSelectedIndex].currentQuantity <= 0)
        {
            SelectSlot(droppedSlotIndex);
        }
        else
        {
            UpdateAllSlotsVisuals();
        }
    }

    // --- CẬP NHẬT MÀU SẮC DẤU CHẤM ---
    public void UpdateAllSlotsVisuals()
    {
        for (int i = 0; i < hotkeySlots.Length; i++)
        {
            // Kiểm tra xem ô này có đồ không
            bool hasItem = hotkeySlots[i].assignedItem != null && hotkeySlots[i].currentQuantity > 0;
            
            // Kiểm tra xem ô này có đang được chọn trên tay không
            bool isSelected = (i == currentSelectedIndex);
            
            // Gọi hàm cập nhật dấu chấm
            hotkeySlots[i].SetDotStatus(isSelected, hasItem);
        }
    }

    // --- RA LỆNH CHO NHÂN VẬT TRANG BỊ VŨ KHÍ ---
private void EquipCurrentSlot()
    {
        // TRỎ ĐÍCH DANH vào nhân vật đang được điều khiển hiện tại
        CharacterSwitcher player = CharacterSwitcher.currentPlayer; 
        
        if (player == null) return;

        // Nếu không có ô nào được chọn (-1) -> Dùng Nắm đấm
        if (currentSelectedIndex == -1)
        {
            player.EquipDefaultWeapon();
            return;
        }

        ToolbarSlot activeSlot = hotkeySlots[currentSelectedIndex];
        
        if (activeSlot.assignedItem != null && activeSlot.currentQuantity > 0)
        {
            if (activeSlot.assignedItem.isWeapon)
            {
                player.EquipWeapon(activeSlot.assignedItem);
            }
            else
            {
                player.EquipDefaultWeapon();
            }
        }
        else
        {
            player.EquipDefaultWeapon();
        }
    }
}