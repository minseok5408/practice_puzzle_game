using System;
using System.Collections;
using PuzzleGame.Core.Board;
using PuzzleGame.Runtime.Levels;
using UnityEngine;

namespace PuzzleGame.Runtime.Board
{
    [RequireComponent(typeof(BoardView))]
    public sealed class BoardController : MonoBehaviour
    {
        [SerializeField, Range(3, 16)] private int width = 8;
        [SerializeField, Range(3, 16)] private int height = 8;
        [SerializeField, Range(3, 6)] private int colorCount = 6;
        [SerializeField] private int seed = 5408;
        [SerializeField, Min(0.01f)] private float swapDuration = 0.18f;
        [SerializeField, Min(0.01f)] private float fallDuration = 0.22f;
        [SerializeField] private LevelSession levelSession;
        public LevelSession Session => levelSession;
        public void ConfigureSession(LevelSession session) => levelSession = session;
        private BoardView view;
        private BoardResolver resolver;
        private System.Random random;
        private PieceState[,] turnSnapshot;
        public BoardState Model { get; private set; }
        public int Seed => seed;
        public bool IsBusy { get; private set; }
        public bool IsPaused { get; private set; }
        public void SetPaused(bool paused) => IsPaused = paused;
        public GridPosition? SelectedPosition { get; private set; }
        public int CompletedMoves { get; private set; }

        private void Awake() => view = GetComponent<BoardView>();
        private void Start()
        {
            if (levelSession) levelSession.StartInitialLevel();
            else GenerateBoard();
        }

        public void ConfigureLevel(int boardSeed) => seed = boardSeed;

        public void GenerateBoard()
        {
            StopAllCoroutines();
            turnSnapshot = null;
            IsBusy = false;
            SelectedPosition = null;
            CompletedMoves = 0;
            Model = BoardGenerator.Generate(width, height, colorCount, seed);
            random = new System.Random(seed);
            resolver = new BoardResolver(Model, colorCount, random);
            if (!view) view = GetComponent<BoardView>();
            view.Rebuild(Model);
            if (levelSession) levelSession.ResetProgress();
        }

        public bool CanSelect(GridPosition position) =>
            isActiveAndEnabled && !IsPaused && !IsBusy && (!levelSession || levelSession.CanPlay) && Model != null && Model.Contains(position)
            && Model.GetPiece(position) != null;

        public void SetSelection(GridPosition? position)
        {
            if (IsBusy) return;
            if (position.HasValue && !CanSelect(position.Value)) return;
            if (view && Model != null) view.ResetDrag(Model);
            SelectedPosition = position;
            if (view) view.ShowSelection(position);
        }

        public void SelectCell(GridPosition position)
        {
            if (!CanSelect(position)) return;
            if (SelectedPosition == position) { SetSelection(null); return; }
            if (SelectedPosition.HasValue && SelectedPosition.Value.IsAdjacentTo(position))
                TrySwap(SelectedPosition.Value, position);
            else SetSelection(position);
        }

        public bool TrySwap(GridPosition first, GridPosition second)
        {
            if (!CanSelect(first) || !CanSelect(second) || !first.IsAdjacentTo(second)) return false;
            bool valid = MoveFinder.IsValidSwap(Model, first, second);
            if (valid && levelSession && !levelSession.BeginMove()) return false;
            turnSnapshot = new PieceState[Model.Width, Model.Height];
            for (int y = 0; y < Model.Height; y++)
                for (int x = 0; x < Model.Width; x++)
                    turnSnapshot[x, y] = Model.GetPiece(new GridPosition(x, y));
            SelectedPosition = null;
            view.ShowSelection(null);
            IsBusy = true;
            StartCoroutine(SwapAndResolve(first, second, valid));
            return true;
        }

        private IEnumerator SwapAndResolve(GridPosition first, GridPosition second, bool valid)
        {
            Model.SwapPieces(first, second);
            yield return view.AnimateToBoard(Model, swapDuration);
            while (IsPaused) yield return null;
            if (!valid)
            {
                Model.SwapPieces(first, second);
                yield return view.AnimateToBoard(Model, swapDuration);
                while (IsPaused) yield return null;
            }
            else
            {
                int cascade = 0;
                bool firstStep = true;
                while (firstStep || MatchFinder.FindMatches(Model).Count > 0)
                {
                    if (++cascade > 128)
                    {
                        FailTurn("Cascade limit exceeded");
                        yield break;
                    }
                    ResolutionStep step = resolver.ResolveMatches(Model, firstStep ? first : (GridPosition?)null,
                        firstStep ? second : (GridPosition?)null);
                    firstStep = false;
                    view.ShowSpecialCreations(step);
                    yield return view.AnimateResolution(step);
                    while (IsPaused) yield return null;
                    if (levelSession) levelSession.ApplyRemoval(step);
                    yield return view.AnimateToBoard(Model, fallDuration, step);
                    while (IsPaused) yield return null;
                }
                if ((!levelSession || !levelSession.Progress.WillFinish) && !MoveFinder.HasAnyMove(Model))
                {
                    if (!BoardShuffler.TryShuffle(Model, random))
                    {
                        FailTurn("Could not shuffle a playable board");
                        yield break;
                    }
                    yield return view.AnimateToBoard(Model, fallDuration);
                    while (IsPaused) yield return null;
                }
                CompletedMoves++;
            }
            turnSnapshot = null;
            IsBusy = false;
            if (valid && levelSession) levelSession.CompleteMove();
        }

        private void FailTurn(string reason)
        {
            Debug.LogError($"{reason}; seed={seed}, completedMoves={CompletedMoves}. Restoring the last stable board.", this);
            RestoreInterruptedTurn();
        }

        private void RestoreInterruptedTurn()
        {
            if (turnSnapshot != null && Model != null)
            {
                for (int y = 0; y < Model.Height; y++)
                    for (int x = 0; x < Model.Width; x++)
                        Model.SetPiece(new GridPosition(x, y), turnSnapshot[x, y]);
                if (view) view.Rebuild(Model);
                if (levelSession) levelSession.CancelMove();
            }
            turnSnapshot = null;
            IsBusy = false;
            SetSelection(null);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            RestoreInterruptedTurn();
        }

        [ContextMenu("Generate Next Board")]
        private void GenerateNextBoard()
        {
            if (!Application.isPlaying) return;
            seed = unchecked(seed + 1);
            GenerateBoard();
        }
    }
}
