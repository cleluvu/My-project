using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueChoiceButton : MonoBehaviour
{
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private Button button;

    private int choiceIndex;
    private Action<int> onClick;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
        if (labelText == null)
            labelText = GetComponentInChildren<TMP_Text>();

        if (button != null)
            button.onClick.AddListener(() => onClick?.Invoke(choiceIndex));
    }

    public void Setup(string label, int index, Action<int> onClickCallback)
    {
        choiceIndex = index;
        onClick = onClickCallback;
        if (labelText != null)
            labelText.text = label;
    }
}
