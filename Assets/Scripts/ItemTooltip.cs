using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemTooltip : MonoBehaviour
{
    public static ItemTooltip Instance;

    [Header("Liên kết Giao diện")]
    public GameObject tooltipPanel; // Cái khung Panel tổng
    public Image backgroundImage;   // Ảnh nền đổi màu theo độ hiếm
    public Image itemIcon;          // Ảnh món đồ
    
    [Header("Liên kết Text")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI statsText;       // Ghi Giá bán, Sát thương...
    public TextMeshProUGUI descriptionText; // Ghi mô tả

    private void Awake()
    {
        Instance = this;
        HideTooltip(); // Tắt bảng lúc mới vào game
    }

    private void Update()
    {
        // Nếu bảng đang bật, ép nó bay theo con trỏ chuột
        if (tooltipPanel.activeSelf)
        {
            // Cộng thêm offset (15, -15) để bảng nằm chếch xuống dưới góc phải con chuột, không bị ngón tay che khuất
            transform.position = Input.mousePosition + new Vector3(15f, -15f, 0f); 
        }
    }

    public void ShowTooltip(ItemData item)
    {
        if (item == null) return;

        // 1. Gắn Tên và Ảnh
        nameText.text = item.itemName;
        itemIcon.sprite = item.itemIcon;
        descriptionText.text = item.description;

        // 2. Xử lý Độ Hiếm (Đổi chữ và lấy ảnh nền từ InventoryUI)
        string rarityString = "";
        int rarityIndex = (int)item.rarity;
        
        switch (item.rarity)
        {
            case ItemRarity.Common: rarityString = "<color=#FFFFFF>Thường</color>"; break;
            case ItemRarity.Uncommon: rarityString = "<color=#00FF00>Hơi Hiếm</color>"; break;
            case ItemRarity.Rare: rarityString = "<color=#0080FF>Hiếm</color>"; break;
            case ItemRarity.Epic: rarityString = "<color=#A020F0>Rất Hiếm</color>"; break;
            case ItemRarity.Legendary: rarityString = "<color=#FFA500>Huyền Thoại</color>"; break;
            case ItemRarity.Mythic: rarityString = "<color=#FF0000>Thần Thoại</color>"; break;
        }
        rarityText.text = rarityString;

        if (InventoryUI.Instance != null && rarityIndex < InventoryUI.Instance.rarityBackgrounds.Length)
        {
            backgroundImage.sprite = InventoryUI.Instance.rarityBackgrounds[rarityIndex];
        }

        // 3. Xử lý Chỉ số (Phân biệt vũ khí và vật phẩm thường)
        string stats = $"Giá bán: <color=#FFD700>{item.sellPrice} Vàng</color>\n"; // Vàng luôn có
        
        if (item.isWeapon)
        {
            stats += $"Sức mạnh: <color=#FF4500>+{item.damage} ST</color>\n";
            stats += $"Tiêu hao: <color=#00FFFF>{item.manaCost} Mana</color>";
        }
        statsText.text = stats;

        // Hiện bảng lên
        tooltipPanel.SetActive(true);
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}