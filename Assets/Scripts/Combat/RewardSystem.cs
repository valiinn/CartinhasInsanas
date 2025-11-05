using UnityEngine;

public class RewardSystem : MonoBehaviour
{
    [Header("Recompensa por vitória")]
    [Tooltip("Gold dado ao jogador por vencer uma fase.")]
    public int goldReward = 100;

    private PlayerStats playerStats;

    [System.Obsolete]
    void Awake()
    {
        // Busca automática de PlayerStats na cena
        if (playerStats == null)
            playerStats = FindObjectOfType<PlayerStats>();
    }

    /// <summary>
    /// Dá recompensa de vitória ao jogador (chame quando o jogador vencer)
    /// </summary>
    public void GiveWinReward()
    {
        if (playerStats == null)
        {
            Debug.LogWarning("RewardSystem: PlayerStats não encontrado na cena!");
            return;
        }

        playerStats.AddGold(goldReward);
        Debug.Log($"💰 Jogador recebeu {goldReward} de gold! Total atual: {playerStats.gold}");
    }
}
