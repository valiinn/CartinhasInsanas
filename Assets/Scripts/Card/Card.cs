using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public enum CardRarity { Comum, Rara, Epica, Lendaria }

[RequireComponent(typeof(CanvasGroup))]
public class Card : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Atributos da Carta")]
    public string nome;
    [HideInInspector] public int custo;
    public CardRarity raridade;

    [Header("Referências UI")]
    public TMP_Text nomeText;
    public TMP_Text custoText;
    public TMP_Text raridadeText;
    public Image background;

    [Header("Painéis (preenchidos via Setup)")]
    public Transform shopPanel;
    public Transform handPanel;
    public int maxHandSize = 7;

    [Header("Drag & Drop")]
    public Transform draggingLayer;

    [Header("Preview")]
    public GameObject cardPreviewPrefab;
    public Transform previewLayer;

    [Header("Player Stats")]
    public PlayerStats playerStats;

    // --- Runtime ---
    private GameObject currentPreview;
    private Transform originalParent;
    public Transform OriginalParent
    {
        get => originalParent;
        set => originalParent = value;
    }

    private CanvasGroup canvasGroup;
    private Canvas mainCanvas;

    // ========================
    //  SETUP
    // ========================
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

    // ========================
    //  UI / Visual
    // ========================
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

    // ========================
    //  DRAG & DROP
    // ========================
    public void OnBeginDrag(PointerEventData eventData)
    {
        // bloqueia drag na loja
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
        if (shopPanel != null && transform.parent == shopPanel)
            return;

        // volta raycasts antes de qualquer coisa
        canvasGroup.blocksRaycasts = true;

        bool droppedInDropSlot = transform.parent != null && transform.parent.GetComponent<DropSlot>() != null;

        if (droppedInDropSlot)
        {
            originalParent = transform.parent;
        }
        else
        {
            Transform target = originalParent != null ? originalParent : handPanel;
            transform.SetParent(target, false);
            ResetForParent(target);

            // força atualização de layout (evita bug visual)
            LayoutRebuilder.ForceRebuildLayoutImmediate(target as RectTransform);
        }

        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
    }

    // ========================
    //  PREVIEW (HOVER)
    // ========================
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
                previewCard.nome = nome;
                previewCard.raridade = raridade;
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

    // ========================
    //  COMPRA / VENDA
    // ========================
    public void OnPointerClick(PointerEventData eventData)
    {
        // Compra
        if (shopPanel != null && transform.parent == shopPanel)
        {
            if (handPanel == null)
            {
                Debug.LogError("HandPanel não atribuído!");
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
        // Venda
        else if (handPanel != null && transform.parent == handPanel)
        {
            if (playerStats != null)
            {
                playerStats.AddGold(custo);
                Debug.Log($"+{custo}g adicionados ao jogador.");
            }

            // Remove da mão e destrói (ou manda de volta pra loja, se quiser reaparecer lá)
            if (shopPanel != null)
            {
                transform.SetParent(shopPanel, false);
                ResetForParent(shopPanel);
            }

            var shopItem = GetComponent<ShopItem>();
            if (shopItem != null)
                shopItem.bought = false;

            // se quiser que a carta suma da mão (venda destrói mesmo)
            Destroy(gameObject);
        }

    }

    // ========================
    //  HELPER DE POSICIONAMENTO
    // ========================
    private void ResetForParent(Transform parent)
    {
        var rt = transform as RectTransform;
        var prt = parent as RectTransform;

        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
        rt.pivot = new Vector2(0.5f, 0.5f);

        // se o pai usa LayoutGroup, ele cuida sozinho
        bool usesLayout = prt && prt.GetComponent<LayoutGroup>() != null;

        if (!usesLayout)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(75, 90); // mesmo tamanho do prefab
        }
    }

}
