using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardCombat : MonoBehaviour
{
    [Header("Atributos de Combate")]
    public int damage = 1;
    public float attackInterval = 1.5f;
    public float projectileSpeed = 6f;

    [Header("Projétil da Carta")]
    [Tooltip("Projétil específico desta carta (opcional). Se vazio, usará o padrão do CombatManager.")]
    public GameObject projectilePrefab;

    [Header("Ataque em Múltiplos Projetéis")]
    [Tooltip("Número de projéteis disparados por ataque.")]
    public int projectilesPerAttack = 1;

    [HideInInspector] public bool IsAlive = true;

    private Transform currentTarget;
    private Coroutine attackLoop;

    public void BeginCombat(Transform enemyBoard)
    {
        if (!IsAlive) return;

        currentTarget = GetRandomTarget(enemyBoard);
        if (attackLoop != null)
            StopCoroutine(attackLoop);

        attackLoop = StartCoroutine(AttackLoop(enemyBoard));
    }

    public void EndCombat()
    {
        if (attackLoop != null)
            StopCoroutine(attackLoop);
    }

    private IEnumerator AttackLoop(Transform enemyBoard)
    {
        while (IsAlive)
        {
            // Se o alvo atual morreu, escolher outro
            if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
            {
                currentTarget = GetRandomTarget(enemyBoard);
                if (currentTarget == null)
                {
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }
            }

            FireProjectiles(enemyBoard);
            yield return new WaitForSeconds(attackInterval);
        }
    }

    private void FireProjectiles(Transform enemyBoard)
    {
        GameObject prefab = projectilePrefab != null
            ? projectilePrefab
            : Resources.Load<GameObject>("DefaultProjectile");

        if (prefab == null)
        {
            Debug.LogWarning($"{name} tentou atacar mas não tem projétil definido nem padrão encontrado.");
            return;
        }

        // Pega inimigos válidos
        List<Transform> validTargets = GetValidTargets(enemyBoard);
        if (validTargets.Count == 0) return;

        // Se só há um inimigo, lança apenas 1 projétil
        int projectileCount = Mathf.Min(projectilesPerAttack, validTargets.Count);

        // 🔹 Dispara múltiplos projéteis instantaneamente, cada um com alvo diferente
        for (int i = 0; i < projectileCount; i++)
        {
            Transform target = validTargets[i];

            Vector3 spawnPos = transform.position + Vector3.up * 0.4f;
            var projObj = Instantiate(prefab, spawnPos, Quaternion.identity);
            var proj = projObj.GetComponent<Projectile>();

            if (proj != null)
                proj.Initialize(target, damage, projectileSpeed);
        }
    }

    private List<Transform> GetValidTargets(Transform enemyBoard)
    {
        List<Transform> validTargets = new List<Transform>();

        if (enemyBoard == null)
            return validTargets;

        var enemies = enemyBoard.GetComponentsInChildren<CardCombat>(true);
        foreach (var e in enemies)
        {
            if (e != null && e.IsAlive && e.gameObject.activeInHierarchy)
                validTargets.Add(e.transform);
        }

        return validTargets;
    }

    private Transform GetRandomTarget(Transform enemyBoard)
    {
        var validTargets = GetValidTargets(enemyBoard);
        if (validTargets.Count == 0) return null;
        return validTargets[Random.Range(0, validTargets.Count)];
    }

    public void OnDie()
    {
        IsAlive = false;
        gameObject.SetActive(false);
    }
}
