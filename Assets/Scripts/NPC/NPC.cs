using UnityEngine;
using System.Collections;

public class NPC : MonoBehaviour, IInteractable
{
    [Header("Data")]
    public NPCDialogue dialogueData;

    private int dialogueIndex;
    private DialogueController dialogueUI;
    private bool isTyping;
    private bool isDialogueActive;
    private bool waitingForChoice;

    private enum QuestState
    {
        NotStarted,
        InProgress,
        Completed
    }
    private QuestState questState = QuestState.NotStarted;

    private void Start()
    {
        dialogueUI = DialogueController.Instance;
        if (dialogueUI == null)
            Debug.LogError($"[{nameof(NPC)}] Không tìm thấy DialogueController trong scene.");
    }

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        if (!CanStartDialogue())
            return;

        if (PauseController.IsGamePause && !isDialogueActive)
            return;

        if (waitingForChoice)
            return;

        if (isDialogueActive)
            NextLine();
        else
            StartDialogue();
    }

    private bool CanStartDialogue()
    {
        return dialogueUI != null
            && dialogueData != null
            && dialogueData.dialogueLines != null
            && dialogueData.dialogueLines.Length > 0;
    }

    void StartDialogue()
    {
        // Đồng bộ trạng thái Quest trước khi mở hội thoại
        SyncQuestState();

        if (questState == QuestState.NotStarted)
        {
            dialogueIndex = 0;
        }
        else if (questState == QuestState.InProgress)
        {
            dialogueIndex = dialogueData.questInProgressIndex;
        }
        else if (questState == QuestState.Completed)
        {
            dialogueIndex = dialogueData.questCompletedIndex;
        }
        
        if (!CanStartDialogue()) return;

        isDialogueActive = true;
        waitingForChoice = false;

        dialogueUI.SetNPCInfo(dialogueData.npcName, dialogueData.npcPortrait);
        dialogueUI.ShowDialogue(true);
        dialogueUI.HideChoices();
        PauseController.SetPause(true);

        StartCoroutine(TypeLine());
    }

    private void SyncQuestState()
    {
        if (dialogueData.quest == null || QuestController.Instance == null) return;

        string questID = dialogueData.quest.questID;

        if (QuestController.Instance.IsQuestHandedIn(questID) || QuestController.Instance.IsQuestCompleted(questID))
        {
            questState = QuestState.Completed;
        }
        else if (QuestController.Instance.IsQuestActive(questID))
        {
            questState = QuestState.InProgress;
        }
        else
        {
            questState = QuestState.NotStarted;
        }
    }

    void NextLine()
    {
        if (!CanStartDialogue())
        {
            EndDialogue();
            return;
        }

        if (isTyping)
        {
            StopAllCoroutines();
            dialogueUI.RevealFullLine(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
            return;
        }

        if (IsEndLine(dialogueIndex))
        {
            EndDialogue();
            return;
        }

        dialogueIndex++;
        if (dialogueIndex < dialogueData.dialogueLines.Length)
            StartCoroutine(TypeLine());
        else
            EndDialogue();
    }

    IEnumerator TypeLine()
    {
        if (!CanStartDialogue())
            yield break;

        isTyping = true;
        string line = dialogueData.dialogueLines[dialogueIndex];

        yield return dialogueUI.PlayTypeLine(line, dialogueData.typingSpeed);

        isTyping = false;

        if (TryShowChoicesForCurrentLine())
            yield break;

        if (IsAutoProgress(dialogueIndex))
        {
            yield return new WaitForSecondsRealtime(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    private bool TryShowChoicesForCurrentLine()
    {
        DialogueChoice branch = FindChoiceAt(dialogueIndex);
        if (branch?.choices == null || branch.choices.Length == 0)
            return false;

        waitingForChoice = true;
        if (!dialogueUI.ShowChoices(branch.choices, OnChoiceSelected))
        {
            waitingForChoice = false;
            Debug.LogWarning($"[{nameof(NPC)}] Không hiển thị được choice — gán Choice Panel trên DialogueController.");
            return false;
        }
        return true;
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        waitingForChoice = false;

        DialogueChoice branch = FindChoiceAt(dialogueIndex);
        if (branch?.nextDialogueIndexes == null
            || choiceIndex < 0
            || choiceIndex >= branch.nextDialogueIndexes.Length)
        {
            EndDialogue();
            return;
        }

        bool givesQuest = false;
        if (branch.giveQuests != null && choiceIndex < branch.giveQuests.Length)
        {
            givesQuest = branch.giveQuests[choiceIndex]; 
        }
        else if (choiceIndex == 0 && branch.giveQuest != null)
        {
            givesQuest = true;
        }

        TryGrantQuestFromChoice(givesQuest);

        int nextIndex = branch.nextDialogueIndexes[choiceIndex];
        if (nextIndex < 0)
        {
            EndDialogue();
            return;
        }

        if (nextIndex >= dialogueData.dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        dialogueIndex = nextIndex;
        if (dialogueUI != null) dialogueUI.HideChoices(); 

        StartCoroutine(TypeLine());
    }

    private void TryGrantQuestFromChoice(bool givesQuest)
    {
        if (givesQuest && dialogueData.quest != null && QuestController.Instance != null)
        {
            QuestController.Instance.AcceptQuest(dialogueData.quest);
            questState = QuestState.InProgress;
        }
    }

    private DialogueChoice FindChoiceAt(int index)
    {
        if (dialogueData.choices == null) return null;
        foreach (DialogueChoice branch in dialogueData.choices)
        {
            if (branch != null && branch.dialogueIndex == index)
                return branch;
        }
        return null;
    }

    private bool IsAutoProgress(int index)
    {
        return dialogueData.autoProgressLines != null
            && index < dialogueData.autoProgressLines.Length
            && dialogueData.autoProgressLines[index];
    }

    private bool IsEndLine(int index)
    {
        return dialogueData.endDialogueLines != null
            && index < dialogueData.endDialogueLines.Length
            && dialogueData.endDialogueLines[index];
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        isTyping = false;
        waitingForChoice = false;

        if (dialogueUI != null)
        {
            dialogueUI.ClearDialogue();
            dialogueUI.ShowDialogue(false);
        }

        if (dialogueData != null && dialogueData.quest != null && QuestController.Instance != null)
        {
            string questID = dialogueData.quest.questID;
            
            // Nếu trạng thái Quest đã xong nhưng chưa được trả
            if (QuestController.Instance.IsQuestCompleted(questID) && !QuestController.Instance.IsQuestHandedIn(questID))
            {
                QuestController.Instance.HandInQuest(questID);
                questState = QuestState.Completed; // Đóng băng trạng thái NPC
            }
        }

        PauseController.SetPause(false);
    }
}