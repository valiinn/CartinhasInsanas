using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // Necessário para GridLayoutGroup

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
        // 1) valida carta arrastada
        if (eventData.pointerDrag == null) return;

        var draggedGo = eventData.pointerDrag;
        var draggedCard = draggedGo.GetComponent<Card>();
        if (draggedCard == null) return;

        var slotRT = (RectTransform)transform;
        var draggedRT = draggedGo.GetComponent<RectTransform>();

        // 2) se já está no mesmo slot, não faz nada
        if (draggedRT.parent == slotRT) return;

        // 3) encontra a carta que já ocupa este slot (em profundidade), ignorando a "dragged"
        var occupyingRT = FindOccupyingCardRT(slotRT, draggedGo);

        // 4) se tem ocupante
        if (occupyingRT != null)
        {
            if (!allowSwap)
            {
                // recusa swap -> OnEndDrag do Card devolve
                return;
            }

            // destino do ocupante é de onde a dragged saiu
            Transform targetParent = draggedCard.OriginalParent;
            if (targetParent == null) return;

            // move ocupante para o parent original da dragged
            occupyingRT.SetParent(targetParent, false);
            AjustarLayout(occupyingRT, targetParent as RectTransform);
        }

        // 5) coloca a dragged neste slot
        draggedRT.SetParent(slotRT, false);
        AjustarLayout(draggedRT, slotRT);
    }

    // Procura por profundidade um descendente do slot que tenha "Card",
    // ignorando "exclude" (a própria carta que está sendo arrastada).
    private RectTransform FindOccupyingCardRT(Transform slot, GameObject exclude)
    {
        // primeiro passa rápida pelos filhos diretos
        for (int i = 0; i < slot.childCount; i++)
        {
            var ch = slot.GetChild(i);
            if (ch == null || ch.gameObject == exclude) continue;
            if (ch.GetComponent<Card>() != null) return ch as RectTransform;
        }
        // depois procura em profundidade
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

        // se o parent é um DropSlot e fillParent = true, preenche com padding
        var parentSlot = parentRect ? parentRect.GetComponent<DropSlot>() : null;
        bool shouldFill = parentSlot != null && parentSlot.fillParent;

        if (shouldFill)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(parentSlot.padding.x, parentSlot.padding.y);
            rect.offsetMax = new Vector2(-parentSlot.padding.x, -parentSlot.padding.y);
            rect.anchoredPosition = Vector2.zero;
        }
        else
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            var grid = parentRect && parentRect.parent
                ? parentRect.parent.GetComponent<GridLayoutGroup>()
                : null;

            rect.sizeDelta = grid ? grid.cellSize : parentRect.rect.size;
        }
    }
}
