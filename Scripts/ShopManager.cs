using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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

    private Dictionary<GameObject, bool> cardBought = new Dictionary<GameObject, bool>();

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

        cardBought.Clear();

        // Cria novas cartas
        for (int i = 0; i < cardsPerRefresh; i++)
        {
            if (availableCards.Count == 0) return;

            GameObject cardPrefab = availableCards[Random.Range(0, availableCards.Count)];
            GameObject cardInstance = Instantiate(cardPrefab, shopPanel);

            cardBought[cardInstance] = false;

            // Adiciona drag dinamicamente
            EventTrigger trigger = cardInstance.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = cardInstance.AddComponent<EventTrigger>();

            // BeginDrag
            EventTrigger.Entry beginDrag = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
            beginDrag.callback.AddListener((data) =>
            {
                cardInstance.transform.SetParent(transform.root); // sobe na hierarquia
            });
            trigger.triggers.Add(beginDrag);

            // Drag
            EventTrigger.Entry drag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
            drag.callback.AddListener((data) =>
            {
                PointerEventData pointerData = (PointerEventData)data;
                cardInstance.transform.position = pointerData.position;
            });
            trigger.triggers.Add(drag);

            // EndDrag
            EventTrigger.Entry endDrag = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
            endDrag.callback.AddListener((data) =>
            {
                PointerEventData pointerData = (PointerEventData)data;
                GameObject dropTarget = pointerData.pointerCurrentRaycast.gameObject;

                if (dropTarget != null && dropTarget.transform.IsChildOf(boardPanel))
                {
                    if (!cardBought[cardInstance] && playerStats.gold >= 5)
                    {
                        playerStats.SpendGold(5);
                        cardBought[cardInstance] = true;
                    }
                    cardInstance.transform.SetParent(dropTarget.transform);
                    cardInstance.transform.localPosition = Vector3.zero;
                }
                else
                {
                    if (cardBought[cardInstance])
                    {
                        playerStats.AddGold(5);
                        cardBought[cardInstance] = false;
                    }
                    cardInstance.transform.SetParent(shopPanel);
                    cardInstance.transform.localPosition = Vector3.zero;
                }
            });
            trigger.triggers.Add(endDrag);
        }
    }
}
