using UnityEngine;
using UnityEngine.UI;

public class MerchantUI : MonoBehaviour
{
    [Header("Referências de UI")]
    [SerializeField] private GameObject merchantPanel; // painel principal da loja
    [SerializeField] private Transform itemGrid;       // grid onde as cartas aparecerão futuramente
    [SerializeField] private Button closeButton;       // botão para fechar a loja

    private void Start()
    {
        // Garante que o painel comece desativado (fora da fase de merchant)
        if (merchantPanel != null)
            merchantPanel.SetActive(false);

        // Liga o botão de fechar, se existir
        if (closeButton != null)
            closeButton.onClick.AddListener(() =>
            {
                if (merchantPanel != null)
                    merchantPanel.SetActive(false);
            });
    }

    /// <summary>
    /// Mostra a loja (chamado pelo PhaseManager quando entra na fase Merchant)
    /// </summary>
    public void ShowMerchant()
    {
        if (merchantPanel != null)
            merchantPanel.SetActive(true);
    }

    /// <summary>
    /// Esconde a loja (chamado quando sai da fase Merchant)
    /// </summary>
    public void HideMerchant()
    {
        if (merchantPanel != null)
            merchantPanel.SetActive(false);
    }
}

