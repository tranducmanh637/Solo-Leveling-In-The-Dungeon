using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Cài đặt Theo dõi (Follow)")]
    [Tooltip("Tốc độ camera di chuyển bám theo nhân vật")]
    public float smoothSpeed = 5f;

    [Header("Cài đặt Thu phóng (Zoom)")]
    public Camera cam;
    
    [Tooltip("Kích thước Camera mặc định (Mức 1x)")]
    public float defaultSize = 5f; 
    public float zoomSpeed = 5f;

    // Các mức zoom: 1x, 0.8x, 0.5x
    // Giải thích: Mức 0.5x nghĩa là mọi thứ nhỏ đi một nửa -> Camera phải mở rộng ra gấp đôi (Size to ra).
    private float[] zoomLevels = { 1f, 0.8f, 0.5f };
    private int currentZoomIndex = 0; // Bắt đầu ở mức 1x (Vị trí số 0)
    
    private float targetSize;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        
        // Cài đặt size ban đầu
        targetSize = defaultSize / zoomLevels[currentZoomIndex];
        cam.orthographicSize = targetSize;
    }

    void LateUpdate()
    {
        // 1. CAMERA BÁM THEO NHÂN VẬT HIỆN TẠI (Dùng LateUpdate để không bị giật lag khung hình)
        if (CharacterSwitcher.currentPlayer != null)
        {
            Vector3 targetPosition = CharacterSwitcher.currentPlayer.transform.position;
            targetPosition.z = transform.position.z; // Phải giữ nguyên trục Z của Camera (thường là -10)

            // Di chuyển mượt mà (Lerp)
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }

        // 2. LÀM MƯỢT QUÁ TRÌNH THU PHÓNG (ZOOM)
        if (cam != null && Mathf.Abs(cam.orthographicSize - targetSize) > 0.01f)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, zoomSpeed * Time.deltaTime);
        }
    }

    // --- CÁC HÀM DÀNH CHO NÚT BẤM UI ---

    // Hàm gắn vào Nút Dấu Cộng (+) : Phóng to cảnh vật / Nhìn gần lại
    public void ZoomIn()
    {
        if (currentZoomIndex > 0)
        {
            currentZoomIndex--; // Lùi index về phía 1x
            targetSize = defaultSize / zoomLevels[currentZoomIndex];
            Debug.Log($"[CAMERA] Phóng to: {zoomLevels[currentZoomIndex]}x");
        }
    }

    // Hàm gắn vào Nút Dấu Trừ (-) : Thu nhỏ cảnh vật / Nhìn xa ra bao quát hơn
    public void ZoomOut()
    {
        if (currentZoomIndex < zoomLevels.Length - 1)
        {
            currentZoomIndex++; // Tăng index về phía 0.5x
            targetSize = defaultSize / zoomLevels[currentZoomIndex];
            Debug.Log($"[CAMERA] Thu nhỏ: {zoomLevels[currentZoomIndex]}x");
        }
    }
}