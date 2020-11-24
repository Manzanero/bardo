using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableWindow : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler
{
    [SerializeField] private RectTransform dragRectTransform;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image backgroundImage;
    private Color _backgroundColor;

    private void Awake()
    {
        if (dragRectTransform == null)
            dragRectTransform = transform.parent.GetComponent<RectTransform>();

        if (canvas == null)
        {
            var testCanvasTransform = transform.parent;
            while (testCanvasTransform != null)
            {
                canvas = testCanvasTransform.GetComponent<Canvas>();
                if (canvas != null)
                    break;

                testCanvasTransform = testCanvasTransform.parent;
            }
        }
        
        if (backgroundImage == null)
            backgroundImage = transform.GetComponent<Image>();
        
        _backgroundColor = backgroundImage.color;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _backgroundColor.a /= 2f;
        backgroundImage.color = _backgroundColor;
    }

    public void OnDrag(PointerEventData eventData)
    {
        dragRectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _backgroundColor.a *= 2f;
        backgroundImage.color = _backgroundColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        dragRectTransform.SetAsLastSibling();
    }
}
