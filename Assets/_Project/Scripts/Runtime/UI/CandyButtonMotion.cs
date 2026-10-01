using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class CandyButtonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        private Button button;
        private bool hovered, pressed;
        private void Awake() => button = GetComponent<Button>();
        public void OnPointerEnter(PointerEventData data) => hovered = true;
        public void OnPointerExit(PointerEventData data) { hovered = false; pressed = false; }
        public void OnPointerDown(PointerEventData data) { if (data.button == PointerEventData.InputButton.Left) pressed = true; }
        public void OnPointerUp(PointerEventData data) => pressed = false;
        private void Update()
        {
            float target = !button.interactable ? 1 : pressed ? .95f : hovered ? 1.035f : 1;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * target, 1 - Mathf.Exp(-22 * Time.unscaledDeltaTime));
        }
        private void OnDisable() { hovered = pressed = false; transform.localScale = Vector3.one; }
    }
}
