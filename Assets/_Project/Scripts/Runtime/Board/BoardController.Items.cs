using System;
using System.Collections;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Services;

namespace PuzzleGame.Runtime.Board
{
    public sealed partial class BoardController
    {
        private ItemType? pendingItem;
        public ItemType? SelectedItem { get; private set; }
        public event Action ItemSelectionChanged;
        public event Action ItemUseFailed;
        public string ItemFailureKey { get; private set; } = "itemUnavailable";
        public bool CanUseItem(ItemType type) => ItemRules.IsValid(type) && isActiveAndEnabled && !IsBusy && !IsPaused
            && Model != null && levelSession && levelSession.CanPlay && levelSession.ItemCount(type)>0;

        public bool ArmItem(ItemType type)
        {
            if (!ItemRules.NeedsTarget(type) || !CanUseItem(type)) return false;
            SetSelection(null); SelectedItem=type; ItemSelectionChanged?.Invoke(); return true;
        }
        public void CancelItem()
        {
            if (!SelectedItem.HasValue) return;
            SelectedItem=null; if(view)view.ShowSelection(null); ItemSelectionChanged?.Invoke();
        }
        public bool TryUseItem(ItemType type,GridPosition? target=null)
        {
            ItemFailureKey="itemUnavailable";
            if (!CanUseItem(type) || (ItemRules.NeedsTarget(type) && (!target.HasValue || !CanTarget(target.Value)))) return false;
            BoardState shuffled=null;
            if(type==ItemType.Shuffle)
            {
                shuffled=new BoardState(Model.Width,Model.Height);
                for(int y=0;y<Model.Height;y++)for(int x=0;x<Model.Width;x++)
                { var p=new GridPosition(x,y);shuffled.SetPiece(p,Model.GetPiece(p));shuffled.SetCell(p,Model.GetCell(p)); }
                if(!BoardShuffler.TryShuffle(shuffled,random)){ItemFailureKey="shuffleUnavailable";ItemUseFailed?.Invoke();return false;}
            }
            if(!levelSession.TrySpendItem(type)){ItemFailureKey="itemSaveFailed";ItemUseFailed?.Invoke();return false;}
            CancelItem(); SetSelection(null);
            if(type==ItemType.ExtraMoves)
            {
                if(!levelSession.AddItemMoves()){levelSession.RefundItem(type);return false;}
                levelSession.RecordItemUse();
                if(levelSession.IsCampaignRun)PlaytestJournal.Item(type);
                GameAudio.Play("special"); Activity?.Invoke();return true;
            }
            if(!levelSession.BeginItem()){levelSession.RefundItem(type);return false;}
            pendingItem=type;
            turnSnapshot=new PieceState[Model.Width,Model.Height];cellSnapshot=new CellState[Model.Width,Model.Height];
            for(int y=0;y<Model.Height;y++)for(int x=0;x<Model.Width;x++)
            {var p=new GridPosition(x,y);turnSnapshot[x,y]=Model.GetPiece(p);cellSnapshot[x,y]=Model.GetCell(p);}
            IsBusy=true; Activity?.Invoke();StartCoroutine(UseItem(type,target,shuffled));return true;
        }

        private IEnumerator UseItem(ItemType type,GridPosition? target,BoardState shuffled)
        {
            bool blocked=false;
            GameAudio.Play("special");
            if(shuffled!=null)
            {
                for(int y=0;y<Model.Height;y++)for(int x=0;x<Model.Width;x++)
                {var p=new GridPosition(x,y);Model.SetPiece(p,shuffled.GetPiece(p));}
                yield return view.AnimateToBoard(Model,fallDuration*2);
                while(IsPaused)yield return null;
            }
            else
            {
                int cascade=0;
                do
                {
                    if(++cascade>128){FailTurn("Item cascade limit exceeded");yield break;}
                    var step=cascade==1?resolver.ResolveItem(Model,target.Value,type==ItemType.Bomb?1:0):resolver.ResolveMatches(Model);
                    if(cascade>1)GameAudio.Play(step.SpecialActivations.Count>0?"special":"cascade");
                    view.ShowSpecialCreations(step);ResolutionStarted?.Invoke(step);
                    yield return view.AnimateResolution(step);
                    while(IsPaused)yield return null;
                    levelSession.ApplyRemoval(step);
                    yield return view.AnimateToBoard(Model,fallDuration,step);
                    while(IsPaused)yield return null;
                }while(MatchFinder.FindMatches(Model).Count>0);
                if(!levelSession.Progress.WillFinish && !MoveFinder.HasAnyMove(Model))
                {
                    if(!BoardShuffler.TryShuffle(Model,random))blocked=true;
                    else yield return view.AnimateToBoard(Model,fallDuration);
                    while(IsPaused)yield return null;
                }
            }
            if(levelSession.IsCampaignRun)PlaytestJournal.Item(type,target?.X??-1,target?.Y??-1);
            pendingItem=null;turnSnapshot=null;cellSnapshot=null;IsBusy=false;
            levelSession.RecordItemUse();
            levelSession.CompleteMove(blocked);
        }
        private void RefundPendingItem()
        {
            if(!pendingItem.HasValue)return;
            var type=pendingItem.Value;pendingItem=null;
            if(levelSession)levelSession.RefundItem(type);
        }
    }
}
