using System;
using System.Collections;
using PuzzleGame.Core.Board;
using PuzzleGame.Runtime.Levels;
using UnityEngine;

namespace PuzzleGame.Runtime.Board
{
    [RequireComponent(typeof(BoardView))]
    public sealed partial class BoardController : MonoBehaviour
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
        private CellState[,] cellSnapshot;
        private LevelDefinition levelDefinition;
        public event Action Activity;
        public event Action<ResolutionStep> ResolutionStarted;
        public BoardState Model { get; private set; }
        public int Seed => seed;
        public bool IsBusy { get; private set; }
        public bool IsPaused { get; private set; }
        public void SetPaused(bool paused) { IsPaused = paused; if (paused) CancelItem(); }
        public GridPosition? SelectedPosition { get; private set; }
        public int CompletedMoves { get; private set; }

        private void Awake() => view = GetComponent<BoardView>();
        private void Start()
        {
            if (levelSession) levelSession.StartInitialLevel();
            else GenerateBoard();
        }

        public void ConfigureLevel(int boardSeed) => seed = boardSeed;
        public void ConfigureLevel(LevelDefinition level) { levelDefinition = level; seed = level.Seed; }

        public void GenerateBoard()
        {
            StopAllCoroutines();
            RestoreInterruptedTurn();
            turnSnapshot = null;
            cellSnapshot = null;
            Activity?.Invoke();
            IsBusy = false;
            SelectedPosition = null;
            CompletedMoves = 0;
            Model = levelDefinition ? levelDefinition.CreateBoard() : BoardGenerator.Generate(width, height, colorCount, seed);
            random = new System.Random(seed);
            resolver = new BoardResolver(Model, colorCount, random);
            if (!view) view = GetComponent<BoardView>();
            view.Rebuild(Model);
            if (levelSession) levelSession.ResetProgress();
        }

        public bool CanTarget(GridPosition position) =>
            isActiveAndEnabled && !IsPaused && !IsBusy && (!levelSession || levelSession.CanPlay) && Model != null && Model.Contains(position)
            && Model.GetPiece(position) != null;
        public bool CanSelect(GridPosition position) => CanTarget(position) && !Model.IsFrozen(position);

        public void SetSelection(GridPosition? position)
        {
            if (IsBusy) return;
            if (position.HasValue && !CanSelect(position.Value)) return;
            Activity?.Invoke();
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
            if (SelectedItem.HasValue) return false;
            if (!CanSelect(first) || !CanSelect(second) || !first.IsAdjacentTo(second)) return false;
            Activity?.Invoke();
            bool valid = MoveFinder.IsValidSwap(Model, first, second);
            PuzzleGame.Runtime.Services.GameAudio.Play(valid ? "swap" : "invalid");
            if (valid && levelSession && !levelSession.BeginMove()) return false;
            if (levelSession && levelSession.IsCampaignRun) PuzzleGame.Runtime.Services.PlaytestJournal.Move(first.X, first.Y, second.X, second.Y, valid);
            turnSnapshot = new PieceState[Model.Width, Model.Height];
            cellSnapshot = new CellState[Model.Width, Model.Height];
            for (int y = 0; y < Model.Height; y++)
                for (int x = 0; x < Model.Width; x++)
                {
                    turnSnapshot[x, y] = Model.GetPiece(new GridPosition(x, y));
                    cellSnapshot[x, y] = Model.GetCell(new GridPosition(x, y));
                }
            SelectedPosition = null;
            view.ShowSelection(null);
            IsBusy = true;
            StartCoroutine(SwapAndResolve(first, second, valid));
            return true;
        }

        private IEnumerator SwapAndResolve(GridPosition first, GridPosition second, bool valid)
        {
            bool blocked = false;
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
                    PuzzleGame.Runtime.Services.GameAudio.Play(step.SpecialActivations.Count > 0 ? "special" : cascade > 1 ? "cascade" : "match");
                    view.ShowSpecialCreations(step);
                    ResolutionStarted?.Invoke(step);
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
                        blocked = true;
                    }
                    else yield return view.AnimateToBoard(Model, fallDuration);
                    while (IsPaused) yield return null;
                }
                CompletedMoves++;
            }
            turnSnapshot = null;
            cellSnapshot = null;
            IsBusy = false;
            if (valid && levelSession) levelSession.CompleteMove(blocked);
        }

        private void FailTurn(string reason)
        {
            Debug.LogError($"{reason}; seed={seed}, completedMoves={CompletedMoves}. Restoring the last stable board.", this);
            RestoreInterruptedTurn();
        }

        public bool CompleteStageForEasterEgg()
        {
            if(!isActiveAndEnabled || IsPaused || Model==null || !levelSession || levelSession.Progress==null || levelSession.Progress.IsFinished)return false;
            // Cancel an in-flight swap/cascade before showing its result, just like a restart.
            StopAllCoroutines();
            RestoreInterruptedTurn();
            for(int y=0;y<Model.Height;y++)for(int x=0;x<Model.Width;x++)
                Model.SetCell(new GridPosition(x,y),new CellState());
            view.Rebuild(Model);
            return levelSession.CompleteForEasterEgg();
        }

        private void RestoreInterruptedTurn()
        {
            if (turnSnapshot != null && Model != null)
            {
                for (int y = 0; y < Model.Height; y++)
                    for (int x = 0; x < Model.Width; x++)
                    {
                        Model.SetPiece(new GridPosition(x, y), turnSnapshot[x, y]);
                        Model.SetCell(new GridPosition(x, y), cellSnapshot[x,y]);
                    }
                if (view) view.Rebuild(Model);
                if (levelSession) levelSession.CancelMove();
            }
            turnSnapshot = null;
            cellSnapshot = null;
            IsBusy = false;
            SetSelection(null);
            CancelItem();
            RefundPendingItem();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            RestoreInterruptedTurn();
        }

        private void OnApplicationQuit()
        {
            // Refund an unfinished item before persistent services are destroyed.
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
