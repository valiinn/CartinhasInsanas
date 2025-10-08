using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public enum CardRarity { Comum, Rara, Epica, Lendaria }

[RequireComponent(typeof(CanvasGroup))]
public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Atributos da Carta")]
    public string nome;
    public int vida = 10;
    public int dano = 5;
    [HideInInspector] public int custo; // calculado pela raridade
    public CardRarity raridade;

    [Header("Referências UI")]
    public TMP_Text vidaText;
    public TMP_Text danoText;
    public TMP_Text nomeText;
    public TMP_Text custoText;
    public TMP_Text raridadeText;
    public Image background;

    [Header("Painéis (podem ser preenchidos via Setup)")]
    public Transform shopPanel;
    public Transform handPanel;
    public int maxHandSize = 7;

    [Header("Drag & Drop")]
    public Transform draggingLayer;

    [Header("Preview")]
    public GameObject cardPreviewPrefab;
    public Transform previewLayer;

    [Header("Player Stats (pode ser preenchido via Setup)")]
    public PlayerStats playerStats;

    // runtime
    private GameObject currentPreview;
    private Transform originalParent;
    public Transform OriginalParent => originalParent;
    private CanvasGroup canvasGroup;
    private Canvas mainCanvas;

    // ==== NOVO: método para injetar referências da cena (prefab-friendly) ====
    public void Setup(Transform shop, Transform hand, PlayerStats stats, int maxHand = 7,
                      Transform draggingLayerOverride = null, Transform previewLayerOverride = null)
    {
        shopPanel = shop;
        handPanel = hand;
        playerStats = stats;
        maxHandSize = maxHand;

        if (draggingLayerOverride != null) draggingLayer = draggingLayerOverride;
        if (previewLayerOverride != null) previewLayer = previewLayerOverride;
    }
    // ========================================================================

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        mainCanvas = GetComponentInParent<Canvas>();

        if (mainCanvas == null)
            Debug.LogWarning("Card: Canvas pai não encontrado. Defina um Canvas no GameObject pai.");

        if (draggingLayer == null && mainCanvas != null)
            draggingLayer = mainCanvas.transform;

        if (previewLayer == null)
            previewLayer = draggingLayer;

        DefinirCustoPorRaridade();
        AtualizarUI();
    }

    void DefinirCustoPorRaridade()
    {
        switch (raridade)
        {
            case CardRarity.Comum: custo = 2; break;
            case CardRarity.Rara: custo = 3; break;
            case CardRarity.Epica: custo = 4; break;
            case CardRarity.Lendaria: custo = 5; break;
            default: custo = 2; break;
        }
    }

    public void AtualizarUI()
    {
        if (vidaText != null) vidaText.text = "❤" + vida;
        if (danoText != null) danoText.text = "⚔" + dano;
        if (nomeText != null) nomeText.text = nome;
        if (custoText != null) custoText.text = "" + custo;
        if (raridadeText != null) raridadeText.text = $"{raridade} ({custo}g)";
        AtualizarCorPorRaridade();
    }

    void AtualizarCorPorRaridade()
    {
        if (background == null) return;
        switch (raridade)
        {
            case CardRarity.Comum: background.color = Color.gray; break;
            case CardRarity.Rara: background.color = Color.cyan; break;
            case CardRarity.Epica: background.color = new Color(0.6f, 0f, 0.8f); break;
            case CardRarity.Lendaria: background.color = new Color(1f, 0.84f, 0f); break;
        }
    }

    #region Drag & Drop
    public void OnBeginDrag(PointerEventData eventData)
    {
        // bloqueia drag na loja, se shopPanel foi configurado
        if (shopPanel != null && transform.parent == shopPanel)
        {
            eventData.pointerDrag = null;
            return;
        }

        originalParent = transform.parent;

        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }

        if (draggingLayer != null)
            transform.SetParent(draggingLayer);

        transform.localScale = Vector3.one;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (shopPanel != null && transform.parent == shopPanel) return; // segurança extra
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (shopPanel != null && transform.parent == shopPanel) return; // segurança extra

        // Aceita apenas se o parent final for um DropSlot
        bool droppedInDropSlot = transform.parent != null && transform.parent.GetComponent<DropSlot>() != null;

        if (droppedInDropSlot)
        {
            // ficou em um slot válido -> permite swaps encadeados
            originalParent = transform.parent;
        }
        else
        {
            // não caiu em slot válido -> volta para a MÃO (se tiver), senão para o original
            Transform target = handPanel != null ? handPanel : originalParent;
            transform.SetParent(target, false);
            ResetForParent(target);
        }

        canvasGroup.blocksRaycasts = true;
    }
    #endregion

    #region Hover Preview
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData != null && eventData.dragging) return;

        if (cardPreviewPrefab != null && currentPreview == null)
        {
            currentPreview = Instantiate(cardPreviewPrefab, previewLayer);
            currentPreview.transform.SetAsLastSibling();
            CanvasGroup previewCG = currentPreview.GetComponent<CanvasGroup>();
            if (previewCG == null) previewCG = currentPreview.AddComponent<CanvasGroup>();
            previewCG.blocksRaycasts = false;

            Card previewCard = currentPreview.GetComponent<Card>();
            if (previewCard != null)
            {
                previewCard.nome = this.nome;
                previewCard.vida = this.vida;
                previewCard.dano = this.dano;
                previewCard.raridade = this.raridade;
                previewCard.DefinirCustoPorRaridade();
                previewCard.AtualizarUI();
            }

            RectTransform rt = currentPreview.GetComponent<RectTransform>();
            if (rt != null) rt.anchoredPosition = Vector2.zero;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }
    }
    #endregion

    #region Compra e Venda (por clique)
    public void OnPointerClick(PointerEventData eventData)
    {
        // COMPRA via clique na loja (se shopPanel/handPanel foram configurados)
        if (shopPanel != null && transform.parent == shopPanel)
        {
            if (handPanel == null)
            {
                Debug.LogError("HandPanel não atribuído no Card (via Setup)!");
                return;
            }

            if (handPanel.childCount >= maxHandSize)
            {
                Debug.Log("Mão cheia! Não é possível comprar mais cartas.");
                return;
            }

            if (playerStats != null && playerStats.SpendGold(custo))
            {
                transform.SetParent(handPanel, false);
                ResetForParent(handPanel);

                var shopItem = GetComponent<ShopItem>();
                if (shopItem != null) shopItem.bought = true;
            }
            else
            {
                Debug.Log("Ouro insuficiente para comprar esta carta!");
            }
        }
        // VENDA via clique na mão
        else if (handPanel != null && transform.parent == handPanel)
        {
            if (playerStats != null)
                playerStats.AddGold(custo);

            if (shopPanel != null)
            {
                transform.SetParent(shopPanel, false);
                ResetForParent(shopPanel);
            }

            var shopItem = GetComponent<ShopItem>();
            if (shopItem != null) shopItem.bought = false;
        }
    }
    #endregion

    // ---------- Helper para não brigar com LayoutGroups ----------
    private void ResetForParent(Transform parent)
    {
        var rt = transform as RectTransform;
        var prt = parent as RectTransform;

        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
        rt.pivot = new Vector2(0.5f, 0.5f);

        var hasGrid = prt ? prt.GetComponent<GridLayoutGroup>() : null;
        var hasH = prt ? prt.GetComponent<HorizontalLayoutGroup>() : null;
        var hasV = prt ? prt.GetComponent<VerticalLayoutGroup>() : null;
        bool usesLayout = (hasGrid || hasH || hasV);

        if (!usesLayout)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }
        // se o pai usa LayoutGroup, ele cuida da posição/tamanho automaticamente
    }
}
