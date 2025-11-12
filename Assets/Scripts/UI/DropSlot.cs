using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DropSlot : MonoBehaviour, IDropHandler
{
    [Header("Layout")]
    [Tooltip("Se true, a carta preenche todo o slot (anchors 0..1).")]
    public bool fillParent = true;

    [Tooltip("Padding interno (pixels) quando fillParent = true.")]
    public Vector2 padding = Vector2.zero;

    [Header("Comportamento")]
    [Tooltip("Se true, quando o slot estiver ocupado, faz troca (swap). Se false, recusa.")]
    public bool allowSwap = true;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        var draggedGo = eventData.pointerDrag;
        var draggedCard = draggedGo.GetComponent<Card>();
        if (draggedCard == null) return;

        var slotRT = (RectTransform)transform;
        var draggedRT = draggedGo.GetComponent<RectTransform>();

        // Se já está no mesmo slot, nada a fazer
        if (draggedRT.parent == slotRT) return;

        // Procura ocupante atual (outra carta neste slot)
        var occupyingRT = FindOccupyingCardRT(slotRT, draggedGo);

        if (occupyingRT != null)
        {
            if (!allowSwap)
            {
                // se não pode trocar, OnEndDrag do Card deve devolver
                return;
            }

            // destino do ocupante é de onde a dragged saiu
            Transform targetParent = draggedCard.OriginalParent;
            if (targetParent == null) return;

            occupyingRT.SetParent(targetParent, false);
            AjustarLayout(occupyingRT, targetParent as RectTransform);
        }

        // move carta arrastada para este slot
        draggedRT.SetParent(slotRT, false);
        AjustarLayout(draggedRT, slotRT);

        // atualiza referência no Card
        draggedCard.SetOriginalParent(slotRT); // ✅
    }

    private RectTransform FindOccupyingCardRT(Transform slot, GameObject exclude)
    {
        for (int i = 0; i < slot.childCount; i++)
        {
            var ch = slot.GetChild(i);
            if (ch == null || ch.gameObject == exclude) continue;
            if (ch.GetComponent<Card>() != null) return ch as RectTransform;
        }

        var cards = slot.GetComponentsInChildren<Card>(true);
        foreach (var c in cards)
        {
            if (c == null) continue;
            if (c.gameObject == exclude) continue;
            if (c.transform.IsChildOf(slot)) return c.transform as RectTransform;
        }
        return null;
    }

    private void AjustarLayout(RectTransform rect, RectTransform parentRect)
    {
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.pivot = new Vector2(0.5f, 0.5f);

        var parentSlot = parentRect ? parentRect.GetComponent<DropSlot>() : null;
        bool shouldFill = parentSlot != null && parentSlot.fillParent;

        if (shouldFill)
        {
            // Carta ocupa todo o slot
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(parentSlot.padding.x, parentSlot.padding.y);
            rect.offsetMax = new Vector2(-parentSlot.padding.x, -parentSlot.padding.y);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        }
        else
        {
            // Parent comum (mão, loja, etc)
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;

            // Se o pai usa GridLayoutGroup ou LayoutGroup, deixa o layout controlar o tamanho
            var layout = parentRect.GetComponent<LayoutGroup>();
            if (layout == null)
            {
                // Define tamanho padrão do card (sem layout)
                rect.sizeDelta = new Vector2(75, 90); // ou o tamanho padrão do seu prefab
            }
        }
    }

}