using UnityEngine;
using UnityEngine.UI;
using TMPro; // Bắt buộc thêm thư viện này để dùng Text số lượng

public class ManaBarUI : MonoBehaviour
{
    [Header("Thành phần UI")]
    [Tooltip("Kéo ảnh thanh màu xanh (đã chọn Image Type là Filled) vào đây")]
    public Image fillImage;
    
    [Tooltip("Kéo chữ Text hiển thị số Mana vào đây")]
    public TextMeshProUGUI manaText;

    // Biến để nhớ mức Mana tối đa dùng cho phép chia tỷ lệ phần trăm
    private float maxManaValue;

    // Hàm này được gọi 1 lần duy nhất lúc mới vào game
    public void SetMaxMana(float maxMana)
    {
        maxManaValue = maxMana;

        if (fillImage != null)
        {
            fillImage.fillAmount = 1f; // 1f tương đương với 100% (Đổ đầy bình)
        }

        if (manaText != null)
        {
            manaText.text = maxManaValue + " / " + maxManaValue;
        }
    }

    // Hàm này được gọi mỗi khi tung chiêu (trừ) hoặc uống bình (cộng)
    public void UpdateMana(float currentMana)
    {
        if (fillImage != null && maxManaValue > 0)
        {
            // Lệnh fillAmount chỉ nhận giá trị từ 0 đến 1. 
            // Ta lấy (Mana Hiện Tại / Mana Tối Đa) để ra phần trăm độ dài thanh.
            fillImage.fillAmount = currentMana / maxManaValue;
        }

        if (manaText != null)
        {
            // Dùng Mathf.RoundToInt để làm tròn số (VD: 15.5 sẽ hiện là 16) cho đẹp
            manaText.text = Mathf.RoundToInt(currentMana) + " / " + maxManaValue;
        }
    }
}