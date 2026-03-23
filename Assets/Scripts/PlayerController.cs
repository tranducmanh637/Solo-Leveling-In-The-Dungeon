using UnityEngine;

// Tự động thêm Rigidbody2D và SpriteRenderer nếu object chưa có
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Dữ liệu nhân vật")]
    public CharacterData characterData; // Kéo thả file data của bạn vào đây

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private Vector2 movement;
    private CharacterSwitcher switcher;
    [Header("Cài đặt Animation")]
    private Sprite[] currentAnimArray;  // Mảng ảnh đang chạy hiện tại
    private int currentFrame;           // Frame hiện tại
    private float frameTimer;           // Bộ đếm thời gian
    private float frameInterval = 0.125f; // Đổi frame mỗi 0.5 giây
    
    // Thêm dòng này vào script PlayerController.cs
    [Header("Vũ Khí Hiện Tại")]
    public ItemData currentWeapon;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        switcher = GetComponent<CharacterSwitcher>();
        // Thiết lập chuẩn cho game 2D Top-down
        rb.gravityScale = 0f; 
        rb.freezeRotation = true; 

        // Gán animation mặc định khi mới vào game
        if (characterData != null)
        {
            ChangeAnimation(characterData.idleFrames);
        }
    }

    void Update()
    {
        // --- 3. KIỂM TRA ĐIỀU KIỆN TRƯỚC KHI NHẬN NÚT ĐIỀU KHIỂN ---
        // Chỉ cho phép bấm phím nếu Tướng này đang được điều khiển (isPlayer = true)
        if (switcher != null && switcher.isPlayer)
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
            movement = movement.normalized;
        }
        else
        {
            // Nếu là NPC, ép vận tốc về 0. 
            // Nhờ đó, UpdateAnimationState ở dưới sẽ tự động biết là đang đứng im và bật Idle!
            movement = Vector2.zero;
        }

        // Các hàm Animation dưới này VẪN ĐƯỢC CHẠY BÌNH THƯỜNG cho cả Player và NPC
        UpdateAnimationState();
        PlayAnimation();
    }

    void FixedUpdate()
    {
        if (characterData != null)
        {
            rb.MovePosition(rb.position + movement * characterData.speed * Time.fixedDeltaTime);
        }
    }
    private void UpdateAnimationState()
    {
        if (characterData == null) return;

        if (movement == Vector2.zero) // Đứng yên
        {
            ChangeAnimation(characterData.idleFrames);
        }
        else // Đang di chuyển
        {
            // Ưu tiên hiển thị animation đi ngang nếu người chơi bấm đi chéo
            if (Mathf.Abs(movement.x) > 0)
            {
                ChangeAnimation(characterData.walkRightFrames);
                // Lật ảnh sang trái nếu đang đi sang trái
                spriteRenderer.flipX = (movement.x < 0);
            }
            else if (movement.y > 0) // Đi lên
            {
                ChangeAnimation(characterData.walkBackFrames);
            }
            else if (movement.y < 0) // Đi xuống
            {
                ChangeAnimation(characterData.walkFrontFrames);
            }
        }
    }

    private void ChangeAnimation(Sprite[] newAnimArray)
    {
        // Chỉ reset lại từ đầu nếu chuyển sang một hành động khác
        if (currentAnimArray != newAnimArray)
        {
            currentAnimArray = newAnimArray;
            currentFrame = 0;
            frameTimer = 0f;

            // Đổi ngay lập tức sang frame đầu tiên của hành động mới
            if (currentAnimArray != null && currentAnimArray.Length > 0)
            {
                spriteRenderer.sprite = currentAnimArray[0];
            }
        }
    }

    private void PlayAnimation()
    {
        // Bỏ qua nếu mảng ảnh trống
        if (currentAnimArray == null || currentAnimArray.Length == 0) return;

        // Cộng dồn thời gian
        frameTimer += Time.deltaTime;

        // Nếu đủ 0.5s thì chuyển frame
        if (frameTimer >= frameInterval)
        {
            frameTimer -= frameInterval; // Trừ đi thời gian đã chờ
            currentFrame++; // Chuyển sang ảnh tiếp theo

            // Nếu chạy hết mảng ảnh thì quay lại ảnh đầu tiên (loop)
            if (currentFrame >= currentAnimArray.Length)
            {
                currentFrame = 0;
            }

            // Gắn ảnh lên màn hình
            spriteRenderer.sprite = currentAnimArray[currentFrame];
        }
    }
}
