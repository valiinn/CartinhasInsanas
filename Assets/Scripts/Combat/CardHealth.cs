using UnityEngine;

public class CardHealth : MonoBehaviour
{
    public int maxHp = 5;

    [SerializeField] private int hp;
    public int CurrentHp => hp;
    public bool IsDead => hp <= 0 || !gameObject.activeInHierarchy;

    void Awake()
    {
        hp = maxHp;
    }

    public void TakeDamage(int amount)
    {
        hp -= amount;
        if (hp <= 0) Die();
    }

    void Die()
    {
        var combat = GetComponent<CardCombat>();
        if (combat != null) combat.OnDie();

        // desativa visualmente
        gameObject.SetActive(false);
    }

    // >>> NOVO: Reviver e curar tudo para a próxima rodada
    public void ReviveAndHeal()
    {
        hp = maxHp;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        var combat = GetComponent<CardCombat>();
        if (combat != null)
            combat.IsAlive = true;
    }
}
