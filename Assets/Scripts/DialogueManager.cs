using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections; // Necesario para las Corrutinas de tipeo

public class DialogueManager : MonoBehaviour
{
    [Header("Elementos de UI (Canvas)")]
    [SerializeField] private TextMeshProUGUI dialogueText;    
    [SerializeField] private TextMeshProUGUI nameTagText;     

    [Header("Referencias de Personajes en Pantalla")]
    [SerializeField] private Image playerSpriteImage;  
    [SerializeField] private Image rivalSpriteImage;   

    [Header("Control de Paneles")]
    [SerializeField] private GameObject inventoryPanel; 

    [Header("Evidencia Inicial Otorgada por V.E.R.A.")]
    [SerializeField] private string initialItemName = "Credencial Falsa";
    [TextArea(2, 4)]
    [SerializeField] private string initialItemDesc = "Credencial administrativa clonada utilizada en el acceso ilegal de las 23:14 PM.";
    [SerializeField] private Sprite initialItemIcon;

    [Header("Configuración de Texto")]
    [SerializeField] private float typingSpeed = 0.03f; // Velocidad a la que aparecen las letras
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private string currentMessageFull = "";

    public enum CharacterTarget { Player, Rival, None }

    [System.Serializable]
    public struct DialogueLine
    {
        public string speakerName;                     
        [TextArea(3, 10)] public string message;       
        public CharacterTarget targetCharacter;        
        public Sprite newExpressionSprite;             
        public bool highlightSpeaker;   
        public bool requiresEvidence; // Pausa el diálogo exigiendo la prueba
        public string requiredItemName; // El nombre exacto del ítem que exige esta línea específica
    }

    [Header("Secuencia de Diálogos Iniciales / Apertura")]
    [SerializeField] private DialogueLine[] currentDialogueLines;

    [Header("Secuencia de Diálogos de Objeción / Defensa")]
    [SerializeField] private DialogueLine[] objectionDialogueLines;

    private DialogueLine[] activeLines;
    private int currentIndex = 0;
    private bool dialogueFinished = false; 
    private bool waitingForEvidence = false; 

    void Start()
    {
        activeLines = currentDialogueLines;
        if (activeLines != null && activeLines.Length > 0)
        {
            ShowDialogueLine(currentIndex);
        }
    }

