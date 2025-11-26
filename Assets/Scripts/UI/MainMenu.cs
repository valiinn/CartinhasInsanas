using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Navegação")]
    [SerializeField] private Button firstSelectedButton; // Botão que será selecionado ao iniciar

    private void Start()
    {
        // Aguarda um frame para garantir que todos os componentes estão inicializados
        StartCoroutine(SelectFirstButtonDelayed());
    }

    private System.Collections.IEnumerator SelectFirstButtonDelayed()
    {
        yield return null; // Aguarda um frame
        
        // Verifica se o EventSystem existe
        if (EventSystem.current == null)
        {
            Debug.LogWarning("MainMenu: EventSystem não encontrado! A navegação pode não funcionar.");
            yield break;
        }

        // Seleciona o primeiro botão para habilitar navegação por teclado/controle
        if (firstSelectedButton != null && firstSelectedButton.gameObject.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
        }
        else
        {
            // Se não foi atribuído, tenta encontrar o primeiro botão disponível
            Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (buttons.Length > 0)
            {
                // Procura pelo botão "Jogar" primeiro
                foreach (Button btn in buttons)
                {
                    if (btn != null && btn.gameObject.activeInHierarchy && btn.interactable)
                    {
                        if (btn.onClick.GetPersistentEventCount() > 0)
                        {
                            string methodName = btn.onClick.GetPersistentMethodName(0);
                            if (methodName == "PlayGame")
                            {
                                EventSystem.current.SetSelectedGameObject(btn.gameObject);
                                yield break;
                            }
                        }
                    }
                }
                // Se não encontrou, seleciona o primeiro botão interagível
                foreach (Button btn in buttons)
                {
                    if (btn != null && btn.gameObject.activeInHierarchy && btn.interactable)
                    {
                        EventSystem.current.SetSelectedGameObject(btn.gameObject);
                        yield break;
                    }
                }
            }
        }
    }

    private void OnEnable()
    {
        // Quando o menu é habilitado, seleciona o primeiro botão novamente
        if (EventSystem.current != null)
        {
            StartCoroutine(SelectFirstButtonDelayed());
        }
    }

    private void Update()
    {
        // Se nenhum botão está selecionado e o jogador pressiona uma tecla de navegação,
        // seleciona o primeiro botão novamente
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
        {
            // Verifica se o jogador está tentando navegar
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || 
                Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
                Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) ||
                Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
            {
                StartCoroutine(SelectFirstButtonDelayed());
            }
        }
    }

    // Botão "Jogar"
    // Quando clicado, carrega a cena 1.

    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1); // 
    }


    // Botão "Opções"
    // Abre um painel que no futuro terá ajuste de áudio, gráficos, etc.

    public void OpenSettings()
    {
        Debug.Log("Abrir painel de opções...");
    }


    // Botão "Sobre"
    // Abre um painel com informações sobre o projeto.

    public void OpenAbout()
    {
        Debug.Log("Mostrar informações do projeto");
    }


    // Botão "Sair"
    // Fecha o jogo quando compilado.

    public void QuitGame()
    {
        Debug.Log("Sair do jogo...");
        Application.Quit();
    }
}
