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

    [Header("Painéis")]
    public Transform shopPanel;   // atribuído pelo ShopManager
    public Transform handPanel;   // atribuído pelo ShopManager
    public int maxHandSize = 7;   // limite de cartas na mão

    [Header("Drag & Drop")]
    public Transform draggingLayer;

    [Header("Preview")]
    public GameObject cardPreviewPrefab;
    public Transform previewLayer;

    // runtime
    private GameObject currentPreview;
    private Transform originalParent;
    public Transform OriginalParent => originalParent;
    private CanvasGroup canvasGroup;
    private Canvas mainCanvas;

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
        if (transform.parent == shopPanel) // 🔒 bloqueia drag na loja
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
        if (transform.parent == shopPanel) return; // 🔒 bloqueia na loja
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (transform.parent == shopPanel) return; // 🔒 bloqueia na loja

        bool droppedOnValidSlot =
            transform.parent != draggingLayer &&
            (mainCanvas == null || transform.parent != mainCanvas.transform);

        if (droppedOnValidSlot)
        {
            originalParent = transform.parent;
        }
        else
        {
            transform.SetParent(originalParent);
            transform.localPosition = Vector2.zero;
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

    #region Compra com Clique
    public void OnPointerClick(PointerEventData eventData)
    {
        if (transform.parent == shopPanel) // só compra se estiver na loja
        {
            if (handPanel == null)
            {
                Debug.LogError("HandPanel não atribuído no Card!");
                return;
            }

            if (handPanel.childCount >= maxHandSize) // limite de cartas
            {
                Debug.Log("Mão cheia! Não é possível comprar mais cartas.");
                return;
            }

            // move pra mão
            transform.SetParent(handPanel);
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
        }
    }
    #endregion
}
