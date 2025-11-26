using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Permite que outros scripts interceptem o dano antes de ele ser aplicado
public delegate void BeforeDamageEvent(CardHealth sender, ref int amount);

public class CardHealth : MonoBehaviour
{
    [Header("HP Colors")]
    public Color fullHpColor = Color.green;
    public Color halfHpColor = new Color(1f, 0.65f, 0f); // laranja
    public Color lowHpColor = Color.red;

    [Header("Vida")]
    public int maxHp = 5;

    [SerializeField] private int hp;
    public int CurrentHp => hp;
    public bool IsDead => hp <= 0 || !gameObject.activeInHierarchy;

    [Header("UI (opcional)")]
    public Image hpBar;       // <- arraste aqui a imagem da barra (Type = Filled)
    public TMP_Text hpText;   // <- texto opcional (valor atual da vida)

    // Evento para o BuffSystem interceptar dano (ex: escudo)
    public event BeforeDamageEvent OnBeforeTakeDamage;

    private void Awake()
    {
        hp = maxHp;
        UpdateUI();
    }

    // ===============================
    // 💥 Dano e morte
    // ===============================
    public void TakeDamage(int amount)
    {
        // Permite que BuffSystem (ou outros sistemas) alterem o dano
        OnBeforeTakeDamage?.Invoke(this, ref amount);

        // Se o buff (ex: escudo) cancelou o dano, não faz nada
        if (amount <= 0)
            return;

        hp -= amount;

        if (hp <= 0)
            Die();
        else
            UpdateUI();
    }

    private void Die()
    {
        var combat = GetComponent<CardCombat>();
        if (combat != null)
            combat.OnDie();

        // Desativa visualmente
        gameObject.SetActive(false);
    }

    // ===============================
    // 💚 Cura e reviver
    // ===============================
    public void ReviveAndHeal()
    {
        hp = maxHp;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        var combat = GetComponent<CardCombat>();
        if (combat != null)
            combat.IsAlive = true;

        // Restaura todos os buffs quando a carta renasce
        var buffSystem = GetComponent<BuffSystem>();
        if (buffSystem != null)
        {
            buffSystem.RestoreAllBuffs();
        }

        UpdateUI();
    }

    // ===============================
    // 💚 Cura parcial (opcional)
    // ===============================
    public void Heal(int amount)
    {
        hp = Mathf.Min(hp + amount, maxHp);
        UpdateUI();
    }

    // ===============================
    // 🎨 Atualização de interface
    // ===============================
    private void UpdateUI()
    {
        if (hpBar != null)
        {
            float fill = (float)hp / maxHp;
            hpBar.fillAmount = Mathf.Clamp01(fill);

            // muda a cor da barra conforme o percentual
            if (fill > 0.5f)
                hpBar.color = fullHpColor;
            else if (fill > 0.25f)
                hpBar.color = halfHpColor;
            else
                hpBar.color = lowHpColor;
        }

        if (hpText != null)
            hpText.text = hp.ToString();
    }
}
