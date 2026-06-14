using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class HireManager : MonoBehaviour
{
    public static HireManager Instance;

    [Header("UI References")]
    public GameObject hirePanel;
    public Transform employeeContainer; 
    public GameObject employeeSlotPrefab; 

    [Header("Pagination UI")]
    public Button nextButton;
    public Button prevButton;

    [Header("System")]
    public int maxAgents = 5;
    public List<EmployeeData> availableEmployees; 
    
    [Header("Pagination Settings")]
    public int itemsPerPage = 3;
    private int currentPage = 0;

    [Header("Farm Settings")]
    public Transform defaultEmployeeHome;

    private DockStation currentDock;
    private List<AgentTaskManager> activeAgents = new List<AgentTaskManager>();

    private void Start()
    {
        currentPage = 0;
        GenerateEmployeeSlots();
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        hirePanel.SetActive(false);

        if (nextButton != null) nextButton.onClick.AddListener(NextPage);
        if (prevButton != null) prevButton.onClick.AddListener(PrevPage);

        PrewarmAgents();
    }

    public void OpenHireUI(DockStation dock)
    {
        currentDock = dock;
        if (currentPage != 0) 
        {
            currentPage = 0;
            GenerateEmployeeSlots();
        }
        
        hirePanel.SetActive(true);
    }

    public void CloseHireUI()
    {
        hirePanel.SetActive(false);
        currentDock = null;
    }

    private void GenerateEmployeeSlots()
    {
        // Bỏ các thẻ cũ
        foreach (Transform child in employeeContainer)
        {
            Destroy(child.gameObject);
        }

        // Tính toán đầu cuối cho danh sách
        int startIndex = currentPage * itemsPerPage;
        int endIndex = Mathf.Min(startIndex + itemsPerPage, availableEmployees.Count);

        // Sinh ra các slot theo dộ dài danh sách
        for (int i = startIndex; i < endIndex; i++)
        {
            EmployeeData emp = availableEmployees[i];
            GameObject slotObj = Instantiate(employeeSlotPrefab, employeeContainer);
            EmployeeSlotUI slotUI = slotObj.GetComponent<EmployeeSlotUI>();
            if (slotUI != null)
            {
                slotUI.Setup(emp, this);
            }
        }

        if (prevButton != null) prevButton.interactable = (currentPage > 0);
        if (nextButton != null) nextButton.interactable = (endIndex < availableEmployees.Count);
    }

    public void NextPage()
    {
        if ((currentPage + 1) * itemsPerPage < availableEmployees.Count)
        {
            currentPage++;
            GenerateEmployeeSlots();
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            GenerateEmployeeSlots(); 
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

    private void PrewarmAgents()
    {
        foreach (EmployeeData emp in availableEmployees)
        {
            if (emp.agentPrefab != null)
            {
                // Sinh ra agent tạm
                GameObject dummyAgent = Instantiate(emp.agentPrefab, new Vector3(-9999, -9999, 0), Quaternion.identity);
                
                // Tắt đi luôn
                dummyAgent.SetActive(false); 
                
                // Hủy luôn
                Destroy(dummyAgent);
            }
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

    public List<AgentTaskManager> GetActiveAgents()
    {
        return activeAgents;
    }

    public EmployeeData GetEmployeeDataByID(string id)
    {
        return availableEmployees.Find(e => e.employeeID == id);
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