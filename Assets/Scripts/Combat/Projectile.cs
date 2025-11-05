using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private int damage;
    private float speed;
    private Animator anim;
    private bool hasHit = false;
    private bool isFlying = false;

    [Header("Configurações")]
    [Tooltip("Offset de rotação para sprites que apontam para cima (geralmente -90 ou +90)")]
    [SerializeField] private float rotationOffset = -90f;

    [Tooltip("Duração da animação de spawn antes de começar a se mover")]
    [SerializeField] private float spawnDelay = 0.20f;

    [Header("Estados do Animator")]
    public string spawnState = "Projectile_Spawn";
    public string trajectoryState = "Projectile_Trajectory";
    public string impactState = "Projectile_Impact";

    public void Initialize(Transform target, int damage, float speed)
    {
        this.target = target;
        this.damage = damage;
        this.speed = speed;
        anim = GetComponent<Animator>();

        // Garante que o projétil fique dentro do parent certo (ProjectileCanvas)
        if (CombatManager.Instance != null && CombatManager.Instance.projectileParent != null)
        {
            transform.SetParent(CombatManager.Instance.projectileParent, true);
        }

        // Define layer de renderização
        int layerIndex = LayerMask.NameToLayer("Projectiles");
        if (layerIndex >= 0 && layerIndex < 32)
        {
            gameObject.layer = layerIndex;
            foreach (Transform child in transform.GetComponentsInChildren<Transform>(true))
                child.gameObject.layer = layerIndex;
        }

        // Inicia a sequência de spawn e movimento
        StartCoroutine(SpawnAndFly());
    }

    private IEnumerator SpawnAndFly()
    {
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            if (anim.HasState(0, Animator.StringToHash(spawnState)))
                anim.Play(spawnState, 0, 0f);
        }

        yield return new WaitForSeconds(spawnDelay);

        isFlying = true;

        if (anim != null && anim.runtimeAnimatorController != null)
        {
            if (anim.HasState(0, Animator.StringToHash(trajectoryState)))
                anim.Play(trajectoryState, 0, 0f);
        }
    }

    private void Update()
    {
        if (!isFlying || hasHit || target == null)
            return;

        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffset;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (Vector3.Distance(transform.position, target.position) < 0.25f)
        {
            hasHit = true;
            StartCoroutine(ImpactSequence());
        }
    }

    private IEnumerator ImpactSequence()
    {
        isFlying = false;

        if (anim != null && anim.runtimeAnimatorController != null)
        {
            if (anim.HasState(0, Animator.StringToHash(impactState)))
                anim.Play(impactState, 0, 0f);
        }

        var health = target.GetComponent<CardHealth>();
        if (health != null)
            health.TakeDamage(damage);

        yield return new WaitForSeconds(0.4f);
        Destroy(gameObject);
    }
}
