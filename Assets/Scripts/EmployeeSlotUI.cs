using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmployeeSlotUI : MonoBehaviour
{
    public Image avatarImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI durationText;
    public Button hireButton;

    private EmployeeData myData; 
    private HireManager manager;

    public void Setup(EmployeeData data, HireManager hireManager)
    {
        myData = data;
        manager = hireManager;

        // Cập nhật giao diện
        avatarImage.sprite = data.avatar;
        nameText.text = data.employeeName;
        priceText.text = data.hirePrice.ToString() + " Gold";
        durationText.text = data.hireDurationDays.ToString() + " Days";

        hireButton.onClick.RemoveAllListeners();
        hireButton.onClick.AddListener(OnHireClicked);
    }

    private void OnHireClicked()
    {
        manager.HireSpecificEmployee(myData);
    }
}