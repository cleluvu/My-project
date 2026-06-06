using System;
using UnityEngine;

[Serializable]
public class DialogueChoice
{
    [Tooltip("Ở dòng dialogueLines nào (0, 1, 2...) thì hiện các nút choice.")]
    public int dialogueIndex;
    public string[] choices;
    [Tooltip("Mỗi phần tử = index trong dialogueLines sau khi chọn. -1 = kết thúc.")]
    public int[] nextDialogueIndexes;
    [Tooltip("Quest đưa khi chọn nút đầu tiên (index 0), nếu không dùng Give Quests.")]
    public Quest giveQuest;
    [Tooltip("Cùng Size với Choices — quest theo từng nút (ưu tiên hơn Give Quest).")]
    public bool[] giveQuests;
}

[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPC Dialogue")]
public class NPCDialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcPortrait;
    public string[] dialogueLines;
    public bool[] autoProgressLines;
    public bool[] endDialogueLines;
    public float autoProgressDelay = 1.5f;
    public float typingSpeed = 0.05f;
    public AudioClip voiceSound;
    public float voicePitch = 1f;
    public DialogueChoice[] choices;

    public int questInProgressIndex;
    public int questCompletedIndex;
    public Quest quest;

#if UNITY_EDITOR
    private void OnValidate()
    {
        int lineCount = dialogueLines?.Length ?? 0;
        if (choices == null) return;

        foreach (DialogueChoice branch in choices)
        {
            if (branch == null) continue;

            if (branch.dialogueIndex < 0 || branch.dialogueIndex >= lineCount)
            {
                Debug.LogWarning(
                    $"[{name}] Choice dialogueIndex {branch.dialogueIndex} ngoài phạm vi dialogueLines (0..{lineCount - 1}).",
                    this);
                continue;
            }

            int labelCount = branch.choices?.Length ?? 0;
            int nextCount = branch.nextDialogueIndexes?.Length ?? 0;
            if (labelCount != nextCount)
            {
                Debug.LogWarning(
                    $"[{name}] Dòng {branch.dialogueIndex}: choices.Length ({labelCount}) != nextDialogueIndexes.Length ({nextCount}).",
                    this);
            }

            int questCount = branch.giveQuests?.Length ?? 0;
            if (questCount > 0 && questCount != labelCount)
            {
                Debug.LogWarning(
                    $"[{name}] Dòng {branch.dialogueIndex}: giveQuests.Length ({questCount}) != choices.Length ({labelCount}).",
                    this);
            }

            if (branch.nextDialogueIndexes == null) continue;
            for (int i = 0; i < branch.nextDialogueIndexes.Length; i++)
            {
                int next = branch.nextDialogueIndexes[i];
                if (next < 0) continue;
                if (next >= lineCount)
                {
                    Debug.LogWarning(
                        $"[{name}] Dòng {branch.dialogueIndex}, choice {i}: nextDialogueIndexes = {next} nhưng dialogueLines chỉ có {lineCount} dòng.",
                        this);
                }
            }
        }
    }
#endif
}
