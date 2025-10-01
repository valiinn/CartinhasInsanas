using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("Referências")]
    public Transform shopPanel;
    public Transform boardPanel;
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

            // Garante que tem CanvasGroup (Card usa para blocksRaycasts)
            var cg = cardInstance.GetComponent<CanvasGroup>();
            if (cg == null) cg = cardInstance.AddComponent<CanvasGroup>();

            // Garante que tem Card (drag real) e ShopItem (estado) e ShopItemPurchase (ouro)
            var card = cardInstance.GetComponent<Card>();
            if (card == null)
            {
                Debug.LogError("O prefab de carta precisa ter o componente 'Card'.");
            }

            var shopItem = cardInstance.GetComponent<ShopItem>();
            if (shopItem == null) shopItem = cardInstance.AddComponent<ShopItem>();

            var purchase = cardInstance.GetComponent<ShopItemPurchase>();
            if (purchase == null) purchase = cardInstance.AddComponent<ShopItemPurchase>();
            purchase.boardPanel = boardPanel;
            purchase.playerStats = playerStats;

            // Nada de EventTrigger aqui! Quem cuida do drag é o Card.cs
        }
    }
}
