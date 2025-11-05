using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PhaseType { Battle, Boss, Merchant, Event }

[System.Serializable]
public class PhaseIcon
{
    public PhaseType type;
    public Sprite icon;
}

public class PhaseManager : MonoBehaviour
{
    [Header("Phase Setup")]
    public int totalPhases = 12;
    public PhaseIcon[] phaseIcons; // ícones padrão (Battle, Boss, Merchant, Event)

    [Header("UI References")]
    public ScrollRect scrollRect;
    public Transform phaseContainer;
    public GameObject phaseIconPrefab;

    [Header("UI Colors & Scale")]
    public Color pastColor = new Color(0.6f, 0.6f, 0.6f);
    public Color currentColor = Color.white;
    public Color futureColor = new Color(1f, 1f, 1f, 0.3f);
    public float centerScale = 1.2f;
    public float sideScale = 0.9f;

    [Header("Animation Settings")]
    public float smoothSpeed = 8f;

    private readonly PhaseType[] pattern = {
        PhaseType.Battle, PhaseType.Battle, PhaseType.Battle,
        PhaseType.Merchant, PhaseType.Boss, PhaseType.Event
    };

    private List<Image> phaseIconsInUI = new List<Image>();
    private int currentPhaseIndex = 0;
    private float scrollLerpTarget = 0f;

    void Start()
    {
        GeneratePhases();
        UpdateUI(true);
        StartCoroutine(InitCenter());
    }

    IEnumerator InitCenter()
    {
        // aguarda 1 frame pro layout calcular tudo
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(phaseContainer.GetComponent<RectTransform>());

        SetCenterNormalizedImmediate(currentPhaseIndex);
    }

    void Update()
    {
        // suaviza rolagem a cada frame
        if (scrollRect != null)
        {
            float cur = scrollRect.horizontalNormalizedPosition;
            float next = Mathf.Lerp(cur, scrollLerpTarget, Time.deltaTime * smoothSpeed);
            scrollRect.horizontalNormalizedPosition = next;
        }

        if (Input.GetKeyDown(KeyCode.N))
            NextPhase();
    }

    // 🔹 Gera as fases dinamicamente com base no padrão
    void GeneratePhases()
    {
        foreach (Transform child in phaseContainer)
            Destroy(child.gameObject);

        phaseIconsInUI.Clear();

        for (int i = 0; i < totalPhases; i++)
        {
            PhaseType type = pattern[i % pattern.Length];

            GameObject iconObj = Instantiate(phaseIconPrefab, phaseContainer);
            Image img = iconObj.GetComponent<Image>();
            img.sprite = GetSpriteForType(type);
            img.color = futureColor;

            phaseIconsInUI.Add(img);
        }
    }

    // 🔹 Atualiza cor e tamanho dos ícones
    void UpdateUI(bool instant = false)
    {
        for (int i = 0; i < phaseIconsInUI.Count; i++)
        {
            var img = phaseIconsInUI[i];
            if (i < currentPhaseIndex)
            {
                img.color = pastColor;
                img.transform.localScale = Vector3.one * sideScale;
            }
            else if (i == currentPhaseIndex)
            {
                img.color = currentColor;
                img.transform.localScale = Vector3.one * centerScale;
            }
            else
            {
                img.color = futureColor;
                img.transform.localScale = Vector3.one * sideScale;
            }
        }
    }

    // 🔹 Avança para a próxima fase
    public void NextPhase()
    {
        if (phaseIconsInUI.Count == 0) return;

        currentPhaseIndex = (currentPhaseIndex + 1) % phaseIconsInUI.Count;

        UpdateUI();
        CenterOnCurrentPhase();
        LoadPhase();
    }

    void LoadPhase()
    {
        var type = pattern[currentPhaseIndex % pattern.Length];
        Debug.Log($"Entrando na fase {currentPhaseIndex + 1}: {type}");
    }

    // 🔹 Busca o sprite correspondente ao tipo
    Sprite GetSpriteForType(PhaseType type)
    {
        foreach (var p in phaseIcons)
            if (p.type == type)
                return p.icon;
        return null;
    }

    // 🔹 Centraliza o ícone atual (Lerp suave)
    void CenterOnCurrentPhase()
    {
        scrollLerpTarget = ComputeCenterNormalized(currentPhaseIndex);
    }

    void SetCenterNormalizedImmediate(int index)
    {
        float norm = ComputeCenterNormalized(index);
        scrollRect.horizontalNormalizedPosition = norm;
        scrollLerpTarget = norm;
    }

    // 🔹 Cálculo robusto pra centralizar o item pelo ScrollRect
    float ComputeCenterNormalized(int index)
    {
        if (scrollRect == null || phaseContainer == null || phaseIconsInUI.Count == 0)
            return 0f;

        RectTransform content = phaseContainer.GetComponent<RectTransform>();
        RectTransform viewport = scrollRect.viewport;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        float contentW = content.rect.width;
        float viewportW = viewport.rect.width;

        if (contentW <= viewportW) return 0.5f;

        RectTransform item = phaseIconsInUI[index].rectTransform;
        Vector3 itemWorldCenter = item.TransformPoint(item.rect.center);
        Vector3 itemLocalInContent = content.InverseTransformPoint(itemWorldCenter);

        float contentLeft = -content.rect.width * content.pivot.x;
        float itemCenterFromLeft = itemLocalInContent.x - contentLeft;

        float desiredLeft = Mathf.Clamp(itemCenterFromLeft - viewportW * 0.5f, 0f, contentW - viewportW);
        float normalized = desiredLeft / (contentW - viewportW);

        return Mathf.Clamp01(normalized);
    }
}
