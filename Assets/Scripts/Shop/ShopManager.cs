using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("Painéis da cena")]
    public Transform shopPanel;          // painel da loja (GridLayoutGroup)
    public Transform handPanel;          // painel da mão (Grid/Horizontal/Vertical Layout)
    public Transform draggingLayer;      // geralmente o Canvas raiz
    public Transform previewLayer;       // opcional (pode ser igual ao draggingLayer)

    [Header("Economia")]
    public PlayerStats playerStats;
    public int maxHandSize = 7;

    [Header("UI")]
    public Button refreshButton;
    public int cardsPerRefresh = 5;

    [Header("Prefabs de carta disponíveis")]
    public List<GameObject> availableCards = new List<GameObject>();

    void Start()
    {
        // Fallbacks leves (apenas se não foram setados no Inspector)
        if (draggingLayer == null) draggingLayer = GetCanvasRootTransform();
        if (previewLayer == null) previewLayer = draggingLayer;
        if (playerStats == null) playerStats = GetFirstPlayerStats();

        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshShop);

        RefreshShop();
    }

    public void RefreshShop()
    {
        // Limpa a loja
        if (shopPanel != null)
        {
            for (int i = shopPanel.childCount - 1; i >= 0; i--)
                Destroy(shopPanel.GetChild(i).gameObject);
        }

        // Cria novas cartas
        for (int i = 0; i < cardsPerRefresh; i++)
        {
            if (availableCards == null || availableCards.Count == 0 || shopPanel == null) break;

            var prefab = availableCards[Random.Range(0, availableCards.Count)];
            var cardGO = Instantiate(prefab, shopPanel);

            // Garante componentes úteis
            if (cardGO.GetComponent<CanvasGroup>() == null)
                cardGO.AddComponent<CanvasGroup>();
            if (cardGO.GetComponent<ShopItem>() == null)
                cardGO.AddComponent<ShopItem>();

            // Injeta referências no Card (prefab-friendly)
            var card = cardGO.GetComponent<Card>();
            if (card == null)
            {
                Debug.LogError("Prefab de carta não tem componente 'Card'.");
                continue;
            }

            card.Setup(
                shop: shopPanel,
                hand: handPanel,
                stats: playerStats,
                maxHand: maxHandSize,
                draggingLayerOverride: draggingLayer,
                previewLayerOverride: previewLayer
            );
        }
    }

    // ---------- Helpers ----------
    private Transform GetCanvasRootTransform()
    {
        // 1) Tenta achar um Canvas no(s) pais
        var canvasInParents = GetComponentInParent<Canvas>();
        if (canvasInParents != null) return canvasInParents.transform;

        // 2) Pega o primeiro Canvas da cena (Unity 2023+ recomenda FindFirstObjectByType)
#if UNITY_2023_1_OR_NEWER
        var canvas = Object.FindFirstObjectByType<Canvas>();
#else
        var canvas = Object.FindObjectOfType<Canvas>();
#endif
        return canvas ? canvas.transform : null;
    }

    private PlayerStats GetFirstPlayerStats()
    {
#if UNITY_2023_1_OR_NEWER
        // Tenta achar o primeiro PlayerStats na cena (apenas fallback)
        return Object.FindFirstObjectByType<PlayerStats>();
#else
        return Object.FindObjectOfType<PlayerStats>();
#endif
    }
}
