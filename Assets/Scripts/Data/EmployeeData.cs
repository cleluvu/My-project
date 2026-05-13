using UnityEngine;

[CreateAssetMenu(fileName = "New_Employee", menuName = "Farm/Employee Data")]
public class EmployeeData : ScriptableObject
{
    public string employeeID;
    public string employeeName;
    public Sprite avatar;
    public int hirePrice = 50;
    public int hireDurationDays = 3; // Số ngày làm việc
    public AgentRole role;           // Nghề nghiệp mặc định
    public GameObject agentPrefab;   // Prefab chứa MyAgent + AgentTaskManager
}