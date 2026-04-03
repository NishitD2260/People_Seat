using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderDragHandler : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private Slider slider;

    public void OnBeginDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    }
}
