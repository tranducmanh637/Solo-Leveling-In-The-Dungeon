using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Game Data/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Thông Tin Cơ Bản")]
    public string characterName;
    
    [TextArea(3, 10)]
    public string description;

    [Header("Hình Ảnh Animation (Frame by Frame)")]
    [Tooltip("Các khung hình (sprites) khi nhân vật đứng yên")]
    public Sprite[] idleFrames;

    [Tooltip("Các khung hình khi bước đi hướng xuống (nhìn thẳng)")]
    public Sprite[] walkFrontFrames;
    
    [Tooltip("Các khung hình khi bước đi hướng lên (quay lưng)")]
    public Sprite[] walkBackFrames;
    
    [Tooltip("Các khung hình khi bước đi sang phải (Lật X khi sang trái)")]
    public Sprite[] walkRightFrames;

    [Header("Chỉ Số Cơ Bản (Stats)")]
    public float strength;       // Sức mạnh / Sát thương
    public float speed;          // Tốc độ di chuyển
    
    [Header("Máu & Năng Lượng")]
    public float maxHealth;      // Máu tối đa
    public float hpRegen;    // Tốc độ hồi máu (máu/giây)
    
    public float maxMana;        // Mana tối đa
    public float manaRegen;      // Tốc độ hồi mana (mana/giây)
}