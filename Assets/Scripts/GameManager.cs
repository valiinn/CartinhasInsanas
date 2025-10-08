using TMPro; 
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentPhase = 1;
    public int totalPhases = 3;

    [Header("UI")]
    public TMP_Text phaseText; 

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

    void Start()
    {
        UpdatePhaseText();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdatePhaseText();
        Debug.Log("Fase atual: " + currentPhase);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Pressione Espaço para avançar para a próxima fase
        {
            GoToNextPhase();
        }
    }

    public void GoToNextPhase()
    {
        currentPhase++;
        string nextScene = "Fase" + currentPhase;

        if (Application.CanStreamedLevelBeLoaded(nextScene))
        {
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            Debug.Log("Não há mais fases disponíveis.");
        }
    }

    void UpdatePhaseText()
    {
        if (phaseText != null)
        {
            phaseText.text = "Fase " + currentPhase;
        }
    }
}
