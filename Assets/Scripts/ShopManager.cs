using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("Referências")]
    public Transform shopPanel;
    public Button refreshButton;
    public int cardsPerRefresh = 5;

    [Header("Cartas disponíveis")]
    public List<GameObject> availableCards; // Prefabs de cartas

    void Start()
    {
        refreshButton.onClick.AddListener(RefreshShop);
        RefreshShop();
    }

    public void RefreshShop()
    {
        // Limpa loja
        foreach (Transform child in shopPanel)
            Destroy(child.gameObject);

        // Cria novas cartas
        for (int i = 0; i < cardsPerRefresh; i++)
        {
            if (availableCards.Count == 0) return;

            // Sorteia prefab
            GameObject cardPrefab = availableCards[Random.Range(0, availableCards.Count)];

            // Instancia direto
            Instantiate(cardPrefab, shopPanel);
        }
    }
}
