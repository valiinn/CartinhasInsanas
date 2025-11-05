using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Componentes de UI")]
    public Slider healthBar;
    public TextMeshProUGUI deathText;
    public Image deathFade;

    [Header("Configurações de Vida")]
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;

        deathText.gameObject.SetActive(false);
        deathFade.gameObject.SetActive(false);
    }

    void Update()
    {
        // Teste: aperte K para perder vida
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(25);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        healthBar.value = currentHealth;

        Debug.Log("O jogador recebeu " + damage + " de dano. Vida atual: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        deathFade.gameObject.SetActive(true);
        deathText.gameObject.SetActive(true);

        Color fadeColor = deathFade.color;
        Color textColor = deathText.color;

        // Começa invisível
        fadeColor.a = 0;
        textColor.a = 0;

        deathFade.color = fadeColor;
        deathText.color = textColor;

        // Fade preto e texto aparecendo lentamente
        float duration = 6f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            fadeColor.a = Mathf.Lerp(0, 1, t / duration);
            textColor.a = Mathf.Lerp(0, 2, t / duration);

            deathFade.color = fadeColor;
            deathText.color = textColor;

            yield return null;
        }

        yield return new WaitForSeconds(2f);

        // Reinicia a cena
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
