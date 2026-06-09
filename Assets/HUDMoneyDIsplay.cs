using UnityEngine;
using TMPro;

public class HUDMoneyDisplay : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("Kéo cái Text Mesh Pro hiển thị tiền ngoài HUD màn hình vào đây.")]
    [SerializeField] private TMP_Text moneyText;

    private void Start()
    {
        // 1. Đăng ký lắng nghe sự kiện mỗi khi tiền thay đổi từ hệ thống CurrencyController
        if (CurrencyController.Instance != null)
        {
            CurrencyController.Instance.OnGoldChanged += UpdateHUDMoneyText;
            
            // 2. Cập nhật số tiền thực tế đang có ngay khi vừa vào game
            UpdateHUDMoneyText(CurrencyController.Instance.GetGold());
        }
        else
        {
            Debug.LogWarning("Không tìm thấy CurrencyController trong Scene để đồng bộ tiền HUD!");
        }
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện khi Object bị hủy để tránh lỗi rác bộ nhớ (Memory Leak)
        if (CurrencyController.Instance != null)
        {
            CurrencyController.Instance.OnGoldChanged -= UpdateHUDMoneyText;
        }
    }

    // Hàm tự động chạy khi sự kiện đổi tiền được kích hoạt
    private void UpdateHUDMoneyText(int currentGold)
    {
        if (moneyText != null)
        {
            moneyText.text = currentGold.ToString();
        }
    }
}