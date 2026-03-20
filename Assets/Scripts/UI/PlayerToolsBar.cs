using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerToolsBar : MonoBehaviour
{
    public GameObject player;
    public List<Button> listButtonTools;
    private PlayerManager playerManager;
    private int idxChosen = -1;

    void Start()
    {
        playerManager = player.GetComponent<PlayerManager>();
    }

    void Update()
    {
        if(playerManager.isActing) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            playerManager.stateTools = 1;
            idxChosen = 0;
            UpdateToolButton(idxChosen);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            playerManager.stateTools = 2;
            idxChosen = 1;
            UpdateToolButton(idxChosen);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            playerManager.stateTools = 3;
            idxChosen = 2;
            UpdateToolButton(idxChosen);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            playerManager.stateTools = 4;
            idxChosen = 3;
            UpdateToolButton(idxChosen);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            playerManager.stateTools = 5;
            idxChosen = 4;
            UpdateToolButton(idxChosen);
        }
    }

    void UpdateToolButton(int idx)
    {
        for(int i = 0; i < listButtonTools.Count; i++)
        {
            RectTransform rt = listButtonTools[i].GetComponent<RectTransform>();
            Image img = listButtonTools[i].GetComponent<Image>();

            if(i == idx)
            {
                StartCoroutine(ScaleButton(rt, Vector3.one * 1.2f));
                img.color = Color.yellow;
            }
            else
            {
                StartCoroutine(ScaleButton(rt, Vector3.one));
                img.color = Color.white;
            }
        }
    }

    IEnumerator ScaleButton(RectTransform rt, Vector3 target)
    {
        float time = 0;
        Vector3 start = rt.localScale;
        while(time < 0.5f)
        {
            rt.localScale = Vector3.Lerp(start, target, time / 0.5f);
            time += Time.deltaTime;
            yield return null;
        }

        rt.localScale = target;
    }
}
