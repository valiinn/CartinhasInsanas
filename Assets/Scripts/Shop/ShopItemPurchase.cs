using UnityEngine;
using UnityEngine.EventSystems;

public class ShopItemPurchase : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    public Transform boardPanel;
    public PlayerStats playerStats; // ✅ agora existe de novo

    private ShopItem shopItem;
    private Card card; // para ler custo
    private Transform startParent; // só para detectar se ficou no board

    void Awake()
    {
        shopItem = GetComponent<ShopItem>();
        card = GetComponent<Card>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Só guardamos o parent de origem (loja ou não)
        startParent = transform.parent;

        // ✅ Compra já ao iniciar o drag, se ainda não foi comprada
        int cost = (card != null) ? card.custo : 5;
        if (shopItem != null && !shopItem.bought)
        {
            if (playerStats != null && playerStats.gold >= cost)
            {
                playerStats.SpendGold(cost);
                shopItem.bought = true;
            }
            else
            {
                // Sem ouro -> não deixa arrastar
                eventData.pointerDrag = null;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        bool nowOnBoard = boardPanel != null && transform.IsChildOf(boardPanel);
        int cost = (card != null) ? card.custo : 5;

        if (!nowOnBoard)
        {
            // Voltou para fora do board → reembolsa e marca como não comprada
            if (shopItem != null && shopItem.bought)
            {
                if (playerStats != null) playerStats.AddGold(cost);
                shopItem.bought = false;
            }

            // Volta para posição inicial
            transform.SetParent(startParent, false);
            var rt = transform as RectTransform;
            if (rt) rt.anchoredPosition = Vector2.zero;
        }
    }
}
