using UnityEngine;

public class CardHealth : MonoBehaviour
{
    public int maxHp = 5;
    private int hp;

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

        // desativa visualmente (ou destrói)
        gameObject.SetActive(false);
    }
}
