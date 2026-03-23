using UnityEngine;
using UnityEngine.EventSystems;

public class TrashSlot : MonoBehaviour, IDropHandler
{
    // HÀM NÀY CHẠY KHI CÓ MỘT MÓN ĐỒ BỊ KÉO THẢ VÀO THÙNG RÁC
    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject == null) return;

        // --- TRƯỜNG HỢP 1: VỨT ĐỒ TỪ TÚI (INVENTORY) ---
        InventorySlot invSlot = draggedObject.GetComponent<InventorySlot>();
        if (invSlot != null && invSlot.currentItem != null)
        {
            ItemData trashedItem = invSlot.currentItem;
            int trashedQty = InventoryManager.Instance.items[invSlot.slotIndex].quantity;

            // Tính 50% giá bán nhân với tổng số lượng đồ vứt đi (Làm tròn xuống để khỏi bị lẻ)
            int refundAmount = Mathf.FloorToInt((trashedItem.sellPrice * 0.5f) * trashedQty);

            // Hoàn lại Vàng
            if (refundAmount > 0)
            {
                InventoryManager.Instance.AddGold(refundAmount);
                Debug.Log($"[THÙNG RÁC] Đã tái chế {trashedItem.itemName} x{trashedQty} -> Nhận lại {refundAmount} Vàng");
            }

            // Xóa sạch món đồ đó khỏi túi
            InventoryManager.Instance.RemoveItemAtIndex(invSlot.slotIndex, trashedQty);
            return; // Xong việc thì thoát
        }

        // --- TRƯỜNG HỢP 2: VỨT ĐỒ TỪ THANH TOOLBAR ---
        ToolbarSlot tbSlot = draggedObject.GetComponent<ToolbarSlot>();
        if (tbSlot != null && tbSlot.assignedItem != null)
        {
            ItemData trashedItem = tbSlot.assignedItem;
            int trashedQty = tbSlot.currentQuantity;

            // Tính 50% tiền hoàn trả
            int refundAmount = Mathf.FloorToInt((trashedItem.sellPrice * 0.5f) * trashedQty);

            if (refundAmount > 0)
            {
                InventoryManager.Instance.AddGold(refundAmount);
                Debug.Log($"[THÙNG RÁC] Đã tái chế {trashedItem.itemName} x{trashedQty} -> Nhận lại {refundAmount} Vàng");
            }

            // Xóa sạch ô Toolbar
            tbSlot.ClearSlot();
            
            // Nếu đồ vừa bị vứt đang được cầm trên tay -> Báo cho nhân vật chuyển về Nắm đấm
            if (ToolbarManager.Instance != null && ToolbarManager.Instance.currentSelectedIndex == tbSlot.slotIndex)
            {
                ToolbarManager.Instance.SelectSlot(tbSlot.slotIndex);
            }
        }
    }
}