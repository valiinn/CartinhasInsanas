using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using System.Collections;

public class NotificationUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Refs")]
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Image backgroundImage;

    [Header("Timing")]
    public float fadeDuration = 0.18f;

    Action onClick;
    Coroutine lifeCoroutine;

    void Reset()
    {
        // tentativa de auto-fill para facilitar durante edição
        canvasGroup = GetComponent<CanvasGroup>();
        if (titleText == null) titleText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Setup(string title, string message, float duration, NotificationType type = NotificationType.Info, Action onClick = null)
    {
        this.onClick = onClick;
        if (titleText != null) titleText.text = title ?? "";
        if (messageText != null) messageText.text = message ?? "";

        if (backgroundImage != null)
            backgroundImage.color = GetColorForType(type);

        // restart lifecycle
        if (lifeCoroutine != null) StopCoroutine(lifeCoroutine);
        lifeCoroutine = StartCoroutine(LifeRoutine(duration));
    }

    Color GetColorForType(NotificationType type)
    {
        switch (type)
        {
            case NotificationType.Success: return new Color(0.18f, 0.8f, 0.36f, 1f);
            case NotificationType.Warning: return new Color(0.95f, 0.76f, 0.2f, 1f);
            case NotificationType.Error:   return new Color(0.85f, 0.2f, 0.2f, 1f);
            default:                       return new Color(0.12f, 0.56f, 0.86f, 1f); // Info
        }
    }

    IEnumerator LifeRoutine(float visibleTime)
    {
        // fade in
        canvasGroup.alpha = 0f;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // permanecer no tempo (use WaitForSecondsRealtime para ignorar Time.timeScale)
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, visibleTime));

        // fade out
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        // devolve ao pool
        NotificationManager.Instance?.ReturnToPool(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // callback opcional ao clicar
        onClick?.Invoke();
        // fecha imediatamente
        HideNow();
    }

    public void HideNow()
    {
        if (lifeCoroutine != null) StopCoroutine(lifeCoroutine);
        StartCoroutine(HideImmediate());
    }

    IEnumerator HideImmediate()
    {
        float t = 0f;
        float start = canvasGroup.alpha;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, 0f, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        NotificationManager.Instance?.ReturnToPool(this);
    }
}
