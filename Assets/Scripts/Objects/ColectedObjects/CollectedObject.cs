using System.Collections;
using UnityEngine;

public class CollectedObject : MonoBehaviour
{
    // Attribute
    public float HP = 2f;

    // Visual Setting
    public GameObject firstVisual;
    public GameObject lastVisual;

    // Shake Image
    public float shakeDuration = 0.15f;
    public float shakeAngle = 8f;
    private Coroutine currentShake;

    void Start()
    {
        UpdateVisuals();
    }

    public void GetDamage(float damage)
    {
        if(HP <= 0) return;
        HP --;
        Debug.Log("Bị khai thác cmnr, máu còn: " + HP);

        if(currentShake != null)
        {
            StopCoroutine(currentShake);
        }
        currentShake = StartCoroutine(ShakeTreeRoutine());

        UpdateVisuals();
        if(HP <= 0)
        {
            DropItem();
        }
    }

    public IEnumerator ShakeTreeRoutine()
    {
        Quaternion originalRotation = firstVisual.transform.rotation;
        float elapsedTime = 0;

        while(elapsedTime < shakeDuration)
        {
            float randomZ = Random.Range(-shakeAngle, shakeAngle);
            firstVisual.transform.rotation = Quaternion.Euler(0, 0, randomZ);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        firstVisual.transform.rotation = originalRotation;
    }

    public void UpdateVisuals()
    {
        if(HP > 0)
        {
            firstVisual.SetActive(true);
            lastVisual.SetActive(false);
        }
        else
        {
            firstVisual.SetActive(false);
            lastVisual.SetActive(true);
        }
    }

    public void DropItem()
    {
        Debug.Log("Rớt item nè cu");
    }

    public void ObjectCollected()
    {
        firstVisual.SetActive(false);
        gameObject.SetActive(false);
    }
}
