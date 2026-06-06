using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text nameText;
    public Image portraitImage;

    [Header("Choice UI (optional)")]
    public GameObject choicePanel;
    [Tooltip("Kéo GameObject nút (ChoiceButton1, …). Có thể gán DialogueChoiceButton hoặc để trống — runtime tự thêm.")]
    public GameObject[] choiceButtons;

    public bool IsShowingChoices { get; private set; }

    private Action<int> onChoiceSelected;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        HideChoices();
    }

    public void ShowDialogue(bool show)
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(show);
    }

    public void SetNPCInfo(string npcName, Sprite portrait)
    {
        if (nameText != null)
            nameText.text = npcName;
        if (portraitImage != null)
            portraitImage.sprite = portrait;
    }

    public void ClearDialogue()
    {
        if (dialogueText != null)
            dialogueText.SetText("");
        HideChoices();
    }

    public void PrepareLineForTyping(string line)
    {
        if (dialogueText == null) return;
        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();
    }

    public void RevealFullLine(string line)
    {
        if (dialogueText == null) return;
        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = line.Length;
        dialogueText.ForceMeshUpdate();
    }

    public void SetVisibleCharacterCount(int count)
    {
        if (dialogueText != null)
            dialogueText.maxVisibleCharacters = count;
    }

    public IEnumerator PlayTypeLine(string line, float typingSpeed)
    {
        if (dialogueText == null || string.IsNullOrEmpty(line))
            yield break;

        PrepareLineForTyping(line);

        for (int i = 0; i <= line.Length; i++)
        {
            SetVisibleCharacterCount(i);
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
    }

    public bool CanShowChoices => choicePanel != null && choiceButtons != null && choiceButtons.Length > 0;

    public bool ShowChoices(string[] labels, Action<int> onSelected)
    {
        if (labels == null || labels.Length == 0)
            return false;

        if (!CanShowChoices)
        {
            Debug.LogWarning("[DialogueController] Có choice trong data nhưng chưa gán Choice Panel / Choice Buttons trên Inspector.");
            return false;
        }

        HideChoices();
        IsShowingChoices = true;
        onChoiceSelected = onSelected;

        choicePanel.SetActive(true);
        int count = Mathf.Min(labels.Length, choiceButtons.Length);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            bool active = i < count;
            GameObject root = choiceButtons[i];
            if (root == null) continue;

            root.SetActive(active);
            if (!active) continue;

            DialogueChoiceButton choiceBtn = GetOrAddChoiceButton(root);
            if (choiceBtn != null)
                choiceBtn.Setup(labels[i], i, OnChoiceButtonClicked);
        }

        return true;
    }

    public void HideChoices()
    {
        IsShowingChoices = false;
        onChoiceSelected = null;

        if (choicePanel != null)
            choicePanel.SetActive(false);

        if (choiceButtons == null) return;
        foreach (GameObject root in choiceButtons)
        {
            if (root != null)
                root.SetActive(false);
        }
    }

    private static DialogueChoiceButton GetOrAddChoiceButton(GameObject root)
    {
        if (root == null) return null;
        if (!root.TryGetComponent(out DialogueChoiceButton choiceBtn))
            choiceBtn = root.AddComponent<DialogueChoiceButton>();
        return choiceBtn;
    }

    private void OnChoiceButtonClicked(int index)
    {
        var callback = onChoiceSelected;
        HideChoices();
        callback?.Invoke(index);
    }
}
