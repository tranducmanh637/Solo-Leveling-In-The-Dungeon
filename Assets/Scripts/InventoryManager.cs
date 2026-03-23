using System.Collections.Generic;
using UnityEngine;

// --- CLASS NHỎ NẰM CHUNG FILE ---
[System.Serializable]
public class ItemStack
{
    public ItemData item;
    public int quantity;

    public ItemStack(ItemData newItem, int newQuantity)
    {
        item = newItem;
        quantity = newQuantity;
    }
}

// --- CLASS CHÍNH QUẢN LÝ TÚI ĐỒ ---
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Cài Đặt Túi Đồ")]
    public int maxSlots = 50; 
    [Header("Tiền Tệ")]
    public int currentGold = 0;
    public List<ItemStack> items = new List<ItemStack>();
    
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool AddItem(ItemData itemToAdd, int amountToAdd = 1)
    {
        foreach (ItemStack stack in items)
        {
            if (stack.item == itemToAdd)
            {
                stack.quantity += amountToAdd; 
                Debug.Log($"Đã cộng dồn: {itemToAdd.itemName} (Tổng: {stack.quantity})");
                if (onItemChangedCallback != null) onItemChangedCallback.Invoke();
                return true; 
            }
        }

        if (items.Count >= maxSlots)
        {
            Debug.Log("Túi đồ đã đầy! Không thể nhặt thêm " + itemToAdd.itemName);
            return false; 
        }

        items.Add(new ItemStack(itemToAdd, amountToAdd));
        Debug.Log($"Đã nhặt: {itemToAdd.itemName} (Đang có: {items.Count}/{maxSlots})");

        if (onItemChangedCallback != null) onItemChangedCallback.Invoke();
        return true; 
    }
// --- HÀM NHẶT TIỀN ---
    public void AddGold(int amount)
    {
        currentGold += amount;
        
        // Gọi sang script CoinBarUI mới tạo
        if (CoinBarUI.Instance != null)
        {
            CoinBarUI.Instance.UpdateGold(currentGold);
        }
        
        Debug.Log($">> Đã nhặt được {amount} Vàng! Tổng: {currentGold}");
    }

    // --- HÀM TIÊU TIỀN ---
    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            
            // Gọi sang script CoinBarUI mới tạo
            if (CoinBarUI.Instance != null) 
            {
                CoinBarUI.Instance.UpdateGold(currentGold);
            }
            return true; // Đủ tiền, mua thành công
        }
        return false; // Không đủ tiền
    }
    public void RemoveItem(ItemData itemToRemove, int amountToRemove = 1)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].item == itemToRemove)
            {
                items[i].quantity -= amountToRemove; 
                
                if (items[i].quantity <= 0)
                {
                    items.RemoveAt(i);
                    Debug.Log("Đã dùng hết/vứt bỏ: " + itemToRemove.itemName);
                }
                
                if (onItemChangedCallback != null) onItemChangedCallback.Invoke();
                return;
            }
        }
    }

    public void UseItem(ItemData item)
    {
        if (item.itemType == ItemType.Weapon)
        {
            if (CharacterSwitcher.currentPlayer != null)
            {
                CharacterSwitcher.currentPlayer.EquipWeapon(item);
            }
        }
        else
        {
            Debug.Log("--- THÔNG BÁO ---");
            Debug.Log("Bạn vừa chọn: " + item.itemName);
            
            if (CharacterSwitcher.currentPlayer != null)
            {
                CharacterSwitcher.currentPlayer.EquipDefaultWeapon();
            }
        }
    }
    // Hàm hoán đổi vị trí vật phẩm
    public void ReorderItem(int fromIndex, int toIndex)
    {
        // Nếu kéo thả vào chính vị trí cũ thì bỏ qua
        if (fromIndex == toIndex) return; 

        // Nếu kéo một ô trống thì bỏ qua
        if (fromIndex >= items.Count) return;

        // 1. Lấy thông tin món đồ đang bị kéo
        var itemToMove = items[fromIndex];

        // 2. Rút nó ra khỏi vị trí cũ (Lúc này các ô phía sau sẽ tự động dồn lên 1 bậc)
        items.RemoveAt(fromIndex);

        // 3. Chèn nó vào vị trí mới
        if (toIndex >= items.Count)
        {
            // Nếu kéo thả vào tít ô cuối cùng (hoặc ô trống xa xôi), thì nhét nó vào cuối danh sách
            items.Add(itemToMove);
        }
        else
        {
            // Chèn vào vị trí đích (Lệnh Insert sẽ ép các ô đang đứng ở đó phải tự động lùi ra sau)
            items.Insert(toIndex, itemToMove);
        }

        // Gọi hàm cập nhật lại toàn bộ giao diện (Tuỳ thuộc vào cách bạn đang code, 
        // có thể bạn cần gọi hàm UpdateUI() từ script InventoryUI ở đây)
        InventoryUI.Instance.UpdateUI(); 
    }
    // Cắt giảm đồ tại 1 ô bất kỳ, nếu hết thì xóa luôn ô đó
    public void RemoveItemAtIndex(int index, int amount)
    {
        // Kiểm tra an toàn
        if (index < 0 || index >= items.Count) return;

        // Trừ số lượng
        items[index].quantity -= amount;
        
        // Nếu số lượng tụt xuống 0 hoặc âm, xóa nó khỏi danh sách
        if (items[index].quantity <= 0)
        {
            items.RemoveAt(index); 
        }
        
        // Vẽ lại giao diện túi đồ ngay lập tức
        InventoryUI.Instance.UpdateUI(); 
    }
}


