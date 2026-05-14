using UnityEngine;
using System.Collections.Generic;

public class EmployeeMenuController : MonoBehaviour
{
    public Transform contentArea;
    public GameObject employeeMenuSlotPrefab;

    private void OnEnable()
    {
        RefreshEmployeeList();
    }

    public void RefreshEmployeeList()
    {
        // Xóa card cũ
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }

        // Lấy danh sách nhân viên
        if (HireManager.Instance == null) return;

        List<AgentTaskManager> activeAgents = HireManager.Instance.GetActiveAgents();

        if (activeAgents.Count == 0)
        {
            Debug.Log("Hiện không có nhân viên nào làm việc.");
            return;
        }

        // Sinh card mới
        foreach (AgentTaskManager agent in activeAgents)
        {
            if (agent.isLeaving) continue; 

            GameObject slotObj = Instantiate(employeeMenuSlotPrefab, contentArea);
            EmployeeMenuSlot slotScript = slotObj.GetComponent<EmployeeMenuSlot>();

            EmployeeData data = HireManager.Instance.GetEmployeeDataByID(agent.hiredEmployeeID);
            
            if (data != null)
            {
                slotScript.Setup(data.employeeName, data.avatar, agent.role, agent.remainingDays, agent, this);
            }
        }
    }
}