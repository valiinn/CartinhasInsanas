using UnityEngine;
using TMPro; // necessário para usar TMP_Dropdown

public class ScreenModeSelector : MonoBehaviour
{
    public TMP_Dropdown dropdown; // arraste seu TMP_Dropdown aqui

    void Start()
    {
        // Garante que o evento é registrado
        dropdown.onValueChanged.AddListener(ChangeScreenMode);
    }

    void ChangeScreenMode(int index)
    {
        if (index == 0)
        {
            // Tela cheia
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.fullScreen = true;
        }
        else if (index == 1)
        {
            // Janela
            Screen.fullScreenMode = FullScreenMode.Windowed;
            Screen.fullScreen = false;
        }
    }
}
