using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class FadeOutAndDestroy : NetworkBehaviour
{
    [SerializeField]
    float fadeOutTime = 3f;

    [SerializeField]
    float cooldownToFadeOutBegin = 5f;

    [SerializeField]
    SpriteRenderer fadeSprite;

    private float totalTime = 0f;

    private bool fadeStarted = false;

    // Update is called once per frame
    void Update()
    {
        totalTime += Time.deltaTime;
        if(totalTime < cooldownToFadeOutBegin)
        {
            return;
        }
        if (!fadeStarted)
        {
            fadeStarted = true;
            StartCoroutine(FadeTo(0, fadeOutTime, this.gameObject));
        }
    }

    IEnumerator FadeTo(float aValue, float aTime, GameObject trackImprints)
    {
        float alpha = fadeSprite.material.color.a;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
        {
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha, aValue, t));
            fadeSprite.material.color = newColor;
            yield return null;
        }
        Destroy(trackImprints);
    }
}
