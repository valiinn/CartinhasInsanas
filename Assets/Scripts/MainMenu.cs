using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
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
