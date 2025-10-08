using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItemPurchase : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Header("Painéis")]
    public Transform boardPanel;   // onde as cartas jogáveis ficam
    public Transform shopPanel;    // painel da loja (se vender/devolver para loja)
    public Transform handPanel;    // <-- SUA MÃO (arraste aqui no Inspector)
    public Canvas rootCanvas;      // canvas raiz (opcional, melhora a detecção fora da tela)

    [Header("Economia")]
    public PlayerStats playerStats;

    private ShopItem shopItem;
    private Card card;          // para ler custo
    private Transform startParent;

    void Awake()
    {
        shopItem = GetComponent<ShopItem>();
        card = GetComponent<Card>();

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startParent = transform.parent; // guardamos de onde saiu
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // onde a carta terminou?
        bool nowOnBoard = boardPanel != null && transform.IsChildOf(boardPanel);
        bool nowOnShop = shopPanel != null && transform.IsChildOf(shopPanel);
        bool nowOnHand = handPanel != null && transform.IsChildOf(handPanel);

        // caiu fora da tela/canvas?
        bool fellOutside = IsOutsideCanvas(eventData);

        int cost = (card != null) ? card.custo : 5;

        if (nowOnBoard)
        {
            // COMPRA se ainda não comprada
            if (shopItem != null && !shopItem.bought)
            {
                if (playerStats != null && playerStats.gold >= cost)
                {
                    playerStats.SpendGold(cost);
                    shopItem.bought = true;
                }
                else
                {
                    // sem ouro: retorna de onde saiu
                    ReturnTo(startParent);
                }
            }
        }
        else if (nowOnShop)
        {
            // VENDA/DEVOLUÇÃO para a loja → reembolsa se estava comprada
            if (shopItem != null && shopItem.bought)
            {
                if (playerStats != null) playerStats.AddGold(cost);
                shopItem.bought = false;
            }
            // Grid da loja cuida do layout
        }
        else if (fellOutside && handPanel != null)
        {
            // ⚠️ CAIU FORA → volta para a MÃO (sem mexer no ouro)
            ReturnTo(handPanel);
        }
        else if (nowOnHand)
        {
            // já está na mão — nada a fazer
        }
        else
        {
            // caiu em um lugar qualquer: comportamento padrão -> volta de onde saiu
            ReturnTo(startParent);
        }
    }

    // ---------- Helpers ----------
    private void ReturnTo(Transform parent)
    {
        var rt = transform as RectTransform;
        transform.SetParent(parent, false);

        // Layout: se for GridLayoutGroup, não force posição (o grid cuida)
        var grid = (parent as RectTransform)?.GetComponent<GridLayoutGroup>();
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
        rt.pivot = new Vector2(0.5f, 0.5f);

        if (grid == null)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }
        // se for grid, opcional: rt.sizeDelta = grid.cellSize;
    }

    private bool IsOutsideCanvas(PointerEventData ev)
    {
        if (ev == null) return false;

        // checagem rápida por tela
        Vector2 p = ev.position;
        if (p.x < 0 || p.y < 0 || p.x > Screen.width || p.y > Screen.height)
            return true;

        // checagem por Canvas (mais robusta para Canvas com camera)
        if (rootCanvas != null)
        {
            var canvasRT = rootCanvas.transform as RectTransform;
            Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
            if (!RectTransformUtility.RectangleContainsScreenPoint(canvasRT, p, cam))
                return true;
        }
        return false;
    }
}
