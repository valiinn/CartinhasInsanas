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
        if (eventData.pointerDrag == null)
        {
            Debug.LogWarning("SellSlot: Nenhum objeto sendo arrastado!");
            return;
        }

        // Pega o objeto raiz que contém o componente Card
        var card = eventData.pointerDrag.GetComponentInParent<Card>();
        if (card == null)
        {
            Debug.LogWarning("SellSlot: Nenhuma carta encontrada no objeto arrastado.");
            return;
        }

        Debug.Log($"SellSlot: Carta '{card.nome}' solta no slot de venda.");

        // Reembolso (se configurado)
        if (refundOnDelete && playerStats != null)
        {
            var shopItem = card.GetComponent<ShopItem>(); // flag bought
            if (shopItem != null)
            {
                if (shopItem.bought)
                {
                    playerStats.AddGold(card.custo);
                    Debug.Log($"SellSlot: +{card.custo}g devolvidos ao jogador por vender '{card.nome}'.");
                    shopItem.bought = false;
                }
                else
                {
                    Debug.Log($"SellSlot: Carta '{card.nome}' não estava marcada como comprada — sem reembolso.");
                }
            }
            else
            {
                Debug.LogWarning("SellSlot: Nenhum ShopItem encontrado na carta — não foi possível verificar se foi comprada.");
            }
        }
        else
        {
            Debug.Log($"SellSlot: refundOnDelete={refundOnDelete} ou playerStats nulo — sem reembolso para '{card.nome}'.");
        }

        // Destroi a carta depois de um pequeno delay para garantir que logs e UI atualizem
        Object.Destroy(card.gameObject, 0.05f);
    }
}
