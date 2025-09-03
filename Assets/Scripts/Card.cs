using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

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
    private GameObject currentPreview;

    private Transform originalParent;
    private CanvasGroup canvasGroup;
    private Canvas canvas;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        if (draggingLayer == null)
            draggingLayer = canvas.transform;

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
        originalParent = transform.parent;
        transform.SetParent(draggingLayer);
        transform.localScale = Vector3.one;
        canvasGroup.blocksRaycasts = false;

        // Se estiver com preview aberto, fecha
        if (currentPreview != null) Destroy(currentPreview);
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
            transform.localPosition = Vector3.zero;
        }
        canvasGroup.blocksRaycasts = true;
    }
    #endregion

    #region Hover Preview
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (cardPreviewPrefab != null)
        {
            currentPreview = Instantiate(cardPreviewPrefab, canvas.transform);
            currentPreview.transform.SetAsLastSibling(); // garante que fique acima de tudo

            // Ajusta o preview com os mesmos dados da carta
            Card previewCard = currentPreview.GetComponent<Card>();
            if (previewCard != null)
            {
                previewCard.nome = this.nome;
                previewCard.vida = this.vida;
                previewCard.dano = this.dano;
                previewCard.AtualizarUI();
            }

            // Opcional: posiciona no centro da tela ou perto do mouse
            RectTransform rt = currentPreview.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }
    }
    #endregion
}
