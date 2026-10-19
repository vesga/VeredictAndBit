using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance { get; private set; }

    [Header("Configuración de Vida")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("UI de Corazones")]
    [SerializeField] private List<Image> heartImages; // Arrastra los objetos de imagen de los corazones
    [SerializeField] private Sprite fullHeartSprite;  // Sprite del corazón lleno
    [SerializeField] private Sprite emptyHeartSprite; // Sprite del corazón vacío

    [Header("Paneles de Estado Final")]
    [SerializeField] private GameObject gameOverPanel;   // Panel que se activa si la vida llega a 0
    [SerializeField] private GameObject victoryPanel;    // Panel que se activa al ganar el juicio

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHeartsUI();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("¡Vida reducida! Salud actual: " + currentHealth);
        UpdateHeartsUI();

        if (currentHealth <= 0)
        {
            TriggerGameOver();
        }
    }

    private void UpdateHeartsUI()
    {
        if (heartImages == null) return;

        for (int i = 0; i < heartImages.Count; i++)
        {
            if (heartImages[i] == null) continue;

            if (i < currentHealth)
            {
                if (fullHeartSprite != null) heartImages[i].sprite = fullHeartSprite;
                heartImages[i].gameObject.SetActive(true);
            }
            else
            {
                if (emptyHeartSprite != null)
                {
                    heartImages[i].sprite = emptyHeartSprite;
                }
                else
                {
                    heartImages[i].gameObject.SetActive(false);
                }
            }
        }
    }

    private void TriggerGameOver()
    {
        Debug.Log("¡GAME OVER! V.E.R.A. ha bloqueado tu acceso permanentemente.");
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f; // Pausa el juego
    }

    public void TriggerVictory()
    {
        Debug.Log("¡VICTORIA! Veredicto de inocencia alcanzado.");
        
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}