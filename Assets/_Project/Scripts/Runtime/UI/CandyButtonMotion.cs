using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PuzzleGame.Runtime.Services;

namespace PuzzleGame.Runtime.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class CandyButtonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        private Button button;
        private bool hovered, pressed, selected;
        private Outline focus;
        private void Awake()
        {
            button = GetComponent<Button>();
            focus = GetComponent<Outline>() ?? gameObject.AddComponent<Outline>();
            focus.effectColor = new Color32(255, 217, 123, 255);
            focus.effectDistance = new Vector2(2, -2); focus.enabled = false;
        }
        public void OnSelect(BaseEventData data) => selected = true;
        public void OnDeselect(BaseEventData data) => selected = false;
        public void OnPointerEnter(PointerEventData data) => hovered = true;
        public void OnPointerExit(PointerEventData data) { hovered = false; pressed = false; }
        public void OnPointerDown(PointerEventData data) { if (data.button == PointerEventData.InputButton.Left) pressed = true; }
        public void OnPointerUp(PointerEventData data) => pressed = false;
        private void Update()
        {
            bool interactive = button.IsInteractable();
            focus.enabled = interactive && selected;
            float target = !interactive || GamePreferences.Current.reducedEffects ? 1 : pressed ? .98f : hovered ? 1.015f : 1;
            if (GamePreferences.Current.reducedEffects) { transform.localScale = Vector3.one; return; }
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * target, 1 - Mathf.Exp(-22 * Time.unscaledDeltaTime));
        }
        private void OnDisable() { hovered = pressed = selected = false; if (focus) focus.enabled = false; transform.localScale = Vector3.one; }
    }
}
