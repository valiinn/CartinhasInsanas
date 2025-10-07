using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CardHealth))]
public class CardCombat : MonoBehaviour
{
    [Header("Combat stats")]
    public int damage = 1;
    public float attackCooldown = 1.2f;
    public float projectileSpeed = 400f;
    public Transform projectileSpawnPoint;

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
            var target = FindClosestEnemy();
            if (target != null)
                SpawnProjectileAt(target);

            yield return new WaitForSeconds(attackCooldown);
        }
    }

    Transform FindClosestEnemy()
    {
        if (enemyBoard == null) return null;

        var enemyCards = enemyBoard.GetComponentsInChildren<CardCombat>();
        Transform best = null;
        float bestDist = float.MaxValue;

        foreach (var ec in enemyCards)
        {
            if (ec == null || !ec.IsAlive) continue;
            float d = Vector3.Distance(transform.position, ec.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = ec.transform;
            }
        }

        return best;
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
