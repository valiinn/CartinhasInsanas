using UnityEngine;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text vidaText;
    public TMP_Text danoText;

    // Configura a UI da carta
    public void Setup(Card card)
    {
        if (card == null) return;

        nameText.text = card.nome;
        vidaText.text = "❤ " + card.vida;
        danoText.text = "⚔ " + card.dano;
    }
}
