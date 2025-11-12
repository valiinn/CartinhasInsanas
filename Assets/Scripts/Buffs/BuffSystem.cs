using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffType
{
    Shield,
    Damage,
    Speed
}

public class BuffSystem : MonoBehaviour
{
    [Header("Configuração do Buff")]
    public List<BuffType> activeBuffs = new List<BuffType>();

    [Header("Referências")]
    private CardCombat cardCombat;
    private CardHealth cardHealth;
    private GameObject shieldVisual; // objeto visual do escudo

    private bool hasShield = false;
    private bool isSpeedBuffed = false;
    private bool isDamageBuffed = false;

    void Awake()
    {
        cardCombat = GetComponent<CardCombat>();
        cardHealth = GetComponent<CardHealth>();
    }

    public void ApplyBuff(BuffType type)
    {
        if (activeBuffs.Contains(type))
            return;

        activeBuffs.Add(type);

        switch (type)
        {
            case BuffType.Shield:
                ApplyShield();
                break;
            case BuffType.Damage:
                ApplyDamageBuff();
                break;
            case BuffType.Speed:
                ApplySpeedBuff();
                break;
        }
    }

    public void RemoveBuff(BuffType type)
    {
        if (!activeBuffs.Contains(type))
            return;

        activeBuffs.Remove(type);

        switch (type)
        {
            case BuffType.Shield:
                RemoveShield();
                break;
            case BuffType.Damage:
                RemoveDamageBuff();
                break;
            case BuffType.Speed:
                RemoveSpeedBuff();
                break;
        }
    }

    // 🛡 ESCUDO — bloqueia o primeiro dano
    void ApplyShield()
    {
        if (hasShield || cardHealth == null) return;

        hasShield = true;

        // cria o visual do escudo
        shieldVisual = new GameObject("ShieldVisual");
        var sr = shieldVisual.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>("ShieldGlow"); // sprite opcional (adicione em Assets/Resources)
        sr.color = new Color(0.4f, 0.7f, 1f, 0.4f);
        shieldVisual.transform.SetParent(transform, false);
        shieldVisual.transform.localPosition = Vector3.zero;
        shieldVisual.transform.localScale = Vector3.one * 1.2f;

        // intercepta dano
        cardHealth.OnBeforeTakeDamage += HandleShieldBlock;

        Debug.Log($"{name} recebeu BUFF: Escudo ativo!");
    }

    void HandleShieldBlock(CardHealth sender, ref int damage)
    {
        if (!hasShield) return;

        Debug.Log($"{name} bloqueou um ataque com o escudo!");
        damage = 0; // bloqueia totalmente
        RemoveShield();
    }

    void RemoveShield()
    {
        hasShield = false;

        if (shieldVisual != null)
            Destroy(shieldVisual);

        cardHealth.OnBeforeTakeDamage -= HandleShieldBlock;
    }

    // ⚔️ DANO +2
    void ApplyDamageBuff()
    {
        if (isDamageBuffed || cardCombat == null) return;
        cardCombat.damage += 2;
        isDamageBuffed = true;
        Debug.Log($"{name} recebeu BUFF: +2 de dano!");
    }

    void RemoveDamageBuff()
    {
        if (!isDamageBuffed || cardCombat == null) return;
        cardCombat.damage -= 2;
        isDamageBuffed = false;
    }

    // ⚡ VELOCIDADE
    void ApplySpeedBuff()
    {
        if (isSpeedBuffed || cardCombat == null) return;
        cardCombat.attackInterval = Mathf.Max(0.4f, cardCombat.attackInterval * 0.7f); // reduz tempo entre ataques
        isSpeedBuffed = true;
        Debug.Log($"{name} recebeu BUFF: Velocidade de ataque aumentada!");
    }

    void RemoveSpeedBuff()
    {
        if (!isSpeedBuffed || cardCombat == null) return;
        cardCombat.attackInterval /= 0.7f;
        isSpeedBuffed = false;
    }
}
