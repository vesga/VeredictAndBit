using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class EvidenceObject : MonoBehaviour, IPointerClickHandler
{
    [Header("Datos de la Evidencia")]
    public string itemName = "Bitácora de Notas";
    [TextArea(3, 5)] public string itemDescription = "Notas escritas a mano encontradas en la escena con apuntes sobre la red y la terminal.";
    public Sprite itemIcon;

    [Header("Comentario del Personaje")]
    [TextArea(2, 4)] public string inspectionComment = "¡Atención! He encontrado una nota relevante sobre el sistema. La guardaré como evidencia.";

    [Header("UI de Diálogo de Investigación")]
    [SerializeField] private GameObject dialoguePanel;      // Arrastra dialogos_inves
    [SerializeField] private TextMeshProUGUI dialogueText;  // Arrastra el objeto Dialogo (TMP)

    [Header("Opciones")]
    [SerializeField] private bool destroyOnCollect = true;

    public void OnPointerClick(PointerEventData eventData)
    {
        CollectEvidence();
    }

    private void CollectEvidence()
    {
        // 1. Guardar en el Inventario
        InventoryManager inventory = FindFirstObjectByType<InventoryManager>();
        if (inventory != null)
        {
            inventory.AddEvidence(itemName, itemDescription, itemIcon);
        }

        // 2. Activar la caja de diálogo e inyectar el comentario
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (dialogueText != null)
        {
            dialogueText.text = inspectionComment;
        }

        // 3. Ocultar o destruir la nota de la escena
        if (destroyOnCollect)
        {
            gameObject.SetActive(false);
        }
    }
}