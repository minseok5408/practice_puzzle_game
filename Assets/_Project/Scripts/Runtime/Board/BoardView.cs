using System;
using System.Collections;
using System.Collections.Generic;
using PuzzleGame.Core.Board;
using PuzzleGame.Runtime.Config;
using UnityEngine;

namespace PuzzleGame.Runtime.Board
{
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private PieceView piecePrefab;
        [SerializeField] private CellView cellPrefab;
        [SerializeField] private PieceCatalog catalog;
        [SerializeField] private Camera boardCamera;
        [SerializeField] private TMPro.TMP_FontAsset colorLabelFont;
        [SerializeField] private Sprite colorLabelBadge;
        public void ConfigureColorLabels(TMPro.TMP_FontAsset font,Sprite badge){colorLabelFont=font;colorLabelBadge=badge;}
        private void OnEnable()=>PuzzleGame.Runtime.Services.GamePreferences.Changed+=RefreshColorLabels;
        private void RefreshColorLabels()
        {foreach(var piece in pieces.Values)piece.ShowColorLabel(PuzzleGame.Runtime.Services.GamePreferences.Current.colorLabels,colorLabelFont,colorLabelBadge);}
        private readonly Dictionary<int, PieceView> pieces = new Dictionary<int, PieceView>();
        private readonly Dictionary<GridPosition, CellView> cells = new Dictionary<GridPosition, CellView>();
        [SerializeField, Range(0f, 0.4f)] private float topInset = 0.23f;
        [SerializeField, Range(0f, 0.25f)] private float bottomInset = 0.12f;
        [SerializeField] private bool usePlayableArea;
        [SerializeField] private Rect playableArea = new Rect(0, 0, 1, 1);
        private Transform content;
        private BoardEffects effects;
        private BoardHintView hint;
        public BoardEffects Effects => effects;
        private int boardWidth;
        private int boardHeight;
        private float previousAspect = -1f;
        public Camera BoardCamera => boardCamera;
        public int PieceCount => pieces.Count;

        public void Configure(PieceView piece, CellView cell, PieceCatalog appearances, Camera camera)
        { piecePrefab = piece; cellPrefab = cell; catalog = appearances; boardCamera = camera; }

