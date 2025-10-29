using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardHealth : MonoBehaviour
{
    [Header("Vida")]
    public int maxHp = 5;

    [SerializeField] private int hp;
    public int CurrentHp => hp;
    public bool IsDead => hp <= 0 || !gameObject.activeInHierarchy;

    [Header("UI (opcional)")]
    public Image hpBar;       // <- arraste aqui a imagem da barra (Type = Filled)
    public TMP_Text hpText;   // <- opcional, texto tipo "3/5"

    void Awake()
    {
        hp = maxHp;
        UpdateUI();
    }

    public void TakeDamage(int amount)
    {
        hp -= amount;
        if (hp <= 0) Die();
        UpdateUI();
    }

    void Die()
    {
        var combat = GetComponent<CardCombat>();
        if (combat != null) combat.OnDie();

        // desativa visualmente
        gameObject.SetActive(false);
    }

    public void ReviveAndHeal()
    {
        hp = maxHp;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        var combat = GetComponent<CardCombat>();
        if (combat != null)
            combat.IsAlive = true;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (hpBar != null)
        {
            float fill = (float)hp / maxHp;
            hpBar.fillAmount = Mathf.Clamp01(fill);
        }

        if (hpText != null)
        {
            hpText.text = $"{hp}/{maxHp}";
        }
    }
}
