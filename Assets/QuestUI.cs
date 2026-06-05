using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    public Transform questListContent;
    public GameObject questEntryPrefab;
    public GameObject ObjectiveTextPrefab;

    void Start()
    {
        UpdateQuestUI();
    }

    public void UpdateQuestUI()
    {
        // 1. Xóa toàn bộ các UI Entry cũ đang hiển thị trên màn hình
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }

        // Kiểm tra an toàn: Nếu QuestController chưa khởi tạo hoặc danh sách trống thì không cần build
        if (QuestController.Instance == null || QuestController.Instance.activateQuests == null)
        {
            return;
        }

        // 2. Duyệt qua danh sách lấy từ QuestController
        foreach (var quest in QuestController.Instance.activateQuests)
        {
            GameObject entry = Instantiate(questEntryPrefab, questListContent);
            TMP_Text questNameText = entry.transform.Find("QuestName").GetComponent<TMP_Text>();
            Transform objectiveList = entry.transform.Find("ObjectiveList");

            // Gán tên Quest lấy từ dữ liệu ScriptableObject
            questNameText.text = quest.quest.name;

            // 3. Duyệt qua các mục tiêu (Objectives) của Quest đó để hiển thị tiến độ
            foreach (var objective in quest.objectives)
            {
                GameObject objTextGO = Instantiate(ObjectiveTextPrefab, objectiveList);
                TMP_Text objText = objTextGO.GetComponent<TMP_Text>();
                objText.text = $"{objective.description} ({objective.currentAmount}/{objective.requiredAmount})";
            }
        }
    }
}