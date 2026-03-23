using UnityEngine;

public enum ItemType
{
    Weapon,     // Vũ khí
    Seed,       // Hạt giống
    Tool,       // Công cụ
    Fruit,      // Trái cây
    Other       // Khác
}
public enum ItemRarity
{
    Common,     // Vật phẩm thường (Thường quy ước màu Trắng/Xám)
    Uncommon,   // Hơi hiếm (Màu Xanh lá)
    Rare,       // Hiếm (Màu Xanh dương)
    Epic,       // Rất hiếm (Màu Tím)
    Legendary,  // Huyền thoại (Màu Cam/Vàng)
    Mythic      // Thần thoại / Cực hiếm (Màu Đỏ/Hồng)
}
[CreateAssetMenu(fileName = "NewItem", menuName = "Game Data/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Thông Tin Cơ Bản")]
    public string itemName;
    public ItemType itemType; 
    
    [TextArea(2, 5)]
    public string description;
    public Sprite itemIcon;
    [Header("Cài đặt hiển thị")]
    [Tooltip("Kích thước khi rớt xuống đất (1 là bình thường, 0.5 là nhỏ đi một nửa)")]
    public float dropScale = 1f; // Mặc định là 1 (giữ nguyên gốc)
    public bool isStackable; 
    public int sellPrice;    
    [Header("--- PHÂN LOẠI & ĐỘ HIẾM ---")]
    [Tooltip("Chọn độ hiếm cho vật phẩm này")]
    public ItemRarity rarity = ItemRarity.Common;
    [Header("Phân loại")]
    [Tooltip("Đánh dấu tích nếu đây là Vũ khí/Trang bị. Bỏ tích nếu là Máu/Mana/Vật liệu.")]
    public bool isWeapon = false;
    [Header("Dành riêng cho Vũ Khí (Nếu có)")]
    [Tooltip("Kéo thả PREFAB của thanh kiếm/cây gậy vào đây")]
    public GameObject weaponPrefab; 
    public float damage;
    public float manaCost;
    
}