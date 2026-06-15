using UnityEngine;
using UnityEngine.EventSystems;

public class MobileCameraTouch : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    [SerializeField] private PlayerInputHandler inputHandler;

    private Vector2 previousPosition;

    public void OnPointerDown(PointerEventData eventData)
    {
        previousPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.position - previousPosition;

        inputHandler.SetRotationInput(delta);

        previousPosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputHandler.SetRotationInput(Vector2.zero);
    }
}