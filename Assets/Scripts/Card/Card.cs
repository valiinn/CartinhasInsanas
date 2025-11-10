using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public enum CardRarity { Comum, Rara, Epica, Lendaria }
public enum CardTrait { Nenhum, Mar, Floresta, Geleira, Deserto }

[RequireComponent(typeof(CanvasGroup))]
public class Card : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // =========================================================
    //                         DADOS
    // =========================================================
    public string nome;
    [HideInInspector] public int custo;
    public CardRarity raridade = CardRarity.Comum;

    // =========================================================
    //                           UI
    // =========================================================
    public TMP_Text nomeText;
    public TMP_Text custoText;
    public TMP_Text raridadeText;
    public Image background;

    // =========================================================
    //                          TRAIT
    // =========================================================
    public CardTrait trait = CardTrait.Nenhum;
    public TMP_Text traitText;
    public Image traitBadge;

    // =========================================================
    //                        PAINÉIS
    // =========================================================
    public Transform shopPanel;
    public Transform handPanel;
    public int maxHandSize = 7;

    // =========================================================
    //                       DRAG & DROP
    // =========================================================
    public Transform draggingLayer;

    // =========================================================
    //                         PREVIEW
    // =========================================================
    public GameObject cardPreviewPrefab;
    public Transform previewLayer;

    // =========================================================
    //                        ECONOMIA
    // =========================================================
    public PlayerStats playerStats;

    // =========================================================
    //                    RUNTIME / ESTADO
    // =========================================================
    private GameObject currentPreview;
    private Transform originalParent;
    public Transform OriginalParent => originalParent;

    private CanvasGroup canvasGroup;
    private Canvas mainCanvas;

    // =========================================================
    //                         SETUP API
    // =========================================================
    public void Setup(Transform shop, Transform hand, PlayerStats stats,
                      int maxHand = 7,
                      Transform draggingLayerOverride = null,
                      Transform previewLayerOverride = null)
    {
        shopPanel = shop;
        handPanel = hand;
        playerStats = stats;
        maxHandSize = maxHand;
        if (draggingLayerOverride != null) draggingLayer = draggingLayerOverride;
        if (previewLayerOverride != null) previewLayer = previewLayerOverride;
    }

    public void SetOriginalParent(Transform newParent)
    {
        originalParent = newParent;
    }

    // =========================================================
    //                      CICLO DE VIDA
    // =========================================================
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        mainCanvas = GetComponentInParent<Canvas>();

        if (draggingLayer == null && mainCanvas != null)
            draggingLayer = mainCanvas.transform;

        if (previewLayer == null)
            previewLayer = draggingLayer;

        DefinirCustoPorRaridade();
        AtualizarUI();
    }

    // =========================================================
    //             DADOS -> CUSTO / CORES / TRAIT
    // =========================================================
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
        if (nomeText != null) nomeText.text = nome;
        if (custoText != null) custoText.text = $"{custo}";
        if (raridadeText != null) raridadeText.text = $"{raridade} ({custo}g)";
        AtualizarCorPorRaridade();
        AtualizarTraitUI();
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

    void AtualizarTraitUI()
    {
        if (traitText != null)
            traitText.text = trait == CardTrait.Nenhum ? "" : trait.ToString();

        if (traitBadge != null)
        {
            switch (trait)
            {
                case CardTrait.Mar: traitBadge.color = new Color(0.30f, 0.60f, 1f); break;
                case CardTrait.Floresta: traitBadge.color = new Color(0.20f, 0.70f, 0.30f); break;
                case CardTrait.Geleira: traitBadge.color = new Color(0.60f, 0.90f, 1f); break;
                case CardTrait.Deserto: traitBadge.color = new Color(1f, 0.85f, 0.40f); break;
                default: traitBadge.color = new Color(0, 0, 0, 0); break;
            }
        }
    }

    // =========================================================
    //                     DRAG & DROP (EVENTOS)
    // =========================================================
    public void OnBeginDrag(PointerEventData eventData)
    {
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
        if (shopPanel != null && transform.parent == shopPanel) return;
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (shopPanel != null && transform.parent == shopPanel) return;

        bool droppedInDropSlot = transform.parent != null &&
                                 transform.parent.GetComponent<DropSlot>() != null;

        if (droppedInDropSlot)
        {
            originalParent = transform.parent;
        }
        else
        {
            Transform target = handPanel != null ? handPanel : originalParent;
            transform.SetParent(target, false);
            ResetForParent(target);
        }

        canvasGroup.blocksRaycasts = true;
    }

    // =========================================================
    //                        HELPERS LAYOUT
    // =========================================================
    private void ResetForParent(Transform parent)
    {
        var rt = transform as RectTransform;
        var prt = parent as RectTransform;

        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
        rt.pivot = new Vector2(0.5f, 0.5f);

        var grid = (prt != null) ? prt.GetComponent<GridLayoutGroup>() : null;
        var hlg = (prt != null) ? prt.GetComponent<HorizontalLayoutGroup>() : null;
        var vlg = (prt != null) ? prt.GetComponent<VerticalLayoutGroup>() : null;
        bool usesLayout = (grid != null || hlg != null || vlg != null);

        if (!usesLayout)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }
        else if (grid != null)
        {
            rt.sizeDelta = grid.cellSize;
        }
    }

    // =========================================================
    //                     HOVER PREVIEW (UI)
    // =========================================================
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData != null && eventData.dragging) return;
        if (cardPreviewPrefab == null || currentPreview != null) return;

        currentPreview = Instantiate(cardPreviewPrefab, previewLayer);
        currentPreview.transform.SetAsLastSibling();

        var previewCG = currentPreview.GetComponent<CanvasGroup>();
        if (previewCG == null) previewCG = currentPreview.AddComponent<CanvasGroup>();
        previewCG.blocksRaycasts = false;

        var previewCard = currentPreview.GetComponent<Card>();
        if (previewCard != null)
        {
            previewCard.nome = this.nome;
            previewCard.raridade = this.raridade;
            previewCard.trait = this.trait;
            previewCard.DefinirCustoPorRaridade();
            previewCard.AtualizarUI();
        }

        var rt = currentPreview.GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition = Vector2.zero;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }
    }

    // =========================================================
    //                     COMPRA / VENDA (CLIQUE)
    // =========================================================
    public void OnPointerClick(PointerEventData eventData)
    {
        if (shopPanel != null && transform.parent == shopPanel)
        {
            if (handPanel == null) return;
            if (handPanel.childCount >= maxHandSize) return;

            if (playerStats != null && playerStats.SpendGold(custo))
            {
                transform.SetParent(handPanel, false);
                ResetForParent(handPanel);

                var shopItem = GetComponent<ShopItem>();
                if (shopItem != null) shopItem.bought = true;
            }
            return;
        }

        if (handPanel != null && transform.parent == handPanel)
        {
            if (playerStats != null) playerStats.AddGold(custo);

            if (shopPanel != null)
            {
                transform.SetParent(shopPanel, false);
                ResetForParent(shopPanel);
            }

            var shopItem = GetComponent<ShopItem>();
            if (shopItem != null) shopItem.bought = false;
        }
    }
}
