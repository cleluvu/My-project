using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestController : MonoBehaviour
{
    public static QuestController Instance { get; private set; }
    
    [Header("Quest Lists")]
    public List<QuestProgress> activateQuests = new();
    
    // Lưu danh sách Quest đã hoàn thành và trả cho NPC để tránh nhận lại
    public List<string> handInQuestIDs = new();
    
    private QuestUI questUI;
    private ItemDictionary itemDictionary; // Cache lại biến này để tìm ảnh phần thưởng

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        questUI = FindFirstObjectByType<QuestUI>();
    }

    private void Start()
    {
        // Khởi tạo tham chiếu đến từ điển vật phẩm trong Scene
        itemDictionary = FindAnyObjectByType<ItemDictionary>();

        // Đăng ký sự kiện: Kho đồ thay đổi thì tự động quét lại tiến độ Quest
        if (InventoryController.Instance != null)
        {
            InventoryController.Instance.onInventoryChanged += CheckInventoryForQuests;
        }
    }

    public void AcceptQuest(Quest quest)
    {
        // Nếu quest đang làm hoặc đã trả rồi thì không nhận lại nữa
        if (IsQuestActive(quest.questID) || IsQuestHandedIn(quest.questID)) return;

        activateQuests.Add(new QuestProgress(quest));
        
        // Kiểm tra ngay xem trong người đã có sẵn vật phẩm yêu cầu từ trước chưa
        CheckInventoryForQuests();
        
        if (questUI != null) questUI.UpdateQuestUI();
    }

    // Cập nhật tiến độ quest dựa trên số lượng vật phẩm trong kho đồ
    public void CheckInventoryForQuests()
    {
        if (InventoryController.Instance == null) return;
        
        // Lấy danh sách số lượng tổng từ cả hòm chính lẫn Hotbar
        var itemCounts = InventoryController.Instance.GetItemCounts();

        foreach (var questProgress in activateQuests)
        {
            foreach (var objective in questProgress.objectives)
            {
                // Thử ép kiểu ObjectiveID (chuỗi string) sang ID vật phẩm (int)
                if (int.TryParse(objective.objectiveID, out int itemID))
                {
                    int currentItemCount = itemCounts.ContainsKey(itemID) ? itemCounts[itemID] : 0;
                    
                    // Giới hạn số lượng hiển thị không vượt quá số lượng yêu cầu
                    objective.currentAmount = Mathf.Min(currentItemCount, objective.requiredAmount);
                }
            }
        }

        if (questUI != null) questUI.UpdateQuestUI();
    }

    // Check Quest đã đủ đk hoàn thành chưa
    public bool IsQuestCompleted(string questID)
    {
        QuestProgress quest = activateQuests.Find(q => q.QuestID == questID);
        if (quest == null) return false;
        return quest.IsCompleted;
    }

    // Xử lý trả Quest + nhận thưởng
    public void HandInQuest(string questID)
    {
        QuestProgress questProgress = activateQuests.Find(q => q.QuestID == questID);
        if (questProgress == null) return;

        // 1. Ghi nhớ trạng thái hoàn thành vĩnh viễn, tránh nhận lại Quest cũ
        if (!handInQuestIDs.Contains(questID))
        {
            handInQuestIDs.Add(questID);
        }

        // Phát thưởng và bắn pop up tương ứng
        GiveQuestRewards(questProgress.quest);

        // 2. Xóa Quest khỏi danh sách đang thực hiện (Bảng UI Quest Log sẽ ẩn dòng Quest này)
        activateQuests.Remove(questProgress);

        // 3. Làm mới giao diện Quest UI công khai trên màn hình
        if (questUI != null) questUI.UpdateQuestUI();
        
        Debug.Log($"[{nameof(QuestController)}] Trả thành công Quest: {questID}. Đã nhận quà thưởng, vật phẩm nhiệm vụ vẫn giữ nguyên!");
    }

    // Hàm duyệt danh sách quà cấu hình từ ScriptableObject để phát cho Player kèm Popup
    private void GiveQuestRewards(Quest questData)
    {
        if (questData.rewards == null || questData.rewards.Count == 0) return;

        foreach (var reward in questData.rewards)
        {
            if (reward.type == RewardType.Gold)
            {
                // Gọi sang CurrencyController để cộng tiền thưởng trực tiếp
                if (CurrencyController.Instance != null)
                {
                    CurrencyController.Instance.AddGold(reward.amount);
                }

                // pop up: bắn thông báo nhận
                if (ItemPickupUIController.Instance != null)
                {
                    ItemPickupUIController.Instance.ShowItemPickup($"+{reward.amount} Vàng", null);
                }
            }
            else if (reward.type == RewardType.Item)
            {
                if (itemDictionary == null)
                {
                    itemDictionary = FindAnyObjectByType<ItemDictionary>();
                }

                if (itemDictionary != null)
                {
                    // Trích xuất Prefab UI gốc từ Dictionary để lấy chuẩn xác Tên hiển thị và Sprite Icon
                    GameObject uiPrefab = itemDictionary.GetItemPrefab(reward.itemID);
                    if (uiPrefab != null)
                    {
                        Item itemScript = uiPrefab.GetComponent<Item>();
                        Image itemImage = uiPrefab.GetComponent<Image>();

                        string rewardName = (itemScript != null && !string.IsNullOrEmpty(itemScript.Name)) ? itemScript.Name : "Vật phẩm";
                        Sprite rewardIcon = itemImage != null ? itemImage.sprite : null;

                        // Tiến hành nạp vật phẩm thưởng vào hòm đồ
                        GameObject dummyRewardObj = new GameObject("DummyRewardItem");
                        Item dummyItem = dummyRewardObj.AddComponent<Item>();
                        dummyItem.ID = reward.itemID;
                        dummyItem.quantity = reward.amount;

                        if (InventoryController.Instance != null)
                        {
                            bool itemAdded = InventoryController.Instance.AddItem(dummyRewardObj);
                            if (itemAdded)
                            {
                                // pop up: bắn thông báo nhận
                                if (ItemPickupUIController.Instance != null)
                                {
                                    ItemPickupUIController.Instance.ShowItemPickup($"{rewardName} x{reward.amount}", rewardIcon);
                                }
                            }
                        }
                        Destroy(dummyRewardObj);
                    }
                }
            }
        }
    }

    // Kiểm tra quest được trả cho NPC chưa
    public bool IsQuestHandedIn(string questID) => handInQuestIDs.Contains(questID);

    // Kiểm tra Quest có đang làm không
    public bool IsQuestActive(string questID) => activateQuests.Exists(q => q.QuestID == questID);

    // Load tiến độ Quest
    public void LoadQuestProgress(List<QuestProgress> savedQuests)
    {
        activateQuests = savedQuests ?? new List<QuestProgress>();
        CheckInventoryForQuests();
        if (questUI != null) questUI.UpdateQuestUI();
    }
}