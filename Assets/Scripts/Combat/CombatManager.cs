using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [Header("Boards")]
    public Transform tabuleiroA; // inimigo
    public Transform tabuleiroB; // jogador

    [Header("Settings")]
    public int maxCardsPerBoard = 4;
    public float fightDuration = 60f;

    [Header("Projectile Layer & Parent")]
    public string projectileLayerName = "Projectile";
    public Transform projectileParent;

    [Header("Referências Externas")]
    public PhaseManager phaseManager; // vincula no Inspector

    [HideInInspector]
    public bool inCombat = false;

    [Header("Reward System")]
    public RewardSystem rewardSystem;

    void Awake()
    {
        Instance = this;

        if (projectileParent == null)
        {
            GameObject fxParent = GameObject.Find("ProjectileCanvas");
            if (fxParent == null)
                fxParent = new GameObject("ProjectileCanvas");

            projectileParent = fxParent.transform;
        }
    }

    public void StartCombatFromButton()
    {
        if (!inCombat)
        {
            StartCombat();
            Notify.Success("Combate iniciado pelo botão!");
        }
        else
        {
            Notify.Error("Combate já está em andamento.");
        }
    }

    public void StartCombat()
    {
        if (inCombat) return;
        StartCoroutine(CombatRoutine());
    }

    public void EndCombatFromButton()
    {
        StopAllCoroutines();
        EndCombatLogic();
        Debug.Log("Combate encerrado manualmente.");
    }

    IEnumerator CombatRoutine()
    {
        inCombat = true;
        ToggleInputForAll(false);

        var cardsA = GetActiveCardCombats(tabuleiroA);
        var cardsB = GetActiveCardCombats(tabuleiroB);

        foreach (var c in cardsA) c.BeginCombat(tabuleiroB);
        foreach (var c in cardsB) c.BeginCombat(tabuleiroA);

        float timer = 0f;

        while (timer < fightDuration && BothSidesHaveAlive(tabuleiroA, tabuleiroB))
        {
            timer += Time.deltaTime;
            yield return null;
        }

        EndCombatLogic();

        bool enemyAlive = GetActiveCardCombats(tabuleiroA).Length > 0;
        bool playerAlive = GetActiveCardCombats(tabuleiroB).Length > 0;

        if (playerAlive)
        {
            Debug.Log("Jogador venceu o combate.");
            yield return new WaitForSeconds(2f);

            if (phaseManager != null)
            {
                phaseManager.NextPhase();
            }
            else
            {
                Debug.LogWarning("PhaseManager não atribuído no CombatManager.");
            }
        }
        else
        {
            Debug.Log("Jogador perdeu o combate.");
        }
    }

    void EndCombatLogic()
    {
        foreach (var c in GetActiveCardCombats(tabuleiroA)) c.EndCombat();
        foreach (var c in GetActiveCardCombats(tabuleiroB)) c.EndCombat();

        bool enemyAlive = GetActiveCardCombats(tabuleiroA).Length > 0;
        bool playerAlive = GetActiveCardCombats(tabuleiroB).Length > 0;

        if (playerAlive && !enemyAlive)
        {
            Debug.Log("✅ Jogador venceu o combate!");
            GiveRewardToPlayer(); // chama o sistema de recompensa
        }
        else if (!playerAlive && enemyAlive)
        {
            Debug.Log(" Jogador perdeu o combate!");
        }
        else
        {
            Debug.Log(" Empate ou tempo acabou!");
        }

        ReviveAndHealBoard(tabuleiroB);

        

        ToggleInputForAll(true);
        inCombat = false;
        Notify.Info("Combate encerrado!");
    }

    // ✅ única forma correta de dar recompensa
    void GiveRewardToPlayer()
    {
        if (rewardSystem == null)
        {
            rewardSystem = FindFirstObjectByType<RewardSystem>();
        }

        if (rewardSystem != null)
        {
            rewardSystem.GiveWinReward();
        }
        else
        {
            Debug.LogWarning("RewardSystem não encontrado na cena!");
        }
    }


    private void ReviveAndHealBoard(Transform board)
    {
        if (board == null) return;

        var allHealth = board.GetComponentsInChildren<CardHealth>(true);
        foreach (var ch in allHealth)
        {
            if (ch == null) continue;
            ch.ReviveAndHeal();
        }

        var allCombat = board.GetComponentsInChildren<CardCombat>(true);
        foreach (var cc in allCombat)
        {
            if (cc == null) continue;
            cc.IsAlive = true;
        }

        // Regenera escudos das cartas que têm buff de escudo
        // Verifica por BuffSystem que tem definição de escudo (mesmo que consumido)
        var allBuffSystems = board.GetComponentsInChildren<BuffSystem>(true);
        foreach (var bs in allBuffSystems)
        {
            if (bs == null) continue;
            // Tenta regenerar - o método verifica internamente se tem definição de escudo
            bs.RegenerateShield();
        }
    }

    public CardCombat[] GetActiveCardCombats(Transform board)
    {
        return board.GetComponentsInChildren<CardCombat>()
            .Where(c => c != null && c.IsAlive)
            .ToArray();
    }

    bool BothSidesHaveAlive(Transform a, Transform b)
    {
        return GetActiveCardCombats(a).Length > 0 && GetActiveCardCombats(b).Length > 0;
    }

    void ToggleInputForAll(bool enabled)
    {
        // Aqui você pode desabilitar botões/interação durante o combate, se quiser
    }

    public bool CanPlaceOnBoard(Transform board)
    {
        int count = board.GetComponentsInChildren<CardCombat>(true)
            .Count(c => c != null && c.gameObject.activeInHierarchy);
        return count < maxCardsPerBoard;
    }
}

