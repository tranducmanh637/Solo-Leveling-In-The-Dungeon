using UnityEngine;
using TMPro;

public class CoinBarUI : MonoBehaviour
{
    public static CoinBarUI Instance;

    [Header("Giao diện Tiền Tệ")]
    [Tooltip("Kéo chữ hiển thị số Vàng vào đây")]
    public TextMeshProUGUI goldText; 

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Cập nhật số vàng ngay khi mới load game xong
        if (InventoryManager.Instance != null)
        {
            UpdateGold(InventoryManager.Instance.currentGold);
        }
    }

    // Hàm này sẽ được InventoryManager gọi mỗi khi nhặt hoặc tiêu tiền
    public void UpdateGold(int currentGoldAmount)
    {
        if (goldText != null)
        {
            // Hiển thị số có dấu phẩy hàng nghìn (VD: 1,000)
            goldText.text = currentGoldAmount.ToString("N0");
        }
    }
}