using System.Collections;
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

    [HideInInspector]
    public bool IsAlive = true;

    private Transform currentTarget;
    private Coroutine attackLoop;

    public void BeginCombat(Transform enemyBoard)
    {
        if (!IsAlive) return;
        currentTarget = GetRandomTarget(enemyBoard);
        if (currentTarget != null)
            attackLoop = StartCoroutine(AttackLoop());
    }

    public void EndCombat()
    {
        if (attackLoop != null)
            StopCoroutine(attackLoop);
    }

    private IEnumerator AttackLoop()
    {
        while (IsAlive && currentTarget != null)
        {
            yield return new WaitForSeconds(attackInterval);
            FireProjectile();
        }
    }

    private void FireProjectile()
    {
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
            return;

        // Escolhe o projétil da carta ou o global
        GameObject prefab = projectilePrefab != null
            ? projectilePrefab
            : CombatManager.Instance != null && CombatManager.Instance.projectileParent != null
                ? Resources.Load<GameObject>("DefaultProjectile") // opcional: tenta carregar um placeholder
                : null;

        if (prefab == null)
        {
            Debug.LogWarning($"{name} tentou atacar mas não tem projétil definido nem padrão encontrado.");
            return;
        }

        Vector3 spawnPos = transform.position + Vector3.up * 0.4f;

        var projObj = Instantiate(prefab, spawnPos, Quaternion.identity);
        var proj = projObj.GetComponent<Projectile>();

        if (proj != null)
        {
            proj.Initialize(currentTarget, damage, projectileSpeed);
        }
    }

    private Transform GetRandomTarget(Transform enemyBoard)
    {
        if (enemyBoard == null) return null;

        var enemies = enemyBoard.GetComponentsInChildren<CardCombat>();
        if (enemies.Length == 0) return null;

        return enemies[Random.Range(0, enemies.Length)].transform;
    }

    public void OnDie()
    {
        IsAlive = false;
        gameObject.SetActive(false);
    }
}
