using UnityEngine;
using UnityEngine.UI;
using TMPro; // Cần dòng này để dùng TextMeshPro

public class HealthBar : MonoBehaviour
{
    [Header("Thành phần UI")]
    public Image fillImage;           // Ảnh thanh máu (màu đỏ hoặc xanh)
    public TextMeshProUGUI healthText;// Chữ số hiển thị máu

    // Hàm này sẽ được Nhân vật hoặc Quái vật gọi mỗi khi mất máu
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        // 1. Chỉnh độ dài thanh máu (fillAmount chạy từ 0 đến 1)
        if (fillImage != null)
        {
            fillImage.fillAmount = currentHealth / maxHealth;
        }
        
        // 2. Cập nhật bộ số đếm
        // Dùng Mathf.Max(0, currentHealth) để nếu máu âm thì nó vẫn hiện là 0/100
        if (healthText != null)
        {
            float displayHealth = Mathf.Max(0, currentHealth);
            healthText.text = displayHealth.ToString("0") + " / " + maxHealth.ToString("0");
        }
    }
}