using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [Header("Configuração")]
    public GameObject cartaPrefab;        // Prefab do Card
    public Transform cartasLivresParent;  // Onde as cartas vão aparecer na tela

    void Start()
    {
        // Criar algumas cartas de teste
        CriarCarta(10, 5);
        CriarCarta(7, 8);
        CriarCarta(15, 3);
    }

    void CriarCarta(int vida, int dano)
    {
        // Instancia o prefab como filho do container
        GameObject novaCarta = Instantiate(cartaPrefab, cartasLivresParent, false);

        // Pega o componente Card
        Card cardComp = novaCarta.GetComponent<Card>();
        if (cardComp == null)
        {
            Debug.LogError("Prefab não tem o componente Card!");
            return;
        }

        // Configura atributos
        cardComp.vida = vida;
        cardComp.dano = dano;

        // Atualiza texto se quiser mostrar (opcional)
        // Exemplo: se tiver Text ou TMP_Text nos filhos da carta
        TMPro.TMP_Text[] textos = novaCarta.GetComponentsInChildren<TMPro.TMP_Text>();
        foreach (var t in textos)
        {
            if (t.name.ToLower().Contains("vida"))
                t.text = "❤ " + vida;
            else if (t.name.ToLower().Contains("dano"))
                t.text = "⚔ " + dano;
        }
    }
}
