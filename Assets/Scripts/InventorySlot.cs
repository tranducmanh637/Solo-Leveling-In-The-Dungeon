using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public int slotIndex;
    [Header("Giao diện Ô")]
    // --- THÊM BIẾN NÀY ĐỂ LIÊN KẾT VỚI ẢNH NỀN MÀU MỚI TẠO ---
    public Image backgroundImage; // <-- BIẾN MỚI CHO NỀN MÀU
    public Image icon;
    private static GameObject ghostIcon;
    [Header("Hiển thị số lượng")]
    public TextMeshProUGUI quantityText; // Kéo Text số lượng vào đây

    public ItemData currentItem;

    // --- ĐÃ CẬP NHẬT: Thêm biến quantity vào hàm ---
    public void AddItem(ItemData newItem, int quantity)
    {
        currentItem = newItem;
        icon.sprite = newItem.itemIcon;
        icon.enabled = true;

        // Xử lý hiển thị chữ số lượng (Giữ nguyên)
        if (quantityText != null)
        {
            if (quantity >= 1)
            {
                quantityText.text = quantity.ToString();
                quantityText.enabled = true;
            }
            else
            {
                quantityText.enabled = false;
            }
        }

        // --- HÀM MỚI: ĐỔI MÀU NỀN THEO ĐỘ HIẾM CỦA VẬT PHẨM VỪA ĐƯỢC THÊM ---
        if (backgroundImage != null && InventoryUI.Instance != null && InventoryUI.Instance.rarityBackgrounds != null)
        {
            // Ép kiểu enum Độ hiếm thành số nguyên để lấy ảnh trong mảng (ví dụ: Common = 0, Legendary = 4)
            int rarityIndex = (int)newItem.rarity;
            
            // Kiểm tra an toàn: Nếu ảnh nền có tồn tại trong danh sách
            if (rarityIndex < InventoryUI.Instance.rarityBackgrounds.Length && InventoryUI.Instance.rarityBackgrounds[rarityIndex] != null)
            {
                backgroundImage.sprite = InventoryUI.Instance.rarityBackgrounds[rarityIndex]; // Gắn ảnh nền màu vào
                backgroundImage.enabled = true; // Hiện ảnh nền màu
            }
            else
            {
                // Nếu không tìm thấy ảnh độ hiếm phù hợp, ẩn nền màu đi
                backgroundImage.enabled = false;
            }
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        icon.sprite = null;
        icon.enabled = false;
        
        // Ẩn số lượng đi (Giữ nguyên)
        if (quantityText != null)
        {
            quantityText.enabled = false;
        }

        // --- ẨN ẢNH NỀN MÀU ĐI KHI Ô TRỐNG ---
        if (backgroundImage != null)
        {
            backgroundImage.enabled = false; 
        }
    }

    public void OnSlotClicked()
    {
        if (currentItem != null)
        {
            InventoryManager.Instance.UseItem(currentItem);

            // YÊU CẦU 2: Ấn vào túi đồ -> Toolbar tắt hết viền xanh thành cam -> Chuyển về nắm đấm
            if (ToolbarManager.Instance != null)
            {
                ToolbarManager.Instance.DeselectAll();
            }
        }
    }
    // --- 1. KHI BẮT ĐẦU KÉO ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Nếu ô này không có đồ thì không cho kéo
        if (icon.sprite == null || !icon.enabled) return;

        // Tạo ra một hình ảnh "bóng ma" để bay theo chuột
        ghostIcon = new GameObject("GhostIcon");
        ghostIcon.transform.SetParent(GetComponentInParent<Canvas>().transform, false);
        ghostIcon.transform.SetAsLastSibling(); // Ép nó hiển thị đè lên trên cùng mọi thứ

        Image img = ghostIcon.AddComponent<Image>();
        img.sprite = icon.sprite;
        img.rectTransform.sizeDelta = icon.rectTransform.sizeDelta;
        
        // CỰC KỲ QUAN TRỌNG: Tắt chặn tia Raycast để lúc thả chuột, ô bên dưới mới nhận được sự kiện
        img.raycastTarget = false; 

        // Làm mờ ảnh ở ô gốc đi một chút cho đẹp
        icon.color = new Color(1, 1, 1, 0.5f);
    }

    // --- 2. KHI ĐANG KÉO CHUỘT ---
    public void OnDrag(PointerEventData eventData)
    {
        if (ghostIcon != null)
        {
            // Ép cái bóng ma bay theo tọa độ chuột
            ghostIcon.transform.position = Input.mousePosition;
        }
    }

    // --- 3. KHI BUÔNG CHUỘT RA ---
    public void OnEndDrag(PointerEventData eventData)
    {
        // Xóa bóng ma đi
        if (ghostIcon != null) Destroy(ghostIcon);
        
        // Phục hồi lại độ sáng của ảnh gốc
        if (icon != null) icon.color = new Color(1, 1, 1, 1f);
    }

    // --- 4. KHI BỊ MỘT Ô KHÁC THẢ VÀO NGƯỜI ---
    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject == null) return;

        // 1. TỪ TÚI ĐỒ -> TÚI ĐỒ (Code cũ giữ nguyên)
        InventorySlot sourceSlot = draggedObject.GetComponent<InventorySlot>();
        if (sourceSlot != null && sourceSlot != this)
        {
            InventoryManager.Instance.ReorderItem(sourceSlot.slotIndex, this.slotIndex);
            return; // Đã thêm chữ return ở đây
        }

        // --- THÊM MỚI ---
        // 2. TỪ TOOLBAR -> TÚI ĐỒ 
        ToolbarSlot tbSlot = draggedObject.GetComponent<ToolbarSlot>();
        if (tbSlot != null && tbSlot.assignedItem != null)
        {
            // NẾU THẢ VÀO MỘT Ô TÚI ĐÃ CÓ ĐỒ -> ĐỔI CHỖ
            if (currentItem != null)
            {
                ItemData oldInvItem = currentItem;
                int oldInvQty = InventoryManager.Instance.items[slotIndex].quantity;

                // Cập nhật Data Túi đồ
                InventoryManager.Instance.items[slotIndex].item = tbSlot.assignedItem;
                InventoryManager.Instance.items[slotIndex].quantity = tbSlot.currentQuantity;

                // Toolbar nhận lại đồ của Túi
                tbSlot.assignedItem = oldInvItem;
                tbSlot.currentQuantity = oldInvQty;
                tbSlot.icon.sprite = oldInvItem.itemIcon;
                tbSlot.UpdateQuantityUI();
                tbSlot.SetDotStatus(ToolbarManager.Instance.currentSelectedIndex == tbSlot.slotIndex, true);
                
                InventoryUI.Instance.UpdateUI();
            }
            else
            {
                // NẾU THẢ VÀO Ô TÚI TRỐNG -> Nhét hẳn vào túi, dọn sạch Toolbar
                // (Bạn HÃY ĐỔI chữ "AddItem" bên dưới thành đúng tên hàm nhặt đồ trong InventoryManager của bạn)
                InventoryManager.Instance.AddItem(tbSlot.assignedItem, tbSlot.currentQuantity); 
                
                tbSlot.ClearSlot();
            }

            // Load lại vũ khí trên tay
            ToolbarManager.Instance.SelectSlot(ToolbarManager.Instance.currentSelectedIndex);
        }
    }
    // KHI CHUỘT LƯỚT VÀO Ô -> BẬT THÔNG TIN
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null)
        {
            ItemTooltip.Instance.ShowTooltip(currentItem);
        }
    }

    // KHI CHUỘT RỜI KHỎI Ô -> TẮT THÔNG TIN
    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltip.Instance.HideTooltip();
    }
}