using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmployeeMenuSlot : MonoBehaviour
{
    public Image avatarImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI roleText;
    public TextMeshProUGUI daysLeftText;
    
    [Header("Fire Action")]
    public Button fireButton;
    
    private AgentTaskManager myAgent;
    private EmployeeMenuController myController;

    public void Setup(string empName, Sprite avatar, AgentRole role, int days, AgentTaskManager agent, EmployeeMenuController controller)
    {
        nameText.text = empName;
        avatarImage.sprite = avatar;
        roleText.text = "Role: " + role.ToString();
        daysLeftText.text = "Contract: " + days + " Days left";

        myAgent = agent;
        myController = controller;

        fireButton.onClick.RemoveAllListeners();
        fireButton.onClick.AddListener(OnFireClicked);
    }

    private void OnFireClicked()
    {
        if (myAgent != null)
        {
            myAgent.FireAgent();
            
            Destroy(gameObject); 
        }
    }
}