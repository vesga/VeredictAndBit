using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Fuentes de Audio (Audio Sources)")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Música de Fondo (BGM)")]
    [SerializeField] private AudioClip trialBGM;
    [SerializeField] private AudioClip investigationBGM;

    [Header("Efectos de Sonido (SFX)")]
    [SerializeField] private AudioClip textBeepSFX;     // Sonido retro al escribir texto
    [SerializeField] private AudioClip objectionSFX;    // ¡Objeción! / Presentar prueba correcta
    [SerializeField] private AudioClip errorSFX;        // Error / Pérdida de vida
    [SerializeField] private AudioClip buttonClickSFX;   // Clic en botones o inventario

    private void Awake()
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

    // --- MÚSICA ---
    public void PlayMusicForPhase(GameManager.GamePhase phase)
    {
        AudioClip selectedClip = (phase == GameManager.GamePhase.Trial) ? trialBGM : investigationBGM;

        if (musicSource != null && selectedClip != null && musicSource.clip != selectedClip)
        {
            musicSource.clip = selectedClip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    // --- EFECTOS DE SONIDO ---
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
    void Start()
    {
        // Si estamos en la escena del menú, reproducimos la música de investigación por defecto
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenuScene")
        {
            PlayMusicForPhase(GameManager.GamePhase.Investigation);
        }
    }

    public void PlayTextBeep() => PlaySFX(textBeepSFX);
    public void PlayObjection() => PlaySFX(objectionSFX);
    public void PlayError() => PlaySFX(errorSFX);
    public void PlayButtonClick() => PlaySFX(buttonClickSFX);
}