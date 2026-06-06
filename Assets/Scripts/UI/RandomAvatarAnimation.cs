using UnityEngine;
using System.Collections;

public class RandomAvatarAnimation : MonoBehaviour
{
    private Animator anim;
    
    // Tên animation đứng yên
    public string idleAnimation = "Avatar0";
    
    // Danh sách các animation biểu cảm (1 đến 5)
    public string[] emoteAnimations = { "Avatar1", "Avatar2", "Avatar3", "Avatar4", "Avatar5", "Avatar6", "Avatar7" };

    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogError("Chưa có Animator trên GameObject này!");
            return;
        }
        
        // Bắt đầu vòng lặp
        StartCoroutine(AvatarLogicRoutine());
    }

    IEnumerator AvatarLogicRoutine()
    {
        while (true)
        {
            // 1. Chạy animation tĩnh (Avatar0)
            anim.Play(idleAnimation);
            Debug.Log("Đang ở trạng thái tĩnh (Avatar0)");

            // 2. Đứng yên trong khoảng 3 giây
            yield return new WaitForSeconds(3f);

            // 3. Chọn ngẫu nhiên 1 animation biểu cảm từ mảng emoteAnimations
            if (emoteAnimations.Length > 0)
            {
                string randomEmote = emoteAnimations[Random.Range(0, emoteAnimations.Length)];
                anim.Play(randomEmote);
                Debug.Log("Đang biểu cảm: " + randomEmote);

                yield return new WaitForSeconds(3f);
            }
        }
    }
}