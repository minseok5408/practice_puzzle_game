using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace PuzzleGame.Runtime.Board
{
    public sealed partial class BoardInput
    {
        private GridPosition keyboardCell;
        private Vector2 keyboardPointer;
        private LineRenderer keyboardCursor;
        private Material cursorMaterial;
        private BoardState keyboardBoard;
        public bool KeyboardActive { get; private set; }
        public GridPosition KeyboardCell => keyboardCell;

        private bool HandleKeyboard()
        {
            if(keyboardBoard!=controller.Model){HideKeyboardCursor();keyboardBoard=controller.Model;}
            var keyboard = Keyboard.current;
            if (keyboard == null || controller.Session?.Progress == null || controller.Session.Progress.IsFinished) { HideKeyboardCursor(); return false; }
            if (keyboard.f1Key.wasPressedThisFrame) { FindFirstObjectByType<PlayerDialogs>()?.Guide(); return true; }
            if (keyboard.hKey.wasPressedThisFrame) { HideKeyboardCursor(); GetComponent<BoardGuidance>()?.RequestHint(); return true; }
            var keys = new[] { keyboard.digit1Key, keyboard.digit2Key, keyboard.digit3Key, keyboard.digit4Key };
            for (int i = 0; i < keys.Length; i++) if (keys[i].wasPressedThisFrame)
            { FindFirstObjectByType<ItemToolbar>()?.ShowDetails((ItemType)i); return true; }
            int dx = (keyboard.rightArrowKey.wasPressedThisFrame ? 1 : 0) - (keyboard.leftArrowKey.wasPressedThisFrame ? 1 : 0);
            int dy = (keyboard.upArrowKey.wasPressedThisFrame ? 1 : 0) - (keyboard.downArrowKey.wasPressedThisFrame ? 1 : 0);
            bool submit = keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame;
            if (dx == 0 && dy == 0 && !(KeyboardActive && submit)) { if(KeyboardActive)ShowKeyboardCursor(); return false; }
            if (!KeyboardActive) keyboardCell = controller.SelectedPosition ?? new GridPosition(0, 0);
            KeyboardActive = true; keyboardPointer = point.ReadValue<Vector2>();
            keyboardCell = new GridPosition(Mathf.Clamp(keyboardCell.X + dx, 0, controller.Model.Width - 1), Mathf.Clamp(keyboardCell.Y + dy, 0, controller.Model.Height - 1));
            if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
            GetComponent<BoardGuidance>()?.NotifyInput();
            view.ShowHint(null, null);
            if (submit)
            {
                if (controller.SelectedItem.HasValue) controller.TryUseItem(controller.SelectedItem.Value, keyboardCell);
                else controller.SelectCell(keyboardCell);
            }
            ShowKeyboardCursor(); return true;
        }

        private void ShowKeyboardCursor()
        {
            if (!keyboardCursor)
            {
                keyboardCursor = new GameObject("KeyboardCursor").AddComponent<LineRenderer>();
                keyboardCursor.transform.SetParent(transform, false); cursorMaterial = new Material(Shader.Find("Sprites/Default"));
                keyboardCursor.sharedMaterial = cursorMaterial; keyboardCursor.useWorldSpace = false; keyboardCursor.loop = true;
                keyboardCursor.positionCount = 4; keyboardCursor.widthMultiplier = .045f; keyboardCursor.sortingOrder = 18;
                keyboardCursor.startColor = keyboardCursor.endColor = new Color32(255, 244, 153, 255);
            }
            var center = view.CellToLocal(keyboardCell);
            keyboardCursor.SetPositions(new[] { center + new Vector3(-.46f, -.46f), center + new Vector3(.46f, -.46f), center + new Vector3(.46f, .46f), center + new Vector3(-.46f, .46f) });
            keyboardCursor.gameObject.SetActive(!controller.IsBusy);
        }
        private void HideKeyboardCursor() { KeyboardActive = false; if (keyboardCursor) keyboardCursor.gameObject.SetActive(false); }
    }
}
