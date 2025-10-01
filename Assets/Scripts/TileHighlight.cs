using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image image;
    private Color originalColor;
    private Color targetColor;
    [Header("Cor de destaque")]
    public Color highlightColor = Color.yellow;

    [Header("Velocidade da transição")]
    public float fadeSpeed = 5f;

    void Start()
    {
        image = GetComponent<Image>();
        originalColor = image.color;
        targetColor = originalColor;
    }

    void Update()
    {
        // Transição suave entre a cor atual e a cor alvo
        image.color = Color.Lerp(image.color, targetColor, Time.deltaTime * fadeSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetColor = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetColor = originalColor;
    }
}
