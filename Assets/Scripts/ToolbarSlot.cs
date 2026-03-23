using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using UnityEngine.EventSystems;

public class ToolbarSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    [HideInInspector] public int slotIndex;

    [Header("Giao diện Ô")]
    public Image icon;
    public Sprite defaultPlusSprite;
    public TextMeshProUGUI quantityText; 
    
    [Header("Chỉ báo Trạng thái (Dấu chấm)")]
    [Tooltip("Kéo ảnh Dấu chấm vào đây")]
    public Image statusDot; // <-- BIẾN MỚI CHO DẤU CHẤM

    [HideInInspector] public ItemData assignedItem;
    [HideInInspector] public int currentQuantity = 0; 
    private static GameObject ghostIcon;

    void Start()
    {
        ClearSlot();
    }

    public void ClearSlot()
    {
        assignedItem = null;
        currentQuantity = 0;
        icon.sprite = defaultPlusSprite;
        icon.color = new Color(1, 1, 1, 0.5f);
        UpdateQuantityUI();

        if (ToolbarManager.Instance != null) ToolbarManager.Instance.UpdateAllSlotsVisuals();
    }

    public void UpdateQuantityUI()
    {
        if (quantityText != null)
        {
            if (currentQuantity > 1)
            {
                quantityText.text = currentQuantity.ToString();
                quantityText.enabled = true;
            }
            else
            {
                quantityText.enabled = false;
            }
        }
    }

// ==========================================
    // CỤM HÀM KÉO THẢ (DRAG) MỚI CHO TOOLBAR
    // ==========================================
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (assignedItem == null) return; // Không có đồ thì không cho kéo

        ghostIcon = new GameObject("GhostIcon");
        ghostIcon.transform.SetParent(GetComponentInParent<Canvas>().transform, false);
        ghostIcon.transform.SetAsLastSibling(); 

        Image img = ghostIcon.AddComponent<Image>();
        img.sprite = icon.sprite;
        img.rectTransform.sizeDelta = icon.rectTransform.sizeDelta;
        img.raycastTarget = false; // Tắt chặn tia để thả được

        icon.color = new Color(1, 1, 1, 0.5f); // Làm mờ ô đang kéo
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghostIcon != null) ghostIcon.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ghostIcon != null) Destroy(ghostIcon);
        if (assignedItem != null) icon.color = Color.white; // Phục hồi độ sáng
    }

    // ==========================================
    // HÀM NHẬN ĐỒ (ĐÃ CẬP NHẬT LOGIC ĐỔI CHỖ)
    // ==========================================
    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject == null) return;

        // --- TRƯỜNG HỢP 1: TỪ TÚI ĐỒ -> THẢ VÀO TOOLBAR ---
        InventorySlot invSlot = draggedObject.GetComponent<InventorySlot>();
        if (invSlot != null && invSlot.currentItem != null)
        {
            ItemData draggedItem = invSlot.currentItem;
            int draggedQty = InventoryManager.Instance.items[invSlot.slotIndex].quantity;

            if (assignedItem == null)
            {
                // 1A. Toolbar Trống: Hút đồ như bình thường
                int amountToMove = draggedItem.isWeapon ? 1 : draggedQty;
                assignedItem = draggedItem;
                currentQuantity += amountToMove;
                
                icon.sprite = assignedItem.itemIcon;
                icon.color = Color.white;
                UpdateQuantityUI();
                SetDotStatus(ToolbarManager.Instance.currentSelectedIndex == slotIndex, true);

                InventoryManager.Instance.RemoveItemAtIndex(invSlot.slotIndex, amountToMove);
            }
            else
            {
                // 1B. Toolbar Đã Có Đồ -> ĐỔI CHỖ (Swapping)
                ItemData oldTbItem = assignedItem;
                int oldTbQty = currentQuantity;

                // Toolbar nhận đồ từ Túi
                assignedItem = draggedItem;
                currentQuantity = draggedQty; 
                icon.sprite = assignedItem.itemIcon;
                icon.color = Color.white;
                UpdateQuantityUI();
                SetDotStatus(ToolbarManager.Instance.currentSelectedIndex == slotIndex, true);

                // Túi nhận lại đồ cũ từ Toolbar (Đè trực tiếp vào Data của túi)
                InventoryManager.Instance.items[invSlot.slotIndex].item = oldTbItem;
                InventoryManager.Instance.items[invSlot.slotIndex].quantity = oldTbQty;
                InventoryUI.Instance.UpdateUI();
            }

            ToolbarManager.Instance.AutoSelectAfterDrop(this.slotIndex);
            return; // Xong việc thì thoát
        }

        // --- TRƯỜNG HỢP 2: TỪ TOOLBAR -> THẢ VÀO TOOLBAR (Đổi chỗ 2 ô) ---
        ToolbarSlot sourceTbSlot = draggedObject.GetComponent<ToolbarSlot>();
        if (sourceTbSlot != null && sourceTbSlot != this && sourceTbSlot.assignedItem != null)
        {
            // Lưu giữ liệu của Ô bị thả đè (Đích)
            ItemData tempItem = assignedItem;
            int tempQty = currentQuantity;

            // Ô Đích nhận đồ của Ô Nguồn
            assignedItem = sourceTbSlot.assignedItem;
            currentQuantity = sourceTbSlot.currentQuantity;
            icon.sprite = assignedItem.itemIcon;
            icon.color = Color.white;
            UpdateQuantityUI();
            SetDotStatus(ToolbarManager.Instance.currentSelectedIndex == this.slotIndex, true);

            // Ô Nguồn nhận đồ của Ô Đích
            if (tempItem != null) 
            {
                sourceTbSlot.assignedItem = tempItem;
                sourceTbSlot.currentQuantity = tempQty;
                sourceTbSlot.icon.sprite = tempItem.itemIcon;
                sourceTbSlot.icon.color = Color.white;
                sourceTbSlot.UpdateQuantityUI();
                sourceTbSlot.SetDotStatus(ToolbarManager.Instance.currentSelectedIndex == sourceTbSlot.slotIndex, true);
            }
            else 
            {
                sourceTbSlot.ClearSlot(); // Nếu Đích trống thì Nguồn bị dọn sạch
            }

            // Gọi lệnh trang bị lại vũ khí (Đề phòng ô đang cầm trên tay bị chuyển đi chỗ khác)
            ToolbarManager.Instance.SelectSlot(ToolbarManager.Instance.currentSelectedIndex);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ToolbarManager.Instance.SelectSlot(this.slotIndex);
    }

    // --- HÀM MỚI: CẬP NHẬT DẤU CHẤM ---
    public void SetDotStatus(bool isSelected, bool hasItem)
    {
        if (statusDot == null) return;

        if (hasItem)
        {
            statusDot.enabled = true; // Hiện dấu chấm
            statusDot.color = isSelected ? Color.green : new Color(1f, 0.5f, 0f); // Xanh nếu chọn, Cam nếu không
        }
        else
        {
            statusDot.enabled = false; // Ẩn luôn dấu chấm nếu ô trống
        }
    }
}