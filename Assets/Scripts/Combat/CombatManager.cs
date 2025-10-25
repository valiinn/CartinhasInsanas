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
    public float fightDuration = 60f;
    public GameObject projectilePrefab;

    [Header("Projectile Layer & Parent")]
    public string projectileLayerName = "Projectile";
    public Transform projectileParent; // Canvas ou objeto específico para FX

    [HideInInspector]
    public bool inCombat = false;

    void Awake()
    {
        Instance = this;

        // Cria um parent padrão se não foi setado
        if (projectileParent == null)
        {
            GameObject fxParent = GameObject.Find("ProjectileCanvas");
            if (fxParent == null)
                fxParent = new GameObject("ProjectileCanvas");

            projectileParent = fxParent.transform;
        }
    }

    // 🔹 Chamado pelo botão
    public void StartCombatFromButton()
    {
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

    public void StartCombat()
    {
        if (inCombat) return;
        StartCoroutine(CombatRoutine());
    }

    public void EndCombatFromButton()
    {
        StopAllCoroutines();
        EndCombatLogic();
        Debug.Log("Combate encerrado manualmente pelo botão!");
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
    }

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

        var projObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity, projectileParent);
        projObj.layer = LayerMask.NameToLayer(projectileLayerName);

        var proj = projObj.GetComponent<Projectile>();
        if (proj != null)
            proj.Initialize(target, damage, speed);
    }

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
        // Exemplo: desabilitar interação durante combate
    }

    public bool CanPlaceOnBoard(Transform board)
    {
        int count = board.GetComponentsInChildren<CardCombat>(true)
            .Count(c => c != null && c.gameObject.activeInHierarchy);
        return count < maxCardsPerBoard;
    }
}
