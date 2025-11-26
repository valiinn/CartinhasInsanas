using UnityEngine;

[System.Serializable]
public class BuffDefinition
{
    [Header("Identificação")]
    public BuffType buffType;
    public string buffName;
    public Sprite icon;
    public int cost = 10;
    public GameObject buffPrefab;

    [Header("Valores do Buff")]
    [Tooltip("Dano adicional para buffs de dano (ex: +2, +5)")]
    public int damageBonus = 2;

    [Tooltip("Multiplicador de velocidade de ataque (ex: 0.7 = 30% mais rápido, 0.5 = 50% mais rápido)")]
    [Range(0.1f, 1f)]
    public float speedMultiplier = 0.7f;

    [Tooltip("Quantidade de ataques que o escudo bloqueia (ex: 1 = bloqueia 1 ataque, 3 = bloqueia 3 ataques)")]
    public int shieldCount = 1;

    [Tooltip("Cor do overlay azul transparente que cobre a carta quando tem escudo")]
    public Color shieldColor = new Color(0.4f, 0.7f, 1f, 0.4f);

    [Tooltip("Tempo mínimo entre ataques após aplicar buff de velocidade (em segundos)")]
    public float minAttackInterval = 0.4f;
}

