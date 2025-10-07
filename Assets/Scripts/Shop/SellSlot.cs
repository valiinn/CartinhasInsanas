using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SellSlot : MonoBehaviour, IDropHandler
{
    [Tooltip("Painel da loja (onde a carta deve ficar). Se vazio, usa este Transform.")]
    public Transform targetPanel;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        // Pegue o ROOT da carta (tem componente Card)
        var card = eventData.pointerDrag.GetComponentInParent<Card>();
        if (card == null) return;

        var rt = card.transform as RectTransform;
        var dest = (targetPanel != null) ? targetPanel : transform;

        // Reparenta para a loja
        rt.SetParent(dest, false);

        // Layout básico: GridLayoutGroup dita o tamanho; só centralizamos
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        // Opcional: garantir tamanho = célula do Grid
        var grid = (dest as RectTransform)?.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            rt.sizeDelta = grid.cellSize;
        }
    }
}
