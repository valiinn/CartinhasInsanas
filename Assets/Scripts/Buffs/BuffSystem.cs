using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    private Image shieldOverlay; // overlay UI do escudo

    // Armazena valores originais para reverter corretamente
    private float originalAttackInterval;
    private int originalDamage;
    private bool hasShield = false;
    private bool isSpeedBuffed = false;
    private bool isDamageBuffed = false;

    // Armazena as definições dos buffs aplicados
    private Dictionary<BuffType, BuffDefinition> appliedBuffDefinitions = new Dictionary<BuffType, BuffDefinition>();
    
    // Armazena contadores de escudo (para não modificar a definição original)
    private int currentShieldCount = 0;
    private int maxShieldCount = 0; // Armazena o valor máximo para regenerar

    void Awake()
    {
        cardCombat = GetComponent<CardCombat>();
        cardHealth = GetComponent<CardHealth>();

        // Guarda valores originais
        if (cardCombat != null)
        {
            originalAttackInterval = cardCombat.attackInterval;
            originalDamage = cardCombat.damage;
        }
    }

    void OnEnable()
    {
        // Quando a carta é reativada, restaura os buffs se necessário
        if (appliedBuffDefinitions.Count > 0)
        {
            StartCoroutine(RestoreBuffsOnEnable());
        }
    }

    private System.Collections.IEnumerator RestoreBuffsOnEnable()
    {
        // Espera um frame para garantir que todos os componentes estão inicializados
        yield return null;
        
        // Restaura referências se necessário
        if (cardCombat == null) cardCombat = GetComponent<CardCombat>();
        if (cardHealth == null) cardHealth = GetComponent<CardHealth>();
        
        RestoreAllBuffs();
    }

    /// <summary>
    /// Aplica um buff usando apenas o tipo (compatibilidade com código antigo)
    /// </summary>
    public void ApplyBuff(BuffType type)
    {
        ApplyBuff(type, null);
    }

    /// <summary>
    /// Aplica um buff usando BuffDefinition (método recomendado)
    /// </summary>
    public void ApplyBuff(BuffType type, BuffDefinition definition)
    {
        // Se já tem o buff ativo, não aplica novamente
        if (activeBuffs.Contains(type))
            return;

        // Se não tem definição fornecida, tenta usar uma armazenada anteriormente
        if (definition == null && appliedBuffDefinitions.TryGetValue(type, out BuffDefinition storedDef))
        {
            definition = storedDef;
        }

        activeBuffs.Add(type);

        // Armazena a definição se fornecida
        if (definition != null)
        {
            appliedBuffDefinitions[type] = definition;
        }

        switch (type)
        {
            case BuffType.Shield:
                ApplyShield(definition);
                break;
            case BuffType.Damage:
                ApplyDamageBuff(definition);
                break;
            case BuffType.Speed:
                ApplySpeedBuff(definition);
                break;
        }
    }

    public void RemoveBuff(BuffType type)
    {
        if (!activeBuffs.Contains(type))
            return;

        activeBuffs.Remove(type);
        appliedBuffDefinitions.Remove(type); // Limpa a definição

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

    // 🛡 ESCUDO — bloqueia danos configuráveis
    void ApplyShield(BuffDefinition definition = null)
    {
        if (cardHealth == null) return;

        // Usa valores do BuffDefinition ou padrões
        Color shieldColor = definition != null ? definition.shieldColor : new Color(0.4f, 0.7f, 1f, 0.4f);
        int shieldCount = definition != null ? definition.shieldCount : 1;
        
        // Se já tem escudo, apenas regenera o contador
        if (hasShield)
        {
            currentShieldCount = shieldCount;
            maxShieldCount = shieldCount;
            Debug.Log($"{name} regenerou escudo! ({currentShieldCount} bloqueio(s))");
            return;
        }

        hasShield = true;
        currentShieldCount = shieldCount;
        maxShieldCount = shieldCount;

        // Cria overlay UI que cobre toda a carta
        CreateShieldOverlay(shieldColor);

        // intercepta dano
        cardHealth.OnBeforeTakeDamage += HandleShieldBlock;

        Debug.Log($"{name} recebeu BUFF: Escudo ativo! ({currentShieldCount} bloqueio(s))");
    }

    /// <summary>
    /// Regenera o escudo para o valor máximo (útil após nova rodada)
    /// </summary>
    public void RegenerateShield()
    {
        // Verifica se tem definição de escudo (mesmo que não esteja ativo no momento)
        if (!appliedBuffDefinitions.TryGetValue(BuffType.Shield, out BuffDefinition def))
        {
            // Também verifica se está na lista de buffs ativos (pode ter sido aplicado mas definição não armazenada)
            if (!activeBuffs.Contains(BuffType.Shield))
            {
                return; // Não tem buff de escudo
            }
            // Se está na lista mas não tem definição, tenta usar valores padrão
            def = null;
        }

        // Garante que o buff está na lista ativa
        if (!activeBuffs.Contains(BuffType.Shield))
        {
            activeBuffs.Add(BuffType.Shield);
        }

        // Se não tem escudo ativo, reativa
        if (!hasShield)
        {
            hasShield = true;
            // Reativa o evento de bloqueio se necessário
            if (cardHealth != null)
            {
                cardHealth.OnBeforeTakeDamage -= HandleShieldBlock; // Remove primeiro para evitar duplicatas
                cardHealth.OnBeforeTakeDamage += HandleShieldBlock;
            }
        }

        // Restaura o contador máximo
        maxShieldCount = def != null ? def.shieldCount : 1;
        currentShieldCount = maxShieldCount;

        // Se o overlay foi destruído, recria
        if (shieldOverlay == null)
        {
            Color shieldColor = def != null ? def.shieldColor : new Color(0.4f, 0.7f, 1f, 0.4f);
            CreateShieldOverlay(shieldColor);
        }

        Debug.Log($"{name} regenerou escudo! ({currentShieldCount}/{maxShieldCount} bloqueio(s))");
    }

    /// <summary>
    /// Restaura todos os buffs que a carta tinha (útil quando renasce)
    /// </summary>
    public void RestoreAllBuffs()
    {
        if (appliedBuffDefinitions.Count == 0) return;

        // Restaura referências se necessário
        if (cardCombat == null) cardCombat = GetComponent<CardCombat>();
        if (cardHealth == null) cardHealth = GetComponent<CardHealth>();

        // Reaplica todos os buffs que estavam armazenados
        foreach (var kvp in appliedBuffDefinitions)
        {
            BuffType type = kvp.Key;
            BuffDefinition def = kvp.Value;

            // Se o buff não está na lista ativa, adiciona
            if (!activeBuffs.Contains(type))
            {
                activeBuffs.Add(type);
            }

            // Reaplica o efeito do buff apenas se não estiver ativo
            switch (type)
            {
                case BuffType.Shield:
                    // Regenera o escudo (já verifica internamente se precisa)
                    RegenerateShield();
                    break;
                case BuffType.Damage:
                    // Reaplica o buff de dano apenas se não estiver ativo
                    if (!isDamageBuffed && cardCombat != null)
                    {
                        int damageBonus = def != null ? def.damageBonus : 2;
                        // Se o dano original não foi guardado, calcula removendo o bônus atual
                        if (originalDamage == 0)
                        {
                            // Tenta calcular o dano original removendo o bônus
                            originalDamage = cardCombat.damage - damageBonus;
                            if (originalDamage < 1) originalDamage = cardCombat.damage; // Fallback
                        }
                        cardCombat.damage = originalDamage + damageBonus;
                        isDamageBuffed = true;
                    }
                    break;
                case BuffType.Speed:
                    // Reaplica o buff de velocidade apenas se não estiver ativo
                    if (!isSpeedBuffed && cardCombat != null)
                    {
                        float speedMultiplier = def != null ? def.speedMultiplier : 0.7f;
                        float minInterval = def != null ? def.minAttackInterval : 0.4f;
                        // Se o intervalo original não foi guardado, usa o atual como base
                        if (originalAttackInterval == 0)
                        {
                            originalAttackInterval = cardCombat.attackInterval;
                        }
                        float newInterval = originalAttackInterval * speedMultiplier;
                        cardCombat.attackInterval = Mathf.Max(minInterval, newInterval);
                        isSpeedBuffed = true;
                    }
                    break;
            }
        }

        Debug.Log($"{name} restaurou todos os buffs! ({appliedBuffDefinitions.Count} buff(s))");
    }

    void CreateShieldOverlay(Color overlayColor)
    {
        // Verifica se a carta tem RectTransform (UI)
        RectTransform cardRect = GetComponent<RectTransform>();
        if (cardRect == null)
        {
            Debug.LogWarning($"{name}: Carta não tem RectTransform. Não é possível criar overlay UI do escudo.");
            return;
        }

        // Cria GameObject para o overlay
        GameObject overlayObj = new GameObject("ShieldOverlay");
        overlayObj.transform.SetParent(transform, false);

        // Adiciona RectTransform
        RectTransform overlayRect = overlayObj.AddComponent<RectTransform>();
        
        // Faz o overlay cobrir toda a carta E ficar maior (com offset negativo)
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        float padding = -10f; // Offset negativo para ficar maior que a carta
        overlayRect.offsetMin = new Vector2(padding, padding);
        overlayRect.offsetMax = new Vector2(-padding, -padding);
        overlayRect.localScale = Vector3.one;

        // Adiciona Image para o overlay
        shieldOverlay = overlayObj.AddComponent<Image>();
        shieldOverlay.color = overlayColor;
        
        // Garante que o overlay fica acima de outros elementos
        overlayObj.transform.SetAsLastSibling();

        // Desabilita interação (não bloqueia cliques na carta)
        var canvasGroup = overlayObj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = overlayObj.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    void HandleShieldBlock(CardHealth sender, ref int damage)
    {
        // Verifica se tem escudos disponíveis (mesmo que hasShield seja false temporariamente)
        if (currentShieldCount <= 0)
        {
            // Sem escudos, não bloqueia
            return;
        }

        // Consome um escudo
        currentShieldCount--;
        damage = 0; // bloqueia totalmente

        if (currentShieldCount <= 0)
        {
            Debug.Log($"{name} bloqueou um ataque com o escudo! Escudo consumido (será regenerado na próxima rodada).");
            // Remove apenas o overlay visual, mas mantém o buff ativo para regenerar
            if (shieldOverlay != null)
            {
                Destroy(shieldOverlay.gameObject);
                shieldOverlay = null;
            }
            // NÃO remove o buff da lista, apenas marca como sem escudos
            hasShield = false; // Temporariamente sem escudo, mas buff ainda ativo na lista
        }
        else
        {
            Debug.Log($"{name} bloqueou um ataque com o escudo! Escudos restantes: {currentShieldCount}");
        }
    }

    void RemoveShield()
    {
        hasShield = false;
        currentShieldCount = 0;
        maxShieldCount = 0;

        // Remove o overlay do escudo
        if (shieldOverlay != null)
        {
            Destroy(shieldOverlay.gameObject);
            shieldOverlay = null;
        }

        if (cardHealth != null)
            cardHealth.OnBeforeTakeDamage -= HandleShieldBlock;
    }

    /// <summary>
    /// Remove um buff específico sem limpar a definição (útil para mover buffs entre cartas)
    /// </summary>
    public void RemoveBuffWithoutClearingDefinition(BuffType type)
    {
        if (!activeBuffs.Contains(type))
            return;

        activeBuffs.Remove(type);

        switch (type)
        {
            case BuffType.Shield:
                // Remove visual mas mantém a definição para poder reaplicar
                if (shieldOverlay != null)
                {
                    Destroy(shieldOverlay.gameObject);
                    shieldOverlay = null;
                }
                hasShield = false;
                currentShieldCount = 0;
                if (cardHealth != null)
                    cardHealth.OnBeforeTakeDamage -= HandleShieldBlock;
                break;
            case BuffType.Damage:
                RemoveDamageBuff();
                break;
            case BuffType.Speed:
                RemoveSpeedBuff();
                break;
        }
    }

    // ⚔️ DANO
    void ApplyDamageBuff(BuffDefinition definition = null)
    {
        if (isDamageBuffed || cardCombat == null) return;

        int damageBonus = definition != null ? definition.damageBonus : 2;
        
        // Guarda o dano original se ainda não foi guardado
        if (!isDamageBuffed)
            originalDamage = cardCombat.damage;

        cardCombat.damage += damageBonus;
        isDamageBuffed = true;
        Debug.Log($"{name} recebeu BUFF: +{damageBonus} de dano! (Dano total: {cardCombat.damage})");
    }

    void RemoveDamageBuff()
    {
        if (!isDamageBuffed || cardCombat == null) return;

        // Restaura o dano original
        if (appliedBuffDefinitions.TryGetValue(BuffType.Damage, out BuffDefinition def) && def != null)
        {
            cardCombat.damage -= def.damageBonus;
        }
        else
        {
            // Fallback: assume +2 se não tiver definição
            cardCombat.damage -= 2;
        }

        isDamageBuffed = false;
    }

    // ⚡ VELOCIDADE
    void ApplySpeedBuff(BuffDefinition definition = null)
    {
        if (isSpeedBuffed || cardCombat == null) return;

        float speedMultiplier = definition != null ? definition.speedMultiplier : 0.7f;
        float minInterval = definition != null ? definition.minAttackInterval : 0.4f;

        // Guarda o intervalo original se ainda não foi guardado
        if (!isSpeedBuffed)
            originalAttackInterval = cardCombat.attackInterval;

        float newInterval = cardCombat.attackInterval * speedMultiplier;
        cardCombat.attackInterval = Mathf.Max(minInterval, newInterval);
        isSpeedBuffed = true;

        float speedIncrease = (1f - speedMultiplier) * 100f;
        Debug.Log($"{name} recebeu BUFF: Velocidade de ataque aumentada em {speedIncrease:F0}%! (Intervalo: {cardCombat.attackInterval:F2}s)");
    }

    void RemoveSpeedBuff()
    {
        if (!isSpeedBuffed || cardCombat == null) return;

        // Restaura o intervalo original
        cardCombat.attackInterval = originalAttackInterval;
        isSpeedBuffed = false;
    }
}
