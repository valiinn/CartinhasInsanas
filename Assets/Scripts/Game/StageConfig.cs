using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageConfig", menuName = "Cartinhas/Stage Config", order = 0)]
public class StageConfig : ScriptableObject
{
    [System.Serializable]
    public class EnemyPlacement
    {
        [Tooltip("Prefab da carta/inimigo (deve ter seus componentes de combate/vida etc.)")]
        public GameObject enemyPrefab;

        [Tooltip("Índice do slot no board do inimigo (0..N-1)")]
        public int slotIndex = 0;
    }

    [Header("Inimigos dessa fase")]
    public List<EnemyPlacement> enemies = new List<EnemyPlacement>();
}
