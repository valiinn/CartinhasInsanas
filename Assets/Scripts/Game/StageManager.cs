using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    [Header("Board do INIMIGO")]
    [Tooltip("Pai que contém as casas (DropSlot) do board do inimigo.")]
    public Transform enemyBoard;

    [Tooltip("Opcional: se deixar vazio, os slots serão coletados automaticamente no enemyBoard.")]
    public List<RectTransform> enemySlots = new List<RectTransform>();

    [Header("Fases Normais")]
    public List<StageConfig> stages = new List<StageConfig>();

    [Header("Fases de Boss")]
    public List<StageConfig> bossStages = new List<StageConfig>();

    [Min(0)]
    public int currentStage = 0;

    [Header("Comportamento")]
    [Tooltip("Se true, as cartas inimigas não recebem eventos de input (não arrastáveis).")]
    public bool lockEnemyInput = true;

    [Tooltip("Semente para randomização (0 = aleatório do sistema).")]
    public int randomSeed = 0;

    private int lastLoadedStage = -1;

    void Start()
    {
        EnsureSlots();
        LoadStage(currentStage);
    }

    // ================== NORMAL ==================
    public void SetStage(int stageIndex)
    {
        if (stages == null || stages.Count == 0)
        {
            Debug.LogWarning("StageManager: lista 'stages' vazia.");
            return;
        }

        stageIndex = Mathf.Clamp(stageIndex, 0, stages.Count - 1);
        LoadConfig(stages[stageIndex]);
        lastLoadedStage = stageIndex;
        currentStage = stageIndex;
        Debug.Log($"StageManager: carregou fase {currentStage}.");
    }

    public void LoadStage(int stageIndex)
    {
        if (stages == null || stages.Count == 0) return;
        stageIndex = Mathf.Clamp(stageIndex, 0, stages.Count - 1);
        SetStage(stageIndex);
    }

    // ================== BOSS ====================
    public void SetBossStage(int bossIndex)
    {
        if (bossStages == null || bossStages.Count == 0)
        {
            Debug.LogWarning("StageManager: lista 'bossStages' vazia.");
            return;
        }

        bossIndex = Mathf.Clamp(bossIndex, 0, bossStages.Count - 1);
        LoadConfig(bossStages[bossIndex]);
        Debug.Log($"StageManager: carregou BOSS stage {bossIndex}.");
    }

    // =========== Núcleo de carregamento =========
    private void LoadConfig(StageConfig cfg)
    {
        if (enemyBoard == null || cfg == null) return;
        EnsureSlots();

        // limpa todos os filhos dos slots
        foreach (var slot in enemySlots)
        {
            if (slot == null) continue;
            for (int i = slot.childCount - 1; i >= 0; i--)
                Destroy(slot.GetChild(i).gameObject);
        }

        // embaralha slots para posições aleatórias (sem repetição)
        var bag = new List<RectTransform>(enemySlots);
        Shuffle(bag, randomSeed);

        int count = Mathf.Min(cfg.enemies.Count, bag.Count);
        for (int i = 0; i < count; i++)
        {
            var ep = cfg.enemies[i];
            if (ep == null || ep.enemyPrefab == null) continue;

            var slot = bag[i];
            SpawnEnemyAt(ep.enemyPrefab, slot);
        }
    }

    private void SpawnEnemyAt(GameObject prefab, RectTransform slot)
    {
        var go = Instantiate(prefab, slot);
        NormalizeForSlot(go.transform as RectTransform, slot);

        if (lockEnemyInput)
        {
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false; // não clicável/arrastável
        }
    }

    private void NormalizeForSlot(RectTransform child, RectTransform slot)
    {
        if (child == null || slot == null) return;

        child.localScale = Vector3.one;
        child.localRotation = Quaternion.identity;
        child.pivot = new Vector2(0.5f, 0.5f);
        child.anchorMin = new Vector2(0.5f, 0.5f);
        child.anchorMax = new Vector2(0.5f, 0.5f);
        child.anchoredPosition = Vector2.zero;

        var grid = slot.parent ? slot.parent.GetComponent<GridLayoutGroup>() : null;
        if (grid != null) child.sizeDelta = grid.cellSize;
    }

    private void EnsureSlots()
    {
        if (enemySlots != null && enemySlots.Count > 0) return;
        enemySlots = AutoCollectSlots(enemyBoard);
        if (enemySlots.Count == 0)
            Debug.LogWarning("StageManager: nenhum DropSlot encontrado no enemyBoard.");
    }

    // coleta todos os DropSlot e ordena top→bottom, left→right (para uma ordem estável antes do shuffle)
    private List<RectTransform> AutoCollectSlots(Transform board)
    {
        var slots = board.GetComponentsInChildren<DropSlot>(true)
                         .Select(ds => ds.transform as RectTransform)
                         .Where(rt => rt != null)
                         .ToList();

        slots = slots.OrderByDescending(rt => rt.localPosition.y)
                     .ThenBy(rt => rt.localPosition.x)
                     .ToList();
        return slots;
    }

    // Fisher–Yates
    private void Shuffle<T>(IList<T> list, int seed)
    {
        System.Random rng = (seed == 0) ? new System.Random() : new System.Random(seed);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>
    /// Retorna todos os CardHealth atualmente presentes no board inimigo (enemySlots).
    /// Usado para verificar se restam cartas vivas do boss.
    /// </summary>
    public List<CardHealth> GetCurrentEnemyCardHealths()
    {
        List<CardHealth> list = new List<CardHealth>();

        if (enemySlots == null || enemySlots.Count == 0) return list;

        foreach (var slot in enemySlots)
        {
            if (slot == null) continue;
            for (int i = 0; i < slot.childCount; i++)
            {
                    var ch = slot.GetChild(i).GetComponent<CardHealth>();
                    if (ch != null)
                {
                    list.Add(ch);
                    continue;
                }

                // fallback: se o prefab tiver Card (UI) e CardHealth estiver em outro child, tenta localizar
                var card = slot.GetChild(i).GetComponent<Card>();
                if (card != null)
                {
                    var chChild = slot.GetChild(i).GetComponentInChildren<CardHealth>(true);
                    if (chChild != null) list.Add(chChild);
                }
            }
        }

        return list;
    }
}
