using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public int gold = 0;

    [Header("UI")]
    public TMP_Text goldText;

    void Start()
    {
        AtualizarUI();
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            AtualizarUI();
            Debug.Log("Gold gasto: " + amount + ". Gold restante: " + gold);
            return true; // gasto realizado
        }
        else
        {
            Debug.Log("Gold insuficiente! Tentou gastar: " + amount + ", mas tem: " + gold);
            return false; // não há gold suficiente
        }
    }

    public void AddGold(int amount)
    {
        gold += amount;
        AtualizarUI();
        Debug.Log("Gold adicionado: " + amount + ". Total: " + gold);
    }

    void AtualizarUI()
    {
        if (goldText != null)
            goldText.text = gold.ToString();
    }
}
