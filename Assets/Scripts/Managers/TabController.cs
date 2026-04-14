using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    public Image[] tabImages;
    public GameObject[] pages;

    void Start()
    {
        ActiveTab(0);
    }

    public void ActiveTab(int tabNo)
    {
        for(int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
            tabImages[i].color = Color.gray;
        }
        pages[tabNo].SetActive(true);
        tabImages[tabNo].color = Color.white;
    }
}
