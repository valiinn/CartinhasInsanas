using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentPhase = 1; // Fase atual
    public int totalPhases = 3;  // Total de fases

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Teste: se apertar espaço, vai para próxima fase
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GoToNextPhase();
        }
    }
     
    public void GoToNextPhase()
    {
        currentPhase++;

        string nextScene = "Fase" + currentPhase;

        // Verifica se a cena existe
        if (Application.CanStreamedLevelBeLoaded(nextScene))
        {
            SceneManager.LoadScene(nextScene);
            Debug.Log("Carregando " + nextScene);
        }
        else
        {
            Debug.Log("Não há mais fases disponíveis.");
        }
    }
}