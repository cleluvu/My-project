using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class NPC : MonoBehaviour, IInteractable
{
    [Header("Data")]
    public NPCDialogue dialogueData;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text nameText;
    public Image portraitImage;

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;
    private bool hasValidUIReferences;

    private void Awake()
    {
        hasValidUIReferences = dialoguePanel != null && dialogueText != null && nameText != null && portraitImage != null;
        // Only report missing dialogue UI when this NPC is actually configured for dialogue.
        if (!hasValidUIReferences && dialogueData != null)
        {
            Debug.LogError($"[{nameof(NPC)}] Missing UI references on {gameObject.name}. Please assign Dialogue Panel/Text/Name/Portrait in Inspector.");
        }
    }

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        if (!hasValidUIReferences || dialogueData == null || dialogueData.dialogueLines == null || dialogueData.dialogueLines.Length == 0)
        {
            return;
        }

        if (PauseController.IsGamePause && !isDialogueActive)
            return;

        if (isDialogueActive)
        {
            NextLine();
        }
        else
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        if (!hasValidUIReferences)
        {
            return;
        }

        isDialogueActive = true;
        dialogueIndex = 0;

        nameText.SetText(dialogueData.npcName);
        portraitImage.sprite = dialogueData.npcPortrait;

        dialoguePanel.SetActive(true);
        PauseController.SetPause(true); 

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (!hasValidUIReferences || dialogueData == null || dialogueData.dialogueLines == null || dialogueData.dialogueLines.Length == 0)
        {
            EndDialogue();
            return;
        }

        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        if (!hasValidUIReferences || dialogueData == null || dialogueData.dialogueLines == null || dialogueData.dialogueLines.Length == 0)
        {
            yield break;
        }

        isTyping = true;
        dialogueText.text = dialogueData.dialogueLines[dialogueIndex];
        dialogueText.maxVisibleCharacters = 0;

        for (int i = 0; i <= dialogueText.text.Length; i++)
        {
            dialogueText.maxVisibleCharacters = i;

            yield return new WaitForSecondsRealtime(dialogueData.typingSpeed);
        }

        isTyping = false;

        if (dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSecondsRealtime(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;

        if (dialogueText != null)
        {
            dialogueText.SetText("");
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        PauseController.SetPause(false); 
    }
}