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
    [SerializeField] private float spawnDelay = 0.25f;

    public void Initialize(Transform target, int damage, float speed)
    {
        this.target = target;
        this.damage = damage;
        this.speed = speed;
        anim = GetComponent<Animator>();

        // Garante que o projétil sempre renderize na layer correta
        int layerIndex = LayerMask.NameToLayer("Projectiles");
        if (layerIndex >= 0 && layerIndex < 32)
        {
            gameObject.layer = layerIndex;
            foreach (Transform child in transform.GetComponentsInChildren<Transform>(true))
                child.gameObject.layer = layerIndex;
        }
        else
        {
            Debug.LogWarning("⚠️ Layer 'Projectiles' não encontrada, usando Default.");
        }

        // Inicia a sequência de spawn + voo
        StartCoroutine(SpawnAndFly());
    }

    private IEnumerator SpawnAndFly()
    {
        // Toca a animação de surgimento (spawn)
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            anim.Play("Projectile_Spawn", 0, 0f);
        }

        // Espera a animação de spawn completar
        yield return new WaitForSeconds(spawnDelay);

        // Começa o voo e transiciona para a animação de trajetória
        isFlying = true;

        if (anim != null && anim.runtimeAnimatorController != null)
        {
            // Usa CrossFade ou SetTrigger para manter a animação em loop
            anim.Play("Projectile_Trajectory", 0, 0f);
        }
    }

    private void Update()
    {
        if (!isFlying || hasHit || target == null)
            return;

        // Move em direção ao alvo
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Rotaciona o projétil para apontar na direção do movimento (com offset)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffset;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Checa colisão aproximada com o alvo
        if (Vector3.Distance(transform.position, target.position) < 0.25f)
        {
            hasHit = true;
            StartCoroutine(ImpactSequence());
        }
    }

    private IEnumerator ImpactSequence()
    {
        // Para o movimento do projétil
        isFlying = false;

        // Toca animação de impacto
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            anim.Play("Projectile_Impact", 0, 0f);
        }

        // Aplica dano no alvo
        var health = target.GetComponent<CardHealth>();
        if (health != null)
            health.TakeDamage(damage);

        // Espera a animação de impacto terminar
        yield return new WaitForSeconds(0.4f);

        Destroy(gameObject);
    }
}