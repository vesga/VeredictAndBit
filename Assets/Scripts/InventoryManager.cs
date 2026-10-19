using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    [Header("Paneles y Contenedores UI")]
    [SerializeField] private GameObject inventoryPanel;     // El panel menu_evidance
    [SerializeField] private Transform contentGridParent;   // El contenedor con Grid Layout Group
    [SerializeField] private GameObject buttonTemplate;     // El botón plantilla (ItemButtonTemplate)

    [Header("Detalles de la Evidencia Seleccionada")]
    [SerializeField] private TextMeshProUGUI itemTitleText;   // ItemTitleText
    [SerializeField] private TextMeshProUGUI itemDescText;    // ItemDescText
    [SerializeField] private Image itemPreviewImage;          // ItemPreviewImage

    [System.Serializable]
    public struct InventoryItemData
    {
        public string itemName;
        [TextArea(3, 5)] public string itemDescription;
        public Sprite itemIcon;
    }

    [Header("Lista de Evidencias en el Caso")]
    [SerializeField] private List<InventoryItemData> evidenceList = new List<InventoryItemData>();

    private bool isInventoryOpen = false;
    private List<GameObject> spawnedButtons = new List<GameObject>();
    private int currentlySelectedIndex = -1;

    void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false); // Inicia oculto
        }

        if (buttonTemplate != null)
        {
            buttonTemplate.SetActive(false); // Ocultar la plantilla base
        }
    }

    void Update()
    {
        // Abrir/Cerrar con la tecla 'I'
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isInventoryOpen);
            
            if (isInventoryOpen)
            {
                RefreshInventoryUI();
                if (evidenceList.Count > 0)
                {
                    SelectEvidence(0); // Selecciona la primera por defecto al abrir
                }
            }
        }
    }

    private void RefreshInventoryUI()
    {
        // Limpiar botones anteriores
        foreach (GameObject btn in spawnedButtons)
        {
            Destroy(btn);
        }
        spawnedButtons.Clear();

        if (buttonTemplate == null || contentGridParent == null) return;

        for (int i = 0; i < evidenceList.Count; i++)
        {
            int index = i;
            GameObject newButton = Instantiate(buttonTemplate, contentGridParent);
            newButton.SetActive(true);

            // Asignar el icono de la evidencia directamente a la imagen de fondo del botón
            Image btnImage = newButton.GetComponent<Image>();
            if (btnImage != null && evidenceList[i].itemIcon != null)
            {
                btnImage.sprite = evidenceList[i].itemIcon;
                btnImage.preserveAspect = true; // Mantiene las proporciones del pixel art
            }

            // Ocultar el texto del botón si lo tiene, para que solo se vea la imagen limpia
            TextMeshProUGUI btnText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.gameObject.SetActive(false);
            }

            // Configurar el evento OnClick del botón generado
            Button btnComponent = newButton.GetComponent<Button>();
            if (btnComponent != null)
            {
                btnComponent.onClick.AddListener(() => {
                    SelectEvidence(index);
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
                });
            }

            spawnedButtons.Add(newButton);
        }
    }

    public void SelectEvidence(int index)
    {
        if (index >= 0 && index < evidenceList.Count)
        {
            currentlySelectedIndex = index;
            InventoryItemData item = evidenceList[index];

            if (itemTitleText != null) itemTitleText.text = item.itemName;
            if (itemDescText != null) itemDescText.text = item.itemDescription;
            if (itemPreviewImage != null && item.itemIcon != null)
            {
                itemPreviewImage.sprite = item.itemIcon;
                itemPreviewImage.preserveAspect = true;
                itemPreviewImage.gameObject.SetActive(true);
            }
        }
    }

    public void AddEvidence(string name, string desc, Sprite icon)
    {
        InventoryItemData newItem = new InventoryItemData
        {
            itemName = name,
            itemDescription = desc,
            itemIcon = icon
        };
        evidenceList.Add(newItem);
    }

    public void PresentCurrentEvidence()
    {
        if (currentlySelectedIndex >= 0 && currentlySelectedIndex < evidenceList.Count)
        {
            string selectedName = evidenceList[currentlySelectedIndex].itemName.ToLower();
            Debug.Log("Has intentado presentar la evidencia: " + selectedName);

            // Cerramos el panel de inventario
            ToggleInventory();

            // Consultamos al DialogueManager qué ítem exige específicamente la línea actual del juicio
            DialogueManager dialogue = FindFirstObjectByType<DialogueManager>();
            string itemRequested = "log"; // Valor por defecto si no se encuentra
            
            if (dialogue != null)
            {
                string req = dialogue.GetRequiredItemForCurrentLine();
                if (!string.IsNullOrEmpty(req))
                {
                    itemRequested = req.ToLower();
                }
            }

            // Verificamos si seleccionó el ítem correcto que pedía V.E.R.A. en esta pregunta
            bool isCorrectItem = selectedName.Contains(itemRequested);
            
            // Exigencia de investigación: Al menos 3 pruebas recolectadas en total
            bool hasInvestigatedEnough = evidenceList.Count >= 3; 

            if (isCorrectItem && hasInvestigatedEnough)
            {
                Debug.Log("¡OBJECIÓN CORRECTA! Prueba aceptada por V.E.R.A.");
                
                // Sonido de Objeción / Acierto
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayObjection();
                }

                if (dialogue != null)
                {
                    dialogue.OnEvidencePresentedSuccessfully();
                }
            }
            else if (isCorrectItem && !hasInvestigatedEnough)
            {
                Debug.Log("¡Falta de pistas! Tienes el ítem correcto, pero te falta explorar más el laboratorio.");
                
                // Sonido de Error
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayError();
                }

                if (HealthManager.Instance != null)
                {
                    HealthManager.Instance.TakeDamage(1);
                }
            }
            else
            {
                Debug.Log("¡EVIDENCIA INCORRECTA! V.E.R.A. rechaza la objeción.");
                
                // Sonido de Error
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayError();
                }

                if (HealthManager.Instance != null)
                {
                    HealthManager.Instance.TakeDamage(1);
                }
            }
        }
        else
        {
            Debug.Log("Ninguna evidencia seleccionada.");
        }
    }
}