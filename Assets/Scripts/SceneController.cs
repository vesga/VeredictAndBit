using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // Reinicia la escena actual (para el botón de Reintentar en Game Over)
    public void RestartGame()
    {
        Time.timeScale = 1f; // Restaura el tiempo por si estaba pausado
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Carga la escena del Menú Principal
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene"); // Asegúrate de que tu escena de menú se llame así
    }
}