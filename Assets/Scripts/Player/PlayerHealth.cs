using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Corações")]
    public Image[] hearts;            // arraste Heart1..Heart3
    public Sprite fullHeart;
    public Sprite emptyHeart;
    public int maxHearts = 3;

    [Header("Tela de Morte (setup)")]
    public Image deathFade;                  // imagem preta fullscreen (deixe Raycast Target = false)
    public TextMeshProUGUI deathText;        // texto "VOCÊ MORREU"
    public GameObject deathScreenPanel;      // painel que contém os botões (desativado no Start)
    public CanvasGroup deathButtonsCanvasGroup; // (adicionar CanvasGroup no painel de botões filhos)

    private int currentHearts;
    private bool isDead = false;

    void Start()
    {
        currentHearts = maxHearts;
        UpdateHeartsUI();

        if (deathFade != null)
        {
            deathFade.gameObject.SetActive(true); 
            var c = deathFade.color;
            c.a = 0f;
            deathFade.color = c;
            deathFade.raycastTarget = false; // desativa bloqueio de clique
        }

        if (deathText != null)
        {
            deathText.gameObject.SetActive(true); 
            var t = deathText.color;
            t.a = 0f;
            deathText.color = t;
        }

        if (deathScreenPanel != null)
            deathScreenPanel.SetActive(false);

        if (deathButtonsCanvasGroup != null)
        {
            deathButtonsCanvasGroup.alpha = 0f;
            deathButtonsCanvasGroup.interactable = false;
            deathButtonsCanvasGroup.blocksRaycasts = false;
        }
    }

    void Update()
    {
        if (!isDead && Input.GetKeyDown(KeyCode.K))
        {
            LoseHeart();
        }
    }

    public void LoseHeart()
    {
        if (isDead) return;

        currentHearts = Mathf.Max(0, currentHearts - 1);
        UpdateHeartsUI();
        Debug.Log("[PlayerHealth] Perdeu 1 coração. Restam: " + currentHearts);

        if (currentHearts <= 0)
        {
            StartCoroutine(DeathFlow());
        }
    }

    void UpdateHeartsUI()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;
            hearts[i].sprite = (i < currentHearts) ? fullHeart : emptyHeart;
        }
    }

    IEnumerator DeathFlow()
    {
        isDead = true;
        Debug.Log("[PlayerHealth] Iniciando sequencia de morte...");

        float fadeDuration = 1f;
        float t = 0f;

        if (deathFade != null)
        {
            Color c = deathFade.color;
            c.a = 0f;
            deathFade.color = c;
            deathFade.gameObject.SetActive(true);

            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                c.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
                deathFade.color = c;
                yield return null;
            }
        }

        yield return new WaitForSecondsRealtime(0.2f);

        if (deathText != null)
        {
            t = 0f;
            Color textColor = deathText.color;
            while (t < 0.5f)
            {
                t += Time.unscaledDeltaTime;
                textColor.a = Mathf.Lerp(0f, 1f, t / 0.5f);
                deathText.color = textColor;
                yield return null;
            }
        }

        yield return new WaitForSecondsRealtime(0.3f);

        if (deathScreenPanel != null)
            deathScreenPanel.SetActive(true);

        if (deathButtonsCanvasGroup != null)
        {
            // fade-in rápido dos botões e habilita interações
            t = 0f;
            while (t < 0.3f)
            {
                t += Time.unscaledDeltaTime;
                deathButtonsCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.3f);
                yield return null;
            }
            deathButtonsCanvasGroup.interactable = true;
            deathButtonsCanvasGroup.blocksRaycasts = true;
        }

        Debug.Log("[PlayerHealth] Sequencia de morte concluída.");
    }

    public void RestartGame()
    {
        Debug.Log("[PlayerHealth] Reiniciando cena...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        Debug.Log("[PlayerHealth] Indo para menu...");
        SceneManager.LoadScene("Menu Principal"); 
    }
}
