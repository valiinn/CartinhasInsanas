using System.Collections;
using System.Linq;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [Header("Boards")]
    public Transform tabuleiroA;
    public Transform tabuleiroB;

    [Header("Settings")]
    public int maxCardsPerBoard = 4;
    public float fightDuration = 60f; // timeout safety
    public GameObject projectilePrefab;

    [HideInInspector]
    public bool inCombat = false;

    void Awake()
    {
        Instance = this;
    }

    // 🔹 Chamado pelo botão
    public void StartCombatFromButton()
    {
        // Só inicia se não estiver em combate
        if (!inCombat)
        {
            StartCombat();
            Debug.Log("Combate iniciado pelo botão!");
        }
        else
        {
            Debug.LogWarning("Combate já está em andamento.");
        }
    }

    // 🔹 Método de iniciar combate (usado tanto por botão quanto internamente)
    public void StartCombat()
    {
        if (inCombat) return;
        StartCoroutine(CombatRoutine());
    }

    // 🔹 (Opcional) Método pra encerrar combate via botão
    public void EndCombatFromButton()
    {
        StopAllCoroutines();
        EndCombatLogic();
        Debug.Log("Combate encerrado manualmente pelo botão!");
    }

    IEnumerator CombatRoutine()
    {
        inCombat = true;

        // Desativa interações durante o combate (drag, etc)
        ToggleInputForAll(false);

        // Manda todas as cartas iniciarem ataque
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
    }

    // 🔹 Lógica de fim do combate
    void EndCombatLogic()
    {
        foreach (var c in GetActiveCardCombats(tabuleiroA)) c.EndCombat();
        foreach (var c in GetActiveCardCombats(tabuleiroB)) c.EndCombat();

        ToggleInputForAll(true);
        inCombat = false;
        Debug.Log("Combate encerrado!");
    }

    public void SpawnProjectile(Vector3 spawnPos, Transform target, int damage, float speed)
    {
        if (projectilePrefab == null) return;
        var projObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity, transform);
        var proj = projObj.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.Initialize(target, damage, speed);
        }
    }

    // Helpers
    public CardCombat[] GetActiveCardCombats(Transform board)
    {
        return board.GetComponentsInChildren<CardCombat>().Where(c => c != null && c.IsAlive).ToArray();
    }

    bool BothSidesHaveAlive(Transform a, Transform b)
    {
        return GetActiveCardCombats(a).Length > 0 && GetActiveCardCombats(b).Length > 0;
    }

    void ToggleInputForAll(bool enabled)
    {
        // Aqui você pode desabilitar scripts de interação enquanto o combate acontece
        // Exemplo: arraste todos objetos com "DragHandler" e desative-os aqui.
    }

    // 🔹 Checa se pode colocar carta no board (máx 4)
    public bool CanPlaceOnBoard(Transform board)
    {
        int count = board.GetComponentsInChildren<CardCombat>(true)
            .Count(c => c != null && c.gameObject.activeInHierarchy);
        return count < maxCardsPerBoard;
    }
}
