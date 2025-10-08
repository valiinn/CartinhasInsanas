using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class SellSlot : MonoBehaviour, IDropHandler
{
    [Tooltip("Se true, devolve o ouro quando a carta é vendida.")]
    public bool refundOnDelete = true;

    public PlayerStats playerStats;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        // Pega SEMPRE o ROOT que tem Card (não um filho da arte)
        var card = eventData.pointerDrag.GetComponentInParent<Card>();
        if (card == null) return;

        // Reembolso (se a carta foi comprada)
        if (refundOnDelete && playerStats != null)
        {
            var shopItem = card.GetComponent<ShopItem>(); // flag bought
            if (shopItem != null && shopItem.bought)
            {
                playerStats.AddGold(card.custo);
                shopItem.bought = false;
            }
        }

        // Destroi a carta inteira (root) para sumir do cenário
        Destroy(card.gameObject);
    }
}
