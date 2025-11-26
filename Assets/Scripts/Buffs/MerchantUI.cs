using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MerchantUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject merchantPanel;
    [SerializeField] private Transform itemGrid;
    [SerializeField] private Transform myBuffsPanel;
    [SerializeField] private GameObject buffCardPrefab; // Prefab padrão (fallback se BuffDefinition não tiver prefab)
    [SerializeField] private Button closeButton;
    [SerializeField] private Button closeXButton; // Botão X para fechar

    [Header("Referências")]
    [SerializeField] private PlayerStats playerStats; // Referência direta ao invés de FindObjectOfType

    [Header("Buff Definitions")]
    [SerializeField] private List<BuffDefinition> buffDefinitions = new List<BuffDefinition>();

    [Header("Shop Settings")]
    public int buffsPerRefresh = 3;
    public bool randomize = true;

    private void Start()
    {
        if (merchantPanel != null) merchantPanel.SetActive(false);
        
        // Configura botão de fechar padrão
        if (closeButton != null) 
            closeButton.onClick.AddListener(HideMerchant);
        
        // Configura botão X de fechar
        if (closeXButton != null) 
            closeXButton.onClick.AddListener(HideMerchant);

        // Busca PlayerStats se não foi atribuído no Inspector
        if (playerStats == null)
        {
#if UNITY_2023_1_OR_NEWER
            playerStats = FindFirstObjectByType<PlayerStats>();
#else
            playerStats = FindObjectOfType<PlayerStats>();
#endif
        }
    }

    public void ShowMerchant()
    {
        merchantPanel.SetActive(true);
        GenerateBuffCards();
    }

    public void HideMerchant()
    {
        merchantPanel.SetActive(false);
    }

    // --------------------------------------------------------------------
    // GERA BUFES NO SHOP
    // --------------------------------------------------------------------
    public void GenerateBuffCards()
    {
        foreach (Transform c in itemGrid)
            Destroy(c.gameObject);

        if (buffDefinitions.Count == 0)
        {
            Debug.LogWarning("Nenhum buff definido!");
            return;
        }

        List<BuffDefinition> choices = new List<BuffDefinition>();

        if (randomize)
        {
            List<int> idxs = new List<int>();
            for (int i = 0; i < buffDefinitions.Count; i++)
                idxs.Add(i);

            for (int i = 0; i < buffsPerRefresh && idxs.Count > 0; i++)
            {
                int pick = Random.Range(0, idxs.Count);
                choices.Add(buffDefinitions[idxs[pick]]);
                idxs.RemoveAt(pick);
            }
        }
        else
        {
            for (int i = 0; i < Mathf.Min(buffsPerRefresh, buffDefinitions.Count); i++)
                choices.Add(buffDefinitions[i]);
        }

        foreach (var def in choices)
            CreateBuffCard(def);
    }

    // --------------------------------------------------------------------
    // CRIA A CARTA DE BUFF NO SHOP
    // --------------------------------------------------------------------
    private void CreateBuffCard(BuffDefinition def)
    {
        // Usa o prefab específico do BuffDefinition, ou o genérico como fallback
        GameObject prefabToUse = def.buffPrefab != null ? def.buffPrefab : buffCardPrefab;
        
        if (prefabToUse == null)
        {
            Debug.LogError($"MerchantUI: Nenhum prefab definido para o buff '{def.buffName}' e nenhum prefab padrão configurado!");
            return;
        }

        GameObject card = Instantiate(prefabToUse, itemGrid);

        // Tenta encontrar os componentes UI (pode variar dependendo do prefab)
        var icon = card.transform.Find("Content/Icon")?.GetComponent<Image>();
        if (icon == null) icon = card.transform.Find("Icon")?.GetComponent<Image>();
        
        var nameText = card.transform.Find("Content/Name")?.GetComponent<TMP_Text>();
        if (nameText == null) nameText = card.transform.Find("Name")?.GetComponent<TMP_Text>();
        
        var costText = card.transform.Find("Content/Cost")?.GetComponent<TMP_Text>();
        if (costText == null) costText = card.transform.Find("Cost")?.GetComponent<TMP_Text>();

        var button = card.GetComponent<Button>();
        if (button == null) button = card.AddComponent<Button>();

        if (icon != null) icon.sprite = def.icon;
        if (nameText != null) nameText.text = def.buffName;
        if (costText != null) costText.text = def.cost + "G";

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnBuyBuff(def, card));

        // garante opacidade
        CanvasGroup cg = card.GetComponent<CanvasGroup>();
        if (cg == null) cg = card.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
    }

    // --------------------------------------------------------------------
    // COMPRA DO BUFF
    // --------------------------------------------------------------------
    private void OnBuyBuff(BuffDefinition def, GameObject card)
    {
        if (playerStats == null)
        {
            Debug.LogWarning("MerchantUI: PlayerStats não encontrado!");
            return;
        }

        if (playerStats.gold < def.cost)
        {
            Debug.Log("Ouro insuficiente");
            return;
        }

        playerStats.SpendGold(def.cost);

        // move pro painel do jogador (o jogador arrasta para a carta desejada)
        AddBuffToMyPanel(def, card);

        Debug.Log($"Comprou o buff {def.buffName}");
    }

    // --------------------------------------------------------------------
    // A CARTA VAI PARA O PAINEL DO JOGADOR (110×90)
    // --------------------------------------------------------------------
    private void AddBuffToMyPanel(BuffDefinition def, GameObject card)
    {
        card.transform.SetParent(myBuffsPanel, false);

        RectTransform rt = card.GetComponent<RectTransform>();

        if (rt != null)
        {
            // 🔥 TAMANHO REAL do buff dentro do painel
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 110f);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 90f);

            // 🔥 Escala (opcional, se quiser ainda menor)
            rt.localScale = Vector3.one * 0.45f;

            // 🔥 Reseta o position pra evitar offset estranho
            rt.anchoredPosition = Vector2.zero;

        }

        // desabilita compra
        var btn = card.GetComponent<Button>();
        if (btn != null) btn.interactable = false;

        // torna arrastável
        var drag = card.GetComponent<BuffDraggable>();
        if (drag == null) drag = card.AddComponent<BuffDraggable>();

        // Passa a definição completa para usar valores configuráveis
        drag.Setup(def);
        // Define o painel de origem (MyBuffs) para o buff
        drag.SetHomePanel(myBuffsPanel);

        LayoutRebuilder.ForceRebuildLayoutImmediate(myBuffsPanel.GetComponent<RectTransform>());
    }
}
