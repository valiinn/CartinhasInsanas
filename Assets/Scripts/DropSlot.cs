using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // se quiser ler cellSize do Grid

[RequireComponent(typeof(RectTransform))]
public class DropSlot : MonoBehaviour, IDropHandler
{
    [Tooltip("Se true, a carta estica para o tamanho EXATO do slot.")]
    public bool fillParent = true;

    [Tooltip("Padding interno opcional (em pixels) quando fillParent = true.")]
    public Vector2 padding = Vector2.zero;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        RectTransform card = eventData.pointerDrag.GetComponent<RectTransform>();
        RectTransform slot = (RectTransform)transform;

        // Reparent sem manter posição de mundo (reseta local pos/scale/rot corretamente)
        card.SetParent(slot, false);

        // Garantir transform limpo
        card.localScale = Vector3.one;
        card.localRotation = Quaternion.identity;
        card.pivot = new Vector2(0.5f, 0.5f);

        if (fillParent)
        {
            // Faz a carta preencher o slot exatamente
            card.anchorMin = Vector2.zero;
            card.anchorMax = Vector2.one;
            card.offsetMin = new Vector2(padding.x, padding.y);
            card.offsetMax = new Vector2(-padding.x, -padding.y);
            card.anchoredPosition = Vector2.zero; // centraliza (com os offsets aplicados)
        }
        else
        {
            // Alternativa: usar o cellSize do Grid (se preferir)
            var grid = slot.parent ? slot.parent.GetComponent<GridLayoutGroup>() : null;
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.anchoredPosition = Vector2.zero;
            card.sizeDelta = grid ? grid.cellSize : slot.rect.size;
        }
    }
}
