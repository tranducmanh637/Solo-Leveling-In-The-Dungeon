using UnityEngine;
using System.Collections.Generic;

public enum EnemyActionType { Move, Wait }

[System.Serializable]
public class EnemyAction
{
    public EnemyActionType actionType;
    public Vector2 targetPosition; 
    public float duration; 
}

// --- THÊM CLASS NÀY ĐỂ QUẢN LÝ TỈ LỆ RỚT ĐỒ ---
[System.Serializable]
public class LootDrop
{
    [Tooltip("Vật phẩm có thể rớt (Vũ khí, hạt giống...)")]
    public ItemData item;
    
    [Tooltip("Tỉ lệ rớt (Từ 0% đến 100%)")]
    [Range(0f, 100f)] 
    public float dropChance = 50f; 
    
    [Tooltip("Số lượng rớt tối thiểu")]
    public int minAmount = 1;
    
    [Tooltip("Số lượng rớt tối đa")]
    public int maxAmount = 1;
}
// ----------------------------------------------

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game Data/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("--- THÔNG TIN CƠ BẢN ---")]
    public string enemyName;
    [TextArea(3, 5)] public string description;

    [Header("--- VŨ KHÍ & CHIẾN ĐẤU ---")]
    public ItemData equippedWeapon;
    public float orbitRadius = 1.2f;
    public float attackRange = 1.5f;

    [Header("--- CHỈ SỐ SỨC MẠNH ---")]
    public float maxHealth = 100f;
    public float strength = 10f;
    public float speed = 2f;

    [Header("--- TRÍ TUỆ NHÂN TẠO (AI) ---")]
    public float detectionRange = 5f;
    public List<EnemyAction> actionList;

    [Header("--- HÌNH ẢNH ANIMATION ---")]
    public Sprite[] idleFrames;
    public Sprite[] walkFrontFrames;
    public Sprite[] walkBackFrames;
    public Sprite[] walkRightFrames;
    
    [Header("--- RỚT VÀNG & MANA & MÁU ---")] 
    public int goldTotalAmount = 50;
    public int goldVisualPieces = 3; 
    public GameObject goldPrefab;

    [Space(10)]
    public int manaTotalAmount = 20;
    public int manaVisualPieces = 2;
    public GameObject manaPrefab;

    [Space(10)]
    public int healthTotalAmount = 30;
    public int healthVisualPieces = 1;
    public GameObject healthPrefab;

    [Header("--- RỚT DANH SÁCH VẬT PHẨM (THEO TỈ LỆ) ---")]
    public GameObject droppedItemPrefab;
    
    // --- ĐÃ ĐỔI TỪ List<ItemData> SANG List<LootDrop> ---
    [Tooltip("Danh sách các vật phẩm sẽ rớt ra dựa theo tỉ lệ %")]
    public List<LootDrop> dropItems = new List<LootDrop>();
}