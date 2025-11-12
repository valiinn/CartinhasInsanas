using UnityEngine;
using UnityEngine.EventSystems;

public class BuffDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private RectTransform rt;

    private Transform originalParent;
    private Vector3 originalScale;

    private Canvas dragCanvas;
    private RectTransform dragCanvasRT;

    private BuffType buffType;

    public void Setup(BuffType type)
    {
        buffType = type;
    }

    private void Awake()
    {
        rt = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    // ===========================================================
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalScale = transform.localScale;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;

        dragCanvas = GameObject.Find("DraggingLayer")?.GetComponent<Canvas>();
        if (dragCanvas == null)
        {
            Debug.LogError("⚠ Não existe um Canvas chamado 'DraggingLayer'!");
            return;
        }

        dragCanvasRT = dragCanvas.GetComponent<RectTransform>();

        transform.SetParent(dragCanvas.transform, false);
        rt.localScale = Vector3.one * 0.5f; // zoom durante drag
    }

    // ===========================================================
    public void OnDrag(PointerEventData eventData)
    {
        if (dragCanvasRT == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragCanvasRT,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 pos))
        {
            rt.anchoredPosition = pos;
        }
    }

    // ===========================================================
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        CardCombat card = null;
        if (eventData.pointerEnter != null)
            card = eventData.pointerEnter.GetComponentInParent<CardCombat>();

        if (card != null)
        {
            AttachToCard(card);
            return;
        }

        // volta pro painel
        transform.SetParent(originalParent, false);
        rt.localScale = originalScale;
    }

    // ===========================================================
    private void AttachToCard(CardCombat card)
    {
        BuffSystem bs = card.GetComponent<BuffSystem>();
        if (bs == null)
            bs = card.gameObject.AddComponent<BuffSystem>();

        bs.ApplyBuff(buffType);

        transform.SetParent(card.transform, false);

        RectTransform buffRT = GetComponent<RectTransform>();

        // Conta quantos buffs já existem
        int buffIndex = 0;
        foreach (Transform child in card.transform)
        {
            if (child.GetComponent<BuffDraggable>())
                buffIndex++;
        }

        // 🔥 OFFSET DE STACK (cada buff desloca um pouquinho pro lado)
        float offsetX = 4f + (buffIndex * 22f); // ajuste fino
        float offsetY = 4f;

        buffRT.anchorMin = new Vector2(0f, 0f);
        buffRT.anchorMax = new Vector2(0f, 0f);
        buffRT.pivot = new Vector2(0f, 0f);

        buffRT.anchoredPosition = new Vector2(offsetX, offsetY);

        // 🔥 Tamanho pequeno
        buffRT.localScale = new Vector3(0.14f, 0.14f, 1f);

        canvasGroup.alpha = 1f;

        buffRT.SetAsLastSibling();
    }
}