    void Update()
    {
        if (dialogueFinished) return;

        if (inventoryPanel != null && inventoryPanel.activeSelf) return;
        if (waitingForEvidence) return; // Congela el avance si V.E.R.A. pide la prueba

        if (GameManager.Instance != null && GameManager.Instance.GetCurrentPhase() != GameManager.GamePhase.Trial)
        {
            return;
        }

        bool advancePressed = false;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            advancePressed = true;
        }
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            advancePressed = true;
        }

        if (advancePressed)
        {
            // Si el texto sigue apareciendo, lo completamos de golpe. Si ya terminó, avanzamos de línea.
            if (isTyping)
            {
                CompleteTextInstantly();
            }
            else
            {
                AdvanceDialogue();
            }
        }
    }

    public void AdvanceDialogue()
    {
        if (dialogueFinished || waitingForEvidence || isTyping) return;

        currentIndex++;
        if (currentIndex < activeLines.Length)
        {
            ShowDialogueLine(currentIndex);
        }
        else
        {
            Debug.Log("Fin de la secuencia de diálogos actual.");
            dialogueFinished = true;
            OnDialogueFinished();
        }
    }

    // Método llamado desde el InventoryManager al presentar la prueba correcta
    public void OnEvidencePresentedSuccessfully()
    {
        Debug.Log("¡Prueba validada con éxito! Avanzando en el debate...");
        waitingForEvidence = false;
        dialogueFinished = false; 
        
        // Continúa automáticamente a la siguiente línea del debate
        AdvanceDialogue();
    }

    // Método para iniciar el juicio/debate al salir de la investigación
    public void StartTrialDialogueSequence()
    {
        if (objectionDialogueLines == null || objectionDialogueLines.Length == 0)
        {
            Debug.LogWarning("No hay líneas configuradas en 'Objection Dialogue Lines' en el Inspector.");
            return;
        }

        activeLines = objectionDialogueLines;
        currentIndex = 0;
        dialogueFinished = false;
        waitingForEvidence = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPhase(GameManager.GamePhase.Trial);
        }

        ShowDialogueLine(currentIndex);
        Debug.Log("V.E.R.A. ha lanzado el diálogo de inicio de debate.");
    }

    private void OnDialogueFinished()
    {
        if (activeLines == currentDialogueLines)
        {
            InventoryManager inventory = FindFirstObjectByType<InventoryManager>();
            if (inventory != null && initialItemIcon != null)
            {
                inventory.AddEvidence(initialItemName, initialItemDesc, initialItemIcon);
                Debug.Log("V.E.R.A. te ha entregado: " + initialItemName);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetPhase(GameManager.GamePhase.Investigation);
            }
        }
        else if (activeLines == objectionDialogueLines)
        {
            Debug.Log("¡Secuencia de objeción finalizada con éxito! Veredicto alcanzado.");
            
            if (HealthManager.Instance != null)
            {
                HealthManager.Instance.TriggerVictory();
            }
        }
    }

    private void ShowDialogueLine(int index)
    {
        if (activeLines == null || index >= activeLines.Length) return;

        DialogueLine line = activeLines[index];
        
        if (nameTagText != null) nameTagText.text = line.speakerName;

        // Guardamos el mensaje completo y disparamos el efecto de máquina de escribir con sonido
        currentMessageFull = line.message;
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(currentMessageFull));

        if (line.newExpressionSprite != null)
        {
            if (line.targetCharacter == CharacterTarget.Player && playerSpriteImage != null)
            {
                playerSpriteImage.sprite = line.newExpressionSprite;
            }
            else if (line.targetCharacter == CharacterTarget.Rival && rivalSpriteImage != null)
            {
                rivalSpriteImage.sprite = line.newExpressionSprite;
            }
        }

        if (line.highlightSpeaker)
        {
            ApplyFocusEffect(line.targetCharacter);
        }

        // Si la línea requiere la prueba, activamos el bloqueo y la pausa
        if (line.requiresEvidence)
        {
            waitingForEvidence = true;
            Debug.Log("Debate pausado: V.E.R.A. exige la presentación de una prueba ('" + line.requiredItemName + "'). Abre el inventario con 'I'.");
        }
    }

    // Corrutina que escribe letra por letra reproduciendo el sonido del AudioManager
    private IEnumerator TypeText(string message)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in message.ToCharArray())
        {
            dialogueText.text += letter;

            // Reproduce el sonido de texto configurado en el AudioManager (si no es espacio en blanco)
            if (char.IsLetterOrDigit(letter) && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayTextBeep();
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    // Si el jugador presiona antes de que termine de escribirse, el texto se muestra completo al instante
    private void CompleteTextInstantly()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        dialogueText.text = currentMessageFull;
        isTyping = false;
    }

    private void ApplyFocusEffect(CharacterTarget target)
    {
        Color activeColor = Color.white;
        Color dimColor = new Color(0.6f, 0.6f, 0.6f, 1f);

        if (playerSpriteImage != null)
            playerSpriteImage.color = (target == CharacterTarget.Player) ? activeColor : dimColor;

        if (rivalSpriteImage != null)
            rivalSpriteImage.color = (target == CharacterTarget.Rival) ? activeColor : dimColor;
    }

    public string GetRequiredItemForCurrentLine()
    {
        if (activeLines != null && currentIndex >= 0 && currentIndex < activeLines.Length)
        {
            return activeLines[currentIndex].requiredItemName;
        }
        return "";
    }
}