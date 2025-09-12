using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

// É uma boa prática garantir que o Prefab da carta também tenha um CanvasGroup.
[RequireComponent(typeof(CanvasGroup))]
public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Atributos da Carta")]
    public string nome;
    public int vida = 10;
    public int dano = 5;

    [Header("Referências UI")]
    public TMP_Text vidaText;
    public TMP_Text danoText;
    public TMP_Text nomeText;

    [Header("Drag & Drop")]
    public Transform draggingLayer;

    [Header("Preview")]
    public GameObject cardPreviewPrefab; // Prefab da versão ampliada

    [Tooltip("A camada/container onde o preview da carta será instanciado. Deve estar em um Canvas superior.")]
    public Transform previewLayer;

    private GameObject currentPreview;
    private Transform originalParent;
    private CanvasGroup canvasGroup;
    private Canvas mainCanvas;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        mainCanvas = GetComponentInParent<Canvas>();

        // Se a draggingLayer não for definida, usa o Canvas principal como padrão.
        if (draggingLayer == null)
            draggingLayer = mainCanvas.transform;

        // **MELHORIA**: Se a camada de preview não for definida, usa a camada de arrastar como padrão.
        if (previewLayer == null)
            previewLayer = draggingLayer;

        AtualizarUI();
    }

    public void AtualizarUI()
    {
        if (vidaText != null) vidaText.text = "❤ " + vida;
        if (danoText != null) danoText.text = "⚔ " + dano;
        if (nomeText != null) nomeText.text = nome;
    }

    #region Drag & Drop
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Se houver um preview ativo ao começar a arrastar, ele é destruído.
        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }

        originalParent = transform.parent;
        transform.SetParent(draggingLayer);
        transform.localScale = Vector3.one; // Garante que a escala não seja afetada pelo novo pai.
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (transform.parent == draggingLayer)
        {
            transform.SetParent(originalParent);
            // Se estiver usando um LayoutGroup, ele cuidará da posição.
            // Se não, descomente a linha abaixo para resetar a posição local.
            // transform.localPosition = Vector3.zero;
        }
        canvasGroup.blocksRaycasts = true;
    }
    #endregion

    #region Hover Preview
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Garante que não está arrastando para mostrar o preview
        if (eventData.dragging) return;

        if (cardPreviewPrefab != null && currentPreview == null)
        {
            // Instancia o preview na camada designada (CanvasOverlay)
            currentPreview = Instantiate(cardPreviewPrefab, previewLayer);
            currentPreview.transform.SetAsLastSibling(); // Garante que fique acima de outros elementos na mesma camada.

            CanvasGroup previewCanvasGroup = currentPreview.GetComponent<CanvasGroup>();
            if (previewCanvasGroup == null)
            {
                // Garante que o prefab do preview sempre terá um CanvasGroup.
                previewCanvasGroup = currentPreview.AddComponent<CanvasGroup>();
            }
            previewCanvasGroup.blocksRaycasts = false;
            // ===================================

            // Ajusta o preview com os mesmos dados da carta
            Card previewCardComponent = currentPreview.GetComponent<Card>();
            if (previewCardComponent != null)
            {
                previewCardComponent.nome = this.nome;
                previewCardComponent.vida = this.vida;
                previewCardComponent.dano = this.dano;
                previewCardComponent.AtualizarUI();
            }

            // Posiciona no centro da tela
            RectTransform rt = currentPreview.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = Vector2.zero;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null; // Limpa a referência para evitar problemas
        }
    }
    #endregion
}
