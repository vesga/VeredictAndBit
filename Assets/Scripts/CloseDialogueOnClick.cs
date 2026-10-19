using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CloseDialogueOnClick : MonoBehaviour, IPointerClickHandler
{
    void Update()
    {
        // También permite cerrar con la barra espaciadora
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            CloseBox();
        }
    }

    // Detecta el clic directo sobre la caja de diálogo negra
    public void OnPointerClick(PointerEventData eventData)
    {
        CloseBox();
    }

    private void CloseBox()
    {
        gameObject.SetActive(false); // Oculta la caja de diálogo de investigación
    }
}