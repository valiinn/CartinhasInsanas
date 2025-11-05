using UnityEngine;

public class RewardSystem : MonoBehaviour
{
    [Header("Recompensa por vitória")]
    [Tooltip("Gold dado ao jogador por vencer uma fase.")]
    public int goldReward = 100;

    // Referência ao PlayerStats (setar no inspector ou buscar em runtime)
    public PlayerStats playerStats;

    // Chame essa função quando o jogador vencer a batalha
    public void GiveWinReward()
    {
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats == null)
            {
                Debug.LogWarning("RewardSystem: PlayerStats não encontrado.");
                return;
            }
        }

        playerStats.AddGold(goldReward);
        Debug.Log($"RewardSystem: jogador recebeu {goldReward} gold.");
        // Aqui você pode disparar efeitos visuais, som, popup, etc.
    }
}
