using System;
using System.Collections.Generic;
using PuzzleGame.Core.Board;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PuzzleGame.Runtime.Board
{
    [RequireComponent(typeof(BoardController), typeof(BoardView))]
    public sealed class BoardInput : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField, Range(0.1f, 0.8f)] private float minimumDragCells = 0.25f;
        private InputActionAsset instance;
        private InputAction point, press, cancel;
        private BoardController controller;
        private BoardView view;
        private bool tracking;
        private BoardState pressedBoard;
        private GridPosition pressedCell;
        private GridPosition? previousSelection;
        private Vector2 pressLocal;
        private readonly List<RaycastResult> uiHits = new List<RaycastResult>();

        public void Configure(InputActionAsset actions) => inputActions = actions;

        private void Awake()
        {
            controller = GetComponent<BoardController>();
            view = GetComponent<BoardView>();
        }

        private void OnEnable()
        {
            if (!inputActions) throw new InvalidOperationException("BoardInput needs PuzzleInput.inputactions.");
            instance = Instantiate(inputActions);
            point = instance.FindAction("Board/Point", true);
            press = instance.FindAction("Board/Press", true);
            cancel = instance.FindAction("Board/Cancel", true);
            instance.Enable();
        }

        private void Update()
        {
            if (!controller || !controller.isActiveAndEnabled || controller.Model == null) return;
            if (controller.IsPaused) { tracking = false; return; }
            if (cancel.WasPressedThisFrame()) { CancelGesture(); return; }
            if (controller.IsBusy) { tracking = false; return; }
            if (tracking && pressedBoard != controller.Model) { CancelGesture(); return; }
            Vector2 screen = point.ReadValue<Vector2>();
            if (press.WasPressedThisFrame())
            {
                tracking = false;
                if (IsOverUI(screen) || !view.TryScreenToCell(screen, out pressedCell)
                    || !controller.CanSelect(pressedCell)) return;
                view.TryScreenToLocal(screen, out pressLocal);
                pressedBoard = controller.Model;
                previousSelection = controller.SelectedPosition;
                controller.SetSelection(pressedCell);
                tracking = true;
            }
            if (!tracking) return;
            if (press.WasReleasedThisFrame())
            {
                tracking = false;
                if (IsOverUI(screen) || !view.TryScreenToCell(screen, out _)
                    || !view.TryScreenToLocal(screen, out Vector2 releaseLocal))
                { controller.SetSelection(null); return; }
                Vector2 delta = releaseLocal - pressLocal;
                if (delta.magnitude >= minimumDragCells)
                {
                    GridPosition target = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)
                        ? new GridPosition(pressedCell.X + (delta.x > 0 ? 1 : -1), pressedCell.Y)
                        : new GridPosition(pressedCell.X, pressedCell.Y + (delta.y > 0 ? 1 : -1));
                    if (!controller.TrySwap(pressedCell, target)) controller.SetSelection(null);
                }
                else
                {
                    controller.SetSelection(previousSelection);
                    controller.SelectCell(pressedCell);
                }
            }
            else if (press.IsPressed() && view.TryScreenToLocal(screen, out Vector2 current))
            {
                view.PreviewDrag(pressedCell, current - pressLocal);
            }
        }

        private bool IsOverUI(Vector2 screen)
        {
            EventSystem events = EventSystem.current;
            if (!events) return false;
            // Raycast this position now, rather than using the previous frame's hover state.
            uiHits.Clear();
            events.RaycastAll(new PointerEventData(events) { position = screen }, uiHits);
            foreach (var hit in uiHits) if (hit.module is GraphicRaycaster) return true;
            return false;
        }

        public void CancelGesture()
        {
            tracking = false;
            if (controller && !controller.IsBusy) controller.SetSelection(null);
        }

        private void OnApplicationFocus(bool focused) { if (!focused) CancelGesture(); }
        private void OnDisable()
        {
            CancelGesture();
            if (instance) { instance.Disable(); Destroy(instance); }
            instance = null;
        }
    }
}
