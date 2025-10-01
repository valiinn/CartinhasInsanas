using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("Referências")]
    public Transform shopPanel;
    public Transform boardPanel;
    public Transform handPanel;      // << novo, arraste no Inspector
    public PlayerStats playerStats;
    public Button refreshButton;
    public int cardsPerRefresh = 5;

    [Header("Cartas disponíveis")]
    public List<GameObject> availableCards;

    void Start()
    {
        refreshButton.onClick.AddListener(RefreshShop);
        RefreshShop();
    }

    public void RefreshShop()
    {
        // Limpa a loja
        foreach (Transform child in shopPanel)
            Destroy(child.gameObject);

        // Cria novas cartas
        for (int i = 0; i < cardsPerRefresh; i++)
        {
            if (availableCards.Count == 0) return;

            GameObject cardPrefab = availableCards[Random.Range(0, availableCards.Count)];
            GameObject cardInstance = Instantiate(cardPrefab, shopPanel);

            // Garante que tem CanvasGroup
            var cg = cardInstance.GetComponent<CanvasGroup>();
            if (cg == null) cg = cardInstance.AddComponent<CanvasGroup>();

            // Configura Card
            var card = cardInstance.GetComponent<Card>();
            if (card != null)
            {
                card.shopPanel = shopPanel;
                card.handPanel = handPanel;
            }
            else
            {
                Debug.LogError("O prefab de carta precisa ter o componente 'Card'.");
            }

            // Garante que tem ShopItem e ShopItemPurchase
            var shopItem = cardInstance.GetComponent<ShopItem>();
            if (shopItem == null) shopItem = cardInstance.AddComponent<ShopItem>();

            var purchase = cardInstance.GetComponent<ShopItemPurchase>();
            if (purchase == null) purchase = cardInstance.AddComponent<ShopItemPurchase>();
            purchase.boardPanel = boardPanel;
            purchase.playerStats = playerStats;
        }
    }
}
