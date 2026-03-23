using UnityEngine;

public enum PickupType { Gold, Mana, Health, Item } // Đã thêm Health

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))] 
public class PickupItem : MonoBehaviour
{
    [Header("Thông tin cơ bản")]
    public PickupType itemType;
    public int amount = 1;

    [Header("Dành cho Vật Phẩm (Vũ khí, Máu...)")]
    public ItemData itemData; 

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Setup(PickupType type, int dropAmount)
    {
        itemType = type;
        amount = dropAmount;
    }

    public void SetupItem(ItemData data, int dropAmount)
    {
        itemType = PickupType.Item;
        itemData = data;
        amount = dropAmount;

        
        if (spriteRenderer != null && itemData != null && itemData.itemIcon != null)
        {
            spriteRenderer.sprite = itemData.itemIcon;
        }
      
        if (itemData != null)
        {
            gameObject.name = "Drop_" + itemData.name; 
        }
        transform.localScale = new Vector3(itemData.dropScale, itemData.dropScale, 1f);
    }

private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CharacterSwitcher player = collision.GetComponent<CharacterSwitcher>();
            if (player != null)
            {
                bool shouldDestroy = false; 

                if (itemType == PickupType.Gold)
                {
                    Debug.Log($"[NHẶT ĐỒ] Bạn vừa nhặt được {amount} Vàng!");
                    InventoryManager.Instance.AddGold(amount);
                    shouldDestroy = true; 
                }
                // Khúc nhặt Mana cũ của bạn
                else if (itemType == PickupType.Mana)
                {
                    player.RestoreMana(amount);
                    Debug.Log($"[NHẶT ĐỒ] Bạn vừa hồi được {amount} Mana!");
                    shouldDestroy = true; 
                }
                // --- THÊM KHÚC NÀY ĐỂ NHẶT BÌNH MÁU ---
                else if (itemType == PickupType.Health)
                {
                    player.RestoreHealth(amount);
                    Debug.Log($"[NHẶT ĐỒ] Bạn vừa hồi được {amount} Máu!");
                    shouldDestroy = true; 
                }

                else if (itemType == PickupType.Item && itemData != null)
                {
                    if (InventoryManager.Instance != null)
                    {
                        // --- TRUYỀN THÊM BIẾN amount VÀO ĐÂY ---
                        bool wasPickedUp = InventoryManager.Instance.AddItem(itemData, amount);
                        
                        if (wasPickedUp)
                        {
                            shouldDestroy = true; 
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Không tìm thấy InventoryManager.Instance trên bản đồ!");
                    }
                }
                if (shouldDestroy)
                {
                    Destroy(gameObject); 
                }
            }
        }
    }
}