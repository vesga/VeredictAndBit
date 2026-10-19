using UnityEngine;

public class ForceResolution : MonoBehaviour
{
    void Awake()
    {
        // Fuerza una resolución limpia de 1280x720 en modo ventana sin bordes o pantalla completa
        Screen.SetResolution(1280, 720, FullScreenMode.FullScreenWindow);
    }
}