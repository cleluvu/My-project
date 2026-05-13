using System.Collections.Generic;
using UnityEngine;

public class HireManager : MonoBehaviour
{
    public static HireManager Instance;

    [Header("UI References")]
    public GameObject hirePanel;
    public Transform employeeContainer; 
    public GameObject employeeSlotPrefab; 

    [Header("System")]
    public int maxAgents = 5;
    public List<EmployeeData> availableEmployees; 

    [Header("Farm Settings")]
    public Transform defaultEmployeeHome;

    private DockStation currentDock;

    private List<AgentTaskManager> activeAgents = new List<AgentTaskManager>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        hirePanel.SetActive(false);
    }

    public void OpenHireUI(DockStation dock)
    {
        currentDock = dock;
        hirePanel.SetActive(true);
        GenerateEmployeeSlots();
    }

    public void CloseHireUI()
    {
        hirePanel.SetActive(false);
        currentDock = null;
    }

    private void GenerateEmployeeSlots()
    {
        // Clear mấy cái thẻ cũ
        foreach (Transform child in employeeContainer)
        {
            Destroy(child.gameObject);
        }

        // Sinh ra thẻ mới
        foreach (EmployeeData emp in availableEmployees)
        {
            GameObject slotObj = Instantiate(employeeSlotPrefab, employeeContainer);
            EmployeeSlotUI slotUI = slotObj.GetComponent<EmployeeSlotUI>();
            if (slotUI != null)
            {
                slotUI.Setup(emp, this);
            }
        }
    }

    public void HireSpecificEmployee(EmployeeData emp)
    {
        if (activeAgents.Count >= maxAgents)
        {
            Debug.Log("Đã đạt số lượng nhân viên giới hạn!");
            return;
        }

        if (CurrencyController.Instance.SpendGold(emp.hirePrice))
        {
            SpawnAgent(emp, emp.hireDurationDays, currentDock.spawnPoint.position);
            CloseHireUI(); 
        }
        else
        {
            Debug.Log("Không đủ tiền!");
        }
    }

    private void SpawnAgent(EmployeeData emp, int daysLeft, Vector3 spawnPos)
    {
        GameObject newAgent = Instantiate(emp.agentPrefab, spawnPos, Quaternion.identity);
        AgentTaskManager taskManager = newAgent.GetComponent<AgentTaskManager>();
        
        if (taskManager != null)
        {
            taskManager.SetupHiredAgent(emp.employeeID, daysLeft, currentDock.spawnPoint, defaultEmployeeHome);
            activeAgents.Add(taskManager);
        }
    }

    public void RemoveAgent(AgentTaskManager agent)
    {
        if (activeAgents.Contains(agent))
        {
            activeAgents.Remove(agent);
        }
    }

    public List<AgentSaveData> GetSaveData()
    {
        List<AgentSaveData> saveData = new List<AgentSaveData>();
        foreach (var agent in activeAgents)
        {
            saveData.Add(new AgentSaveData {
                employeeID = agent.hiredEmployeeID,
                remainingDays = agent.remainingDays,
                position = agent.transform.position
            });
        }
        return saveData;
    }

    public void RestoreData(List<AgentSaveData> saveData, DockStation dock)
    {
        currentDock = dock; 
        foreach (var data in saveData)
        {
            EmployeeData emp = availableEmployees.Find(e => e.employeeID == data.employeeID);
            if (emp != null) SpawnAgent(emp, data.remainingDays, data.position);
        }
    }
}