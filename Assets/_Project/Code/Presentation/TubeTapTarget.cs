using BlockSort.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BlockSort.Presentation
{
    /// <summary>Full-rect tube hit target so taps on blocks, gaps, and padding all count.</summary>
    public sealed class TubeTapTarget : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public int SlotIndex;
        public BlockSortSession Session;
        public RectTransform Visual;

        public void OnPointerClick(PointerEventData eventData)
        {
            Session?.TapSlot(SlotIndex);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Visual != null)
            {
                Visual.localScale = Vector3.one * 0.97f;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Visual != null)
            {
                Visual.localScale = Vector3.one;
            }
        }
    }
}
