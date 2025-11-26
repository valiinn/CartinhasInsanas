using UnityEngine;
using UnityEngine.EventSystems;

public class BuffDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private RectTransform rt;

    private Transform originalParent;
    private Vector3 originalScale;

    private Canvas dragCanvas;
    private RectTransform dragCanvasRT;

    private BuffType buffType;
    private BuffDefinition buffDefinition; // Armazena a definição completa
    private CardCombat previousCard; // Carta que tinha o buff antes
    private Transform homePanel;     // Painel "MyBuffs" de origem

    /// <summary>
    /// Configura o buff usando apenas o tipo (compatibilidade com código antigo)
    /// </summary>
    public void Setup(BuffType type)
    {
        buffType = type;
        buffDefinition = null;
    }

    /// <summary>
    /// Configura o buff usando BuffDefinition completo (método recomendado)
    /// </summary>
    public void Setup(BuffDefinition definition)
    {
        if (definition != null)
        {
            buffType = definition.buffType;
            buffDefinition = definition;
        }
    }

    /// <summary>
    /// Define o painel de origem (MyBuffs) para onde o buff volta quando não está em uma carta
    /// </summary>
    public void SetHomePanel(Transform panel)
    {
        homePanel = panel;
    }

    private void Awake()
    {
        rt = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    // ===========================================================
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalScale = transform.localScale;

        // Detecta qual carta tinha o buff antes de arrastar
        previousCard = transform.parent?.GetComponent<CardCombat>();
        if (previousCard == null)
            previousCard = transform.parent?.GetComponentInParent<CardCombat>();

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;

        dragCanvas = GameObject.Find("DraggingLayer")?.GetComponent<Canvas>();
        if (dragCanvas == null)
        {
            Debug.LogError("⚠ Não existe um Canvas chamado 'DraggingLayer'!");
            return;
        }

        dragCanvasRT = dragCanvas.GetComponent<RectTransform>();

        transform.SetParent(dragCanvas.transform, false);
        rt.localScale = Vector3.one * 0.5f; // zoom durante drag
    }

    // ===========================================================
    public void OnDrag(PointerEventData eventData)
    {
        if (dragCanvasRT == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragCanvasRT,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 pos))
        {
            rt.anchoredPosition = pos;
        }
    }

    // ===========================================================
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        CardCombat card = null;
        if (eventData.pointerEnter != null)
            card = eventData.pointerEnter.GetComponentInParent<CardCombat>();

        // Se soltou em cima de uma carta, aplica/move o buff para ela
        if (card != null)
        {
            AttachToCard(card);
            return;
        }

        // Se o buff já estava aplicado em uma carta, sempre volta para essa carta
        if (previousCard != null)
        {
            AttachToCard(previousCard);
            return;
        }

        // Caso contrário (buff ainda está no painel), volta para o painel de origem
        Transform targetParent = homePanel != null ? homePanel : originalParent;
        transform.SetParent(targetParent, false);
        rt.localScale = originalScale;
    }

    // ===========================================================
    private void AttachToCard(CardCombat card)
    {
        BuffSystem bs = card.GetComponent<BuffSystem>();
        if (bs == null)
            bs = card.gameObject.AddComponent<BuffSystem>();

        // Se está movendo para uma carta diferente, remove o buff da carta anterior
        if (previousCard != null && previousCard != card)
        {
            BuffSystem previousBS = previousCard.GetComponent<BuffSystem>();
            if (previousBS != null)
            {
                // Remove o buff da carta anterior sem limpar a definição
                previousBS.RemoveBuffWithoutClearingDefinition(buffType);
                Debug.Log($"Buff {buffType} removido da carta anterior: {previousCard.name}");
            }
        }

        // Se a carta já tem esse buff, não aplica novamente
        if (bs.activeBuffs.Contains(buffType))
        {
            Debug.Log($"Carta {card.name} já tem o buff {buffType}. Mantendo na carta.");
            // Atualiza a referência mesmo assim
            previousCard = card;
            transform.SetParent(card.transform, false);
            return;
        }

        // Passa a definição completa se disponível, senão usa apenas o tipo
        bs.ApplyBuff(buffType, buffDefinition);

        // Atualiza a referência da carta atual
        previousCard = card;

        transform.SetParent(card.transform, false);

        RectTransform buffRT = GetComponent<RectTransform>();

        // Conta quantos buffs já existem
        int buffIndex = 0;
        foreach (Transform child in card.transform)
        {
            if (child.GetComponent<BuffDraggable>())
                buffIndex++;
        }

        // 🔥 OFFSET DE STACK (cada buff desloca um pouquinho pro lado)
        float offsetX = 4f + (buffIndex * 22f); // ajuste fino
        float offsetY = 4f;

        buffRT.anchorMin = new Vector2(0f, 0f);
        buffRT.anchorMax = new Vector2(0f, 0f);
        buffRT.pivot = new Vector2(0f, 0f);

        buffRT.anchoredPosition = new Vector2(offsetX, offsetY);

        // 🔥 Tamanho pequeno
        buffRT.localScale = new Vector3(0.14f, 0.14f, 1f);

        canvasGroup.alpha = 1f;

        buffRT.SetAsLastSibling();
    }
}
