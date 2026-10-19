using UnityEngine;
using TMPro;
using System.Collections;

public class CmdCursor : MonoBehaviour
{
    private TextMeshProUGUI textComponent;
    private string baseText;
    [SerializeField] private float blinkSpeed = 0.5f;

    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        baseText = textComponent.text;
    }

    void OnEnable()
    {
        StartCoroutine(BlinkCursor());
    }

    IEnumerator BlinkCursor()
    {
        while (true)
        {
            textComponent.text = baseText + " _";
            yield return new WaitForSeconds(blinkSpeed);
            textComponent.text = baseText;
            yield return new WaitForSeconds(blinkSpeed);
        }
    }
}