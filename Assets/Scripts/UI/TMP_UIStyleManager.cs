using System.Collections;
using UnityEngine;

public class TMP_UIStyleManager : MonoBehaviour
{
    [Header("Victory Panels")]
    public GameObject victoryPanel;      // painel curto do Boss 1
    public GameObject finalVictoryPanel; // painel final do Boss 2

    [Header("Settings")]
    public float victoryDisplayTime = 2f;

    public void ShowVictoryScreen()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            Debug.Log("[UIManager] Mostrando tela de vitória do Boss 1.");
            StartCoroutine(HidePanelAfterSeconds(victoryPanel, victoryDisplayTime));
        }
    }

    public void ShowFinalVictoryScreen()
    {
        if (finalVictoryPanel != null)
        {
            finalVictoryPanel.SetActive(true);
            Debug.Log("[UIManager] Mostrando tela de vitória final do Boss 2.");
            StartCoroutine(HidePanelAfterSeconds(finalVictoryPanel, victoryDisplayTime));
        }
    }

    private IEnumerator HidePanelAfterSeconds(GameObject panel, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (panel != null)
            panel.SetActive(false);
    }
}
