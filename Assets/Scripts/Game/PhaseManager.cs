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
    public PhaseIcon[] phaseIcons;

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

    // ================= Integração com StageManager (sem autostart) ================
    [Header("Stages")]
    [SerializeField] private StageManager stageManager;   // arraste no Inspector
    [SerializeField] private int stageCounter = 0;        // próximo stage normal a carregar em Battle
    [SerializeField] private int bossCounter = 0;         // qual boss usar na fase Boss
    [SerializeField] private bool advanceStageOnEachBattle = true; // prepara próximo após carregar
    // ==============================================================================
    
    [Header("Merchant UI")]
    [SerializeField] private MerchantUI merchantUI;

    [Header("Combat Manager")]
    [SerializeField] private CombatManager combatManager;


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
        yield return null; // espera layout/anchors atualizarem
        LayoutRebuilder.ForceRebuildLayoutImmediate(phaseContainer.GetComponent<RectTransform>());
        SetCenterNormalizedImmediate(currentPhaseIndex);
        LoadPhase(); // só carrega stage/boss; quem inicia combate é seu botão Fight
    }

    void Update()
    {
        if (scrollRect != null)
        {
            float cur = scrollRect.horizontalNormalizedPosition;
            float next = Mathf.Lerp(cur, scrollLerpTarget, Time.deltaTime * smoothSpeed);
            scrollRect.horizontalNormalizedPosition = next;
        }

        // atalho opcional pra testar
        if (Input.GetKeyDown(KeyCode.N))
            NextPhase();
    }

    // Cria a sequência visual
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

    // Atualiza cores/tamanhos
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

    // Próxima fase
    public void NextPhase()
    {
        if (phaseIconsInUI.Count == 0) return;

        currentPhaseIndex = (currentPhaseIndex + 1) % phaseIconsInUI.Count;

        UpdateUI();
        CenterOnCurrentPhase();
        LoadPhase(); // só spawna inimigos; nada de iniciar/encerrar combate aqui
    }

    // Lógica ao entrar na fase atual
    void LoadPhase()
    {
        var type = pattern[currentPhaseIndex % pattern.Length];
        Debug.Log($"Entrando na fase {currentPhaseIndex + 1}: {type}");

        // Regenera buffs das cartas do jogador ao mudar de fase
        RegeneratePlayerBuffs();

        if (stageManager == null) return;

        if (type == PhaseType.Battle)
        {
            int totalStages = Mathf.Max(1, stageManager.stages.Count);
            int toLoad = Mathf.Clamp(stageCounter, 0, totalStages - 1);

            // Carrega INIMIGOS da batalha (sem iniciar combate)
            stageManager.SetStage(toLoad);
            Debug.Log($"[PhaseManager] Carregando Stage {toLoad} (sem iniciar combate).");

            // Prepara QUAL será o próximo stage normal
            if (advanceStageOnEachBattle && totalStages > 0)
            {
                stageCounter = (toLoad + 1) % totalStages;
                Debug.Log($"[PhaseManager] Próximo Stage preparado = {stageCounter}.");
            }
        }
        else if (type == PhaseType.Boss)
        {
            int totalBoss = (stageManager.bossStages != null) ? stageManager.bossStages.Count : 0;
            Debug.Log($"[PhaseManager] Entrou em BOSS. bossStages.Count = {totalBoss}, bossCounter = {bossCounter}");

            if (totalBoss > 0)
            {
                int toLoadBoss = Mathf.Clamp(bossCounter, 0, totalBoss - 1);
                // carrega no próximo frame para evitar corrida com scroll/layout/limpezas
                StartCoroutine(LoadBossDeferred(toLoadBoss, totalBoss));
            }
            else
            {
                Debug.LogWarning("[PhaseManager] Não há bossStages configurados no StageManager.");
            }
        }
        if (type == PhaseType.Merchant)
        {
            merchantUI.ShowMerchant();
        }
        else
        {
            merchantUI.HideMerchant();
        }
        if (merchantUI != null)
        {
            merchantUI.gameObject.SetActive(type == PhaseType.Merchant);
        }

    }

    // Delay de 1 frame para spawnar boss com segurança
    IEnumerator LoadBossDeferred(int toLoadBoss, int totalBoss)
    {
        yield return null;

        if (stageManager != null)
        {
            stageManager.SetBossStage(toLoadBoss);
            Debug.Log($"[PhaseManager] Carregado BOSS Stage {toLoadBoss} (deferred).");

            // prepara próximo boss (opcional)
            bossCounter = (toLoadBoss + 1) % Mathf.Max(1, totalBoss);
        }
    }

    // Helpers de UI/scroll
    Sprite GetSpriteForType(PhaseType type)
    {
        foreach (var p in phaseIcons)
            if (p.type == type)
                return p.icon;
        return null;
    }

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

    // Helpers públicos (se quiser controlar por botões/UI)
    public void SetStageManager(StageManager sm) => stageManager = sm;

    public void SetStageCounter(int index, bool clampToAvailable = true)
    {
        if (!clampToAvailable || stageManager == null || stageManager.stages.Count == 0)
            stageCounter = index;
        else
            stageCounter = Mathf.Clamp(index, 0, stageManager.stages.Count - 1);

        Debug.Log($"[PhaseManager] stageCounter definido para {stageCounter}.");
    }

    public void SetBossCounter(int index, bool clampToAvailable = true)
    {
        if (!clampToAvailable || stageManager == null || stageManager.bossStages.Count == 0)
            bossCounter = index;
        else
            bossCounter = Mathf.Clamp(index, 0, stageManager.bossStages.Count - 1);

        Debug.Log($"[PhaseManager] bossCounter definido para {bossCounter}.");
    }

    /// <summary>
    /// Regenera todos os buffs das cartas do jogador
    /// </summary>
    private void RegeneratePlayerBuffs()
    {
        if (combatManager == null)
        {
#if UNITY_2023_1_OR_NEWER
            combatManager = FindFirstObjectByType<CombatManager>();
#else
            combatManager = FindObjectOfType<CombatManager>();
#endif
        }

        if (combatManager != null && combatManager.tabuleiroB != null)
        {
            var allBuffSystems = combatManager.tabuleiroB.GetComponentsInChildren<BuffSystem>(true);
            foreach (var bs in allBuffSystems)
            {
                if (bs == null) continue;
                // Regenera escudos e restaura outros buffs
                bs.RegenerateShield();
                bs.RestoreAllBuffs();
            }
            Debug.Log($"[PhaseManager] Regenerou buffs de {allBuffSystems.Length} cartas do jogador.");
        }
    }
}
