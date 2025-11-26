using UnityEngine;
using UnityEngine.UI;

public class ArenaController : MonoBehaviour
{
    [Header("Arena UI")]
    public Image arenaImage;        // arraste o Image que exibe a arena
    public Sprite defaultArena;     // arena padrão
    public Sprite florestaArena;    // arena do boss 1
    public Sprite outraArena;       // caso queira mais arenas

    // Troca o sprite da arena
    public void SwitchArena(string arenaName)
    {
        if (arenaImage == null) return;

        switch (arenaName)
        {
            case "Floresta":
                arenaImage.sprite = florestaArena;
                break;
            case "Default":
                arenaImage.sprite = defaultArena;
                break;
            case "Outra":
                arenaImage.sprite = outraArena;
                break;
            default:
                Debug.LogWarning("[ArenaController] Arena desconhecida: " + arenaName);
                break;
        }

        Debug.Log("[ArenaController] Arena trocada para: " + arenaName);
    }
}
