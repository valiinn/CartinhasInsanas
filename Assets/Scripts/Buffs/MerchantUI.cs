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
    [SerializeField] private GameObject buffCardPrefab;
    [SerializeField] private Button closeButton;

    [Header("Buff Definitions")]
    [SerializeField] private List<BuffDefinition> buffDefinitions = new List<BuffDefinition>();

    [Header("Shop Settings")]
    public int buffsPerRefresh = 3;
    public bool randomize = true;

    private void Start()
    {
        if (merchantPanel != null) merchantPanel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(HideMerchant);
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
        GameObject card = Instantiate(buffCardPrefab, itemGrid);

        var icon = card.transform.Find("Content/Icon")?.GetComponent<Image>();
        var nameText = card.transform.Find("Content/Name")?.GetComponent<TMP_Text>();
        var costText = card.transform.Find("Content/Cost")?.GetComponent<TMP_Text>();

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
        PlayerStats ps = FindAnyObjectByType<PlayerStats>();
        if (ps == null) return;

        if (ps.gold < def.cost)
        {
            Debug.Log("Ouro insuficiente");
            return;
        }

        ps.SpendGold(def.cost);

        // aplica buff nas cartas do jogador
        var cards = CombatManager.Instance.GetActiveCardCombats(CombatManager.Instance.tabuleiroB);
        foreach (var c in cards)
        {
            var bs = c.GetComponent<BuffSystem>();
            if (bs == null) bs = c.gameObject.AddComponent<BuffSystem>();

            bs.ApplyBuff(def.buffType);
        }

        // move pro painel do jogador
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

        drag.Setup(def.buffType);

        LayoutRebuilder.ForceRebuildLayoutImmediate(myBuffsPanel.GetComponent<RectTransform>());
    }
}
