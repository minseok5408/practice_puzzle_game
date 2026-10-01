using PuzzleGame.Runtime.Board;
using UnityEngine;

namespace PuzzleGame.Runtime.UI
{
    // Uses the same viewport for the board camera and the surrounding UI.
    public sealed class CandyLayout : MonoBehaviour
    {
        [SerializeField] private BoardView board;
        [SerializeField] private SpriteRenderer backdrop;
        [SerializeField] private SpriteRenderer boardFrame;
        [SerializeField] private RectTransform header, sidebar, moves, score, goal, progress, restart, stage, itemSlots;
        [SerializeField] private RectTransform settings;
        [SerializeField] private RectTransform worldTitle, collectionGoals;
        public void ConfigureCampaign(RectTransform world, RectTransform goals) { worldTitle = world; collectionGoals = goals; }
        public void ConfigureSettings(RectTransform button) => settings = button;
        private float previousAspect = -1f;

        public void Configure(BoardView view, SpriteRenderer background, RectTransform heading,
            RectTransform panel, RectTransform moveGroup, RectTransform scoreGroup, RectTransform goalGroup,
            RectTransform progressGroup, RectTransform restartGroup, RectTransform stageLabel, SpriteRenderer frame,
            RectTransform slotGroup)
        {
            board = view; backdrop = background; header = heading; sidebar = panel;
            moves = moveGroup; score = scoreGroup; goal = goalGroup; progress = progressGroup;
            restart = restartGroup; stage = stageLabel; boardFrame = frame; itemSlots = slotGroup;
        }

        private void Start() => ApplyLayout();
        private void LateUpdate()
        {
            if (!board || !board.BoardCamera) return;
            if (!Mathf.Approximately(previousAspect, board.BoardCamera.aspect)) ApplyLayout();
            FitBackdrop();
        }

        public void ApplyLayout()
        {
            if (!board || !board.BoardCamera) return;
            float aspect = board.BoardCamera.aspect;
            bool wide = aspect >= 1.25f;
            board.SetPlayableArea(wide ? new Rect(.33f, .015f, .665f, .97f)
                                       : new Rect(.015f, .015f, .97f, .72f));
            // Project the actual board frame so both panels stay aligned when the window changes.
            Rect panelArea = wide ? new Rect(.055f, .053f, .255f, .894f)
                                  : new Rect(.045f, .742f, .91f, .24f);
            if (wide && boardFrame)
            {
                Bounds bounds = boardFrame.bounds;
                float bottom = board.BoardCamera.WorldToViewportPoint(bounds.min).y;
                float top = board.BoardCamera.WorldToViewportPoint(bounds.max).y;
                panelArea = new Rect(.055f, bottom, .255f, top - bottom);
            }
            Place(sidebar, panelArea);
            if (header.parent != sidebar) header.SetParent(sidebar, false);
            Place(header, wide ? new Rect(.065f, .865f, .87f, .095f) : new Rect(.07f, .78f, .86f, .20f));
            Place(stage, wide ? new Rect(.06f, .755f, .88f, .075f) : new Rect(.18f, .635f, .64f, .13f));
            Place(moves, wide ? new Rect(.20f, .525f, .60f, .20f) : new Rect(.02f, .255f, .25f, .35f));
            Place(score, wide ? new Rect(.09f, .35f, .82f, .15f) : new Rect(.30f, .315f, .29f, .29f));
            Place(goal, wide ? new Rect(.09f, .24f, .82f, .10f) : new Rect(.64f, .405f, .32f, .20f));
            Place(progress, wide ? new Rect(.12f, .21f, .76f, .025f) : new Rect(.31f, .255f, .28f, .025f));
            Place(itemSlots, wide ? new Rect(.075f, .115f, .85f, .075f) : new Rect(.21f, .035f, .58f, .175f));
            Place(restart, wide ? new Rect(.09f, .025f, .82f, .07f) : new Rect(.64f, .245f, .32f, .14f));
            if (settings)
            {
                Place(restart, wide ? new Rect(.075f,.025f,.52f,.07f) : new Rect(.64f,.245f,.32f,.14f));
                Place(settings, wide ? new Rect(.635f,.025f,.29f,.07f) : new Rect(.02f,.035f,.17f,.175f));
            }
            if (collectionGoals)
            {
                Place(header, wide ? new Rect(.065f,.90f,.87f,.075f) : new Rect(.07f,.82f,.86f,.17f));
                Place(worldTitle, wide ? new Rect(.08f,.85f,.84f,.04f) : new Rect(.02f,.67f,.25f,.11f));
                Place(stage, wide ? new Rect(.06f,.755f,.88f,.075f) : new Rect(.30f,.65f,.65f,.14f));
                Place(moves, wide ? new Rect(.20f,.535f,.60f,.20f) : new Rect(.02f,.30f,.25f,.32f));
                Place(score, wide ? new Rect(.09f,.385f,.82f,.135f) : new Rect(.30f,.34f,.29f,.27f));
                Place(goal, wide ? new Rect(.09f,.295f,.82f,.08f) : new Rect(.64f,.40f,.32f,.20f));
                Place(progress, wide ? new Rect(.12f,.27f,.76f,.022f) : new Rect(.31f,.29f,.28f,.025f));
                Place(collectionGoals, wide ? new Rect(.08f,.185f,.84f,.075f) : new Rect(.30f,.11f,.63f,.15f));
                Place(itemSlots, wide ? new Rect(.075f,.105f,.85f,.065f) : new Rect(.22f,.015f,.58f,.07f));
            }
            previousAspect = aspect;
            FitBackdrop();
        }

        private void FitBackdrop()
        {
            if (!backdrop || !backdrop.sprite) return;
            Camera camera = board.BoardCamera;
            float height = camera.orthographicSize * 2;
            Vector2 size = backdrop.sprite.bounds.size;
            float scale = Mathf.Max(height / size.y, height * camera.aspect / size.x);
            backdrop.transform.localScale = Vector3.one * scale;
            backdrop.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, 4);
        }

        private static void Place(RectTransform target, Rect area)
        {
            target.anchorMin = area.min; target.anchorMax = area.max;
            target.offsetMin = target.offsetMax = Vector2.zero;
            target.localScale = Vector3.one;
        }
    }
}