        public void Rebuild(BoardState board)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            if (!piecePrefab || !cellPrefab || !catalog || !boardCamera)
                throw new InvalidOperationException("BoardView is missing a prefab, catalog, or camera reference.");
            if (effects) effects.Clear();
            if (hint) hint.Hide();
            if (content != null)
            {
                content.gameObject.SetActive(false);
                if (Application.isPlaying) Destroy(content.gameObject);
                else DestroyImmediate(content.gameObject);
            }
            pieces.Clear();
            cells.Clear();
            content = new GameObject("GridContent").transform;
            content.SetParent(transform, false);
            boardWidth = board.Width;
            boardHeight = board.Height;
            for (int y = 0; y < board.Height; y++)
                for (int x = 0; x < board.Width; x++)
                {
                    var position = new GridPosition(x, y);
                    CellView cell = Instantiate(cellPrefab, content);
                    cell.transform.localPosition = CellToLocal(position);
                    cell.Initialize(position);
                    cell.ShowFrost(board.GetCell(position).FrostHealth);
                    cells.Add(position, cell);
                    PieceState piece = board.GetPiece(position);
                    if (piece != null) CreatePiece(piece, position, position);
                }
            FitCamera();
            if (!effects)
            {
                effects = new GameObject("CandyEffects").AddComponent<BoardEffects>();
                effects.transform.SetParent(transform, false);
            }
        }

        private PieceView CreatePiece(PieceState piece, GridPosition position, GridPosition start)
        {
            PieceView result = Instantiate(piecePrefab, content);
            result.transform.localPosition = CellToLocal(start);
            result.Initialize(piece, position, catalog.Get(piece));
            result.ShowColorLabel(PuzzleGame.Runtime.Services.GamePreferences.Current.colorLabels,colorLabelFont,colorLabelBadge);
            pieces.Add(piece.Id, result);
            return result;
        }

        public void ShowSelection(GridPosition? position)
        {
            if(hint)hint.Hide();
            foreach(var cell in cells.Values)cell.ShowItemTarget(false);
            foreach (var piece in pieces.Values) piece.SetSelected(position == piece.Position);
        }

        public void ShowHint(GridPosition? first, GridPosition? second)
        {
            if(!first.HasValue || !second.HasValue){if(hint)hint.Hide();return;}
            foreach(var piece in pieces.Values)piece.SetSelected(false);
            if(!hint){hint=new GameObject("MoveHint").AddComponent<BoardHintView>();hint.transform.SetParent(transform,false);}
            hint.Show(CellToLocal(first.Value),CellToLocal(second.Value));
        }

        public void ShowItemTarget(GridPosition? target, int radius)
        {
            if (hint) hint.Hide();
            foreach(var cell in cells.Values)
                cell.ShowItemTarget(target.HasValue && Mathf.Abs(cell.Position.X-target.Value.X)<=radius && Mathf.Abs(cell.Position.Y-target.Value.Y)<=radius);
            foreach (var piece in pieces.Values)
                piece.SetSelected(target.HasValue && Mathf.Abs(piece.Position.X-target.Value.X)<=radius && Mathf.Abs(piece.Position.Y-target.Value.Y)<=radius);
        }

        public void ShowSpecialCreations(ResolutionStep step)
        {
            foreach (var creation in step.SpecialCreations)
                pieces[creation.Piece.Id].SetAppearance(creation.Piece, catalog.Get(creation.Piece));
        }

        public void ResetDrag(BoardState board)
        {
            foreach (var piece in pieces.Values) piece.transform.localPosition = CellToLocal(piece.Position);
        }

        public void PreviewDrag(GridPosition position, Vector2 delta)
        {
            Vector3 offset = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)
                ? new Vector3(Mathf.Clamp(delta.x, -0.8f, 0.8f), 0, 0)
                : new Vector3(0, Mathf.Clamp(delta.y, -0.8f, 0.8f), 0);
            foreach (var piece in pieces.Values)
                if (piece.Position == position) piece.transform.localPosition = CellToLocal(position) + offset;
        }

        public IEnumerator AnimateToBoard(BoardState board, float duration, ResolutionStep step = null)
        {
            var starts = new Dictionary<int, Vector3>();
            var spawnStarts = new Dictionary<int, GridPosition>();
            var pocketSpawns = new HashSet<int>();
            if (step != null)
                foreach (var spawn in step.Spawns) { spawnStarts.Add(spawn.PieceId, spawn.From); if(spawn.From==spawn.To)pocketSpawns.Add(spawn.PieceId); }
            for (int y = 0; y < board.Height; y++)
                for (int x = 0; x < board.Width; x++)
                {
                    var position = new GridPosition(x, y);
                    PieceState model = board.GetPiece(position);
                    if (model == null) continue;
                    if (!pieces.TryGetValue(model.Id, out PieceView piece))
                        piece = CreatePiece(model, position, spawnStarts.TryGetValue(model.Id, out var start) ? start : position);
                    starts.Add(model.Id, piece.transform.localPosition);
                    piece.SetPosition(position);
                }
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime * PuzzleGame.Runtime.Services.GamePreferences.Current.animationSpeed;
                float progress = Mathf.Clamp01(elapsed / duration);
                float t = step == null ? Mathf.SmoothStep(0, 1, progress) : progress * progress;
                foreach (var pair in starts)
                {
                    PieceView piece = pieces[pair.Key];
                    piece.transform.localPosition = Vector3.Lerp(pair.Value, CellToLocal(piece.Position), t);
                    if(pocketSpawns.Contains(pair.Key))piece.SetSpawnProgress(progress);
                }
                yield return null;
            }
            ResetDrag(board);
            if (step != null)
            {
                elapsed = 0;
                while (elapsed < .12f)
                {
                    elapsed += Time.deltaTime * PuzzleGame.Runtime.Services.GamePreferences.Current.animationSpeed;
                    foreach (var pair in starts)
                    {
                        PieceView piece = pieces[pair.Key];
                        if ((pair.Value - CellToLocal(piece.Position)).sqrMagnitude > .01f)
                            piece.SetLandingProgress(elapsed / .12f);
                    }
                    yield return null;
                }
                foreach (var piece in pieces.Values) piece.ResetMotion();
            }
        }

        public IEnumerator AnimateResolution(ResolutionStep step)
        {
            bool reduced = PuzzleGame.Runtime.Services.GamePreferences.Current.reducedEffects;
            if (!reduced) effects.Begin(step, pieces, boardWidth, boardHeight);
            float elapsed = 0;
            float duration = reduced ? .16f : effects.Duration;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime * PuzzleGame.Runtime.Services.GamePreferences.Current.animationSpeed;
                if (!reduced) effects.Sample(elapsed);
                foreach (int id in step.RemovedIds)
                    pieces[id].SetRemovalProgress(reduced ? elapsed / duration : (elapsed - effects.HitTime(id)) / BoardEffects.PopDuration);
                foreach (var creation in step.SpecialCreations)
                    pieces[creation.Piece.Id].SetFormationProgress(elapsed / .6f);
                foreach (var damage in step.FrostDamage)
                    cells[damage.Position].DamageProgress(damage.RemainingHealth, (elapsed - duration + .18f) / .18f, reduced);
                yield return null;
            }
            foreach (var damage in step.FrostDamage) cells[damage.Position].ShowFrost(damage.RemainingHealth);
            effects.Clear();
            foreach (var creation in step.SpecialCreations) pieces[creation.Piece.Id].ResetMotion();
            foreach (int id in step.RemovedIds)
            {
                PieceView piece = pieces[id];
                pieces.Remove(id);
                piece.gameObject.SetActive(false);
                Destroy(piece.gameObject);
            }
        }

        public Vector3 CellToLocal(GridPosition position) =>
            new Vector3(position.X - (boardWidth - 1) * 0.5f, position.Y - (boardHeight - 1) * 0.5f, 0f);

        public bool TryScreenToLocal(Vector2 screen, out Vector2 local)
        {
            local = default;
            if (!boardCamera || !boardCamera.pixelRect.Contains(screen)) return false;
            var plane = new Plane(transform.forward, transform.position);
            Ray ray = boardCamera.ScreenPointToRay(screen);
            if (!plane.Raycast(ray, out float distance)) return false;
            local = transform.InverseTransformPoint(ray.GetPoint(distance));
            return true;
        }

        public bool TryScreenToCell(Vector2 screen, out GridPosition position)
        {
            position = default;
            if (!TryScreenToLocal(screen, out Vector2 local)) return false;
            int x = Mathf.FloorToInt(local.x + boardWidth * 0.5f);
            int y = Mathf.FloorToInt(local.y + boardHeight * 0.5f);
            if (x < 0 || y < 0 || x >= boardWidth || y >= boardHeight) return false;
            position = new GridPosition(x, y);
            return true;
        }

        public void SetPlayableArea(Rect area)
        {
            if (area.width <= 0 || area.height <= 0) throw new ArgumentOutOfRangeException(nameof(area));
            usePlayableArea = true;
            playableArea = area;
            FitCamera();
        }

        public void FitCamera()
        {
            if (!boardCamera || boardWidth == 0) return;
            float aspect = Mathf.Max(0.1f, boardCamera.aspect);
            float halfWidth = boardWidth * 0.5f + 0.7f;
            float halfHeight = boardHeight * 0.5f + 0.7f;
            boardCamera.orthographic = true;
            boardCamera.orthographicSize = Mathf.Max(halfHeight / (1f - topInset - bottomInset), halfWidth / aspect);
            boardCamera.transform.position = transform.position + new Vector3(0f, (topInset - bottomInset) * boardCamera.orthographicSize, -10f);
            if (usePlayableArea)
            {
                boardCamera.orthographicSize = Mathf.Max(halfHeight / playableArea.height, halfWidth / (aspect * playableArea.width));
                Vector2 center = playableArea.center;
                boardCamera.transform.position = transform.position + new Vector3(
                    (0.5f - center.x) * 2 * boardCamera.orthographicSize * aspect,
                    (0.5f - center.y) * 2 * boardCamera.orthographicSize, -10);
            }
            previousAspect = aspect;
        }

        private void LateUpdate()
        {
            if (boardCamera && !Mathf.Approximately(previousAspect, boardCamera.aspect)) FitCamera();
        }

        private void OnDisable() { PuzzleGame.Runtime.Services.GamePreferences.Changed-=RefreshColorLabels;if (effects) effects.Clear(); }
    }
}
