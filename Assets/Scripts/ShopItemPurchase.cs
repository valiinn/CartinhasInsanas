using UnityEngine;
using UnityEngine.EventSystems;

public class ShopItemPurchase : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    public Transform boardPanel;
    public PlayerStats playerStats;

    private ShopItem shopItem;
    private Card card; // para ler custo
    private Transform startParent; // s� para detectar se ficou no board

    void Awake()
    {
        shopItem = GetComponent<ShopItem>();
        card = GetComponent<Card>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // s� guardamos o parent de origem (loja ou n�o)
        startParent = transform.parent;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Depois que DropSlot j� processou o drop, verificamos onde a carta ficou
        bool nowOnBoard = boardPanel != null && transform.IsChildOf(boardPanel);
        int cost = (card != null) ? card.custo : 5;

        if (nowOnBoard)
        {
            // Se caiu no tabuleiro e ainda n�o foi comprada -> compra
            if (shopItem != null && !shopItem.bought)
            {
                if (playerStats != null && playerStats.gold >= cost)
                {
                    playerStats.SpendGold(cost);
                    shopItem.bought = true;
                }
                else
                {
                    // Sem ouro: desfaz (volta para onde estava)
                    transform.SetParent(startParent, false);
                    var rt = transform as RectTransform;
                    if (rt) { rt.anchoredPosition = Vector2.zero; }
                }
            }
        }
        else
        {
            // Voltou para fora do board: se estava comprada, reembolsa
            if (shopItem != null && shopItem.bought)
            {
                if (playerStats != null) playerStats.AddGold(cost);
                shopItem.bought = false;
            }
        }
    }
}
