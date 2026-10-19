using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GamePhase { Investigation, Trial }
    [Header("Fase Actual del Juego")]
    [SerializeField] private GamePhase currentPhase = GamePhase.Investigation;

    [Header("Paneles Principales")]
    [SerializeField] private GameObject investigationPanel;
    [SerializeField] private GameObject trialPanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        SetPhase(currentPhase);
    }

    public void SetPhase(GamePhase newPhase)
    {
        currentPhase = newPhase;

        if (investigationPanel != null)
            investigationPanel.SetActive(currentPhase == GamePhase.Investigation);

        if (trialPanel != null)
            trialPanel.SetActive(currentPhase == GamePhase.Trial);

        Debug.Log("Cambiando a fase de: " + currentPhase);

        // Reproduce o cambia la música correspondiente a la fase actual
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusicForPhase(currentPhase);
        }

        // Si cambiamos a la fase de Tribunal (Trial), le indicamos al DialogueManager que inicie el diálogo del juicio
        if (currentPhase == GamePhase.Trial)
        {
            DialogueManager dialogue = FindFirstObjectByType<DialogueManager>();
            if (dialogue != null)
            {
                dialogue.StartTrialDialogueSequence();
            }
        }
    }

    // Método para cambiar de fase desde un botón o al terminar de investigar
    public void TogglePhase()
    {
        if (currentPhase == GamePhase.Investigation)
        {
            SetPhase(GamePhase.Trial);
        }
        else
        {
            SetPhase(GamePhase.Investigation);
        }
    }

    public GamePhase GetCurrentPhase()
    {
        return currentPhase;
    }
}