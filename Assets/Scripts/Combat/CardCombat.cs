using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CardHealth))]
public class CardCombat : MonoBehaviour
{
    [Header("Combat Stats")]
    public int damage = 1;
    public float attackCooldown = 1.2f;
    public float projectileSpeed = 5f;
    public Transform projectileSpawnPoint;

    [Header("Multi-shot Settings")]
    [Tooltip("Quantos projéteis essa carta dispara por ataque.")]
    public int projectileCount = 1;

    [Tooltip("Se true, evita disparar 2 projéteis no mesmo alvo.")]
    public bool uniqueTargets = true;

    [HideInInspector] public bool IsAlive = true;

    private Coroutine attackRoutine;
    private Transform enemyBoard;

    public void BeginCombat(Transform opposingBoard)
    {
        enemyBoard = opposingBoard;
        IsAlive = true;

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(AttackLoop());
    }

    public void EndCombat()
    {
        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = null;
    }

    IEnumerator AttackLoop()
    {
        yield return new WaitForSeconds(Random.Range(0f, 0.3f));

        while (IsAlive && CombatManager.Instance != null && CombatManager.Instance.inCombat)
        {
            FireProjectiles();
            yield return new WaitForSeconds(attackCooldown);
        }
    }

    void FireProjectiles()
    {
        if (enemyBoard == null) return;

        var enemyCards = new List<CardCombat>(enemyBoard.GetComponentsInChildren<CardCombat>());
        enemyCards.RemoveAll(c => c == null || !c.IsAlive);

        if (enemyCards.Count == 0) return;

        // Embaralha lista de inimigos pra aleatoriedade
        for (int i = 0; i < enemyCards.Count; i++)
        {
            var temp = enemyCards[i];
            int randomIndex = Random.Range(i, enemyCards.Count);
            enemyCards[i] = enemyCards[randomIndex];
            enemyCards[randomIndex] = temp;
        }

        int shots = Mathf.Min(projectileCount, enemyCards.Count);

        for (int i = 0; i < shots; i++)
        {
            Transform target;

            if (uniqueTargets)
                target = enemyCards[i].transform; // sem repetir
            else
                target = enemyCards[Random.Range(0, enemyCards.Count)].transform; // pode repetir

            SpawnProjectileAt(target);
        }
    }

    void SpawnProjectileAt(Transform target)
    {
        Vector3 spawnPos = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
        CombatManager.Instance.SpawnProjectile(spawnPos, target, damage, projectileSpeed);
    }

    public void OnDie()
    {
        IsAlive = false;
        EndCombat();
    }
}
