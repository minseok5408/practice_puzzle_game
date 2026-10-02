#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.Services;
using PuzzleGame.Runtime.UI;
using UnityEngine;

namespace PuzzleGame.Runtime.Diagnostics
{
    public sealed partial class ItemsSmokeCheck
    {
        private IEnumerator RunFrozenCheck()
        {
            var state=CampaignState.Instance;
            for(int n=1;n<50;n++)state.Progress.RecordWin(n,3000);
            FindFirstObjectByType<WorldMapView>().Enter(1);yield return WaitScene("Game");if(failed)yield break;
            board=FindFirstObjectByType<BoardController>();var session=board.Session;var at=new GridPosition(3,3);var beside=new GridPosition(4,3);
            board.Model.SetCell(at,new CellState(3));board.GetComponent<BoardView>().Rebuild(board.Model);yield return null;
            int id=board.Model.GetPiece(at).Id,moves=session.Progress.MovesRemaining;
            yield return Capture("FrozenLocked.png");
            yield return MouseAt(CellScreen(at),true);yield return MouseAt(CellScreen(beside),true);yield return MouseAt(CellScreen(beside),false);
            yield return MouseAt(CellScreen(beside),true);yield return MouseAt(CellScreen(at),true);yield return MouseAt(CellScreen(at),false);
            if(!Check(()=>Require(!board.IsBusy && !board.TrySwap(at,beside) && !board.TrySwap(beside,at) && board.Model.GetPiece(at).Id==id && session.Progress.MovesRemaining==moves,"Frozen drag or incoming swap moved a candy / spent a move.")))yield break;
            board.SetSelection(null);
            var pinned=FrozenIds();
            if(!Check(()=>Require(board.TryUseItem(ItemType.Hammer,new GridPosition(3,1)),"Hammer below ice rejected.")))yield break;
            yield return SettleFrozen(pinned);if(failed)yield break;
            pinned=FrozenIds();
            if(!Check(()=>Require(board.TryUseItem(ItemType.Shuffle),"Legal shuffle rejected.")))yield break;
            yield return SettleFrozen(pinned);if(failed)yield break;
            // Use a fresh stable board so each of the three hits isolates ice damage from cascades.
            session.RestartLevel();yield return null;
            board.Model.SetCell(at,new CellState(3));board.GetComponent<BoardView>().Rebuild(board.Model);id=board.Model.GetPiece(at).Id;
            session.Campaign.items[(int)ItemType.Hammer]=3;state.Save();
            for(int health=2;health>=0;health--)
            {
                if(!Check(()=>Require(board.ArmItem(ItemType.Hammer),"Frozen target could not arm hammer.")))yield break;
                yield return ClickCell(at);yield return Settle();if(failed)yield break;
                int remaining=health;
                if(!Check(()=>Require(board.Model.GetCell(at).FrostHealth==remaining && board.Model.GetPiece(at).Id==id && session.Progress.Score==0 && session.Progress.MovesRemaining==moves && board.CanSelect(at)==(remaining==0),"Ice did not absorb exactly one hit and release the same candy after the last layer.")))yield break;
                yield return Capture("FrozenLayers"+health+".png");
            }
            var dialogs=FindFirstObjectByType<PlayerDialogs>();
            foreach(string language in new[]{"ko","en"})
            {
                GamePreferences.Current.language=language;GamePreferences.Save();dialogs.Guide(2);
                yield return Capture("FrozenGuide-"+language+".png");yield return Capture("FrozenGuideSmall-"+language+".png",960,600);dialogs.Close();
            }
            GamePreferences.Current.language="ko";GamePreferences.Save();
            foreach(int stage in new[]{5,46,49,50})
            {
                session.SelectLevel(stage);yield return null;yield return ReplayFrozen(stage);if(failed)yield break;
                yield return new WaitForSecondsRealtime(.8f);yield return Capture("FrozenWin-"+stage+".png");
            }
            // A board that cannot shuffle must explain the failure without charging inventory or hanging.
            session.SelectLevel(1);yield return null;session.Campaign.items[(int)ItemType.Shuffle]=3;session.Campaign.items[(int)ItemType.Hammer]=3;state.Save();
            for(int y=0;y<8;y++)for(int x=0;x<8;x++)board.Model.SetCell(new GridPosition(x,y),new CellState(2));
            board.GetComponent<BoardView>().Rebuild(board.Model);
            if(!Check(()=>Require(!board.TryUseItem(ItemType.Shuffle) && session.ItemCount(ItemType.Shuffle)==3 && board.ItemFailureKey=="shuffleUnavailable" && dialogs.IsVisible,"Impossible shuffle was charged or lacked an explanation.")))yield break;
            yield return Capture("FrozenShuffleUnavailable.png");dialogs.Close();
            if(!Check(()=>Require(board.TryUseItem(ItemType.Hammer,at),"Blocked-board fixture rejected hammer.")))yield break;
            yield return Settle();if(failed)yield break;
            if(!Check(()=>Require(session.Progress.Outcome==LevelOutcome.Lost && session.Progress.IsBlocked && session.Progress.MovesRemaining>0,"Blocked board did not finish safely.")))yield break;
            yield return new WaitForSecondsRealtime(.8f);yield return Capture("FrozenBlockedResult.png");
            Finish(0,"PASS: real pointer swaps blocked in both directions; frozen model and view stayed fixed during gravity, shuffle and four winning routes (1-5, 5-6, 5-9, 5-10); three hammer hits removed one ice layer each and preserved candy, score and moves; last layer released selection; impossible shuffle kept stock; blocked board ended without hanging; Korean/English guide and result captures.");
        }
        private Dictionary<GridPosition,int> FrozenIds()
        {
            var result=new Dictionary<GridPosition,int>();
            for(int y=0;y<8;y++)for(int x=0;x<8;x++){var p=new GridPosition(x,y);if(board.Model.IsFrozen(p))result.Add(p,board.Model.GetPiece(p).Id);}
            return result;
        }
        private IEnumerator SettleFrozen(Dictionary<GridPosition,int> pinned)
        {
            float until=Time.realtimeSinceStartup+30;var view=board.GetComponent<BoardView>();
            do
            {
                if(!Check(()=>{
                    foreach(var pair in pinned)
                    {
                        if(!board.Model.IsFrozen(pair.Key))continue;
                        Require(board.Model.GetPiece(pair.Key).Id==pair.Value,"Frozen model moved.");
                        var piece=board.GetComponentsInChildren<PieceView>().Single(p=>p.PieceId==pair.Value);
                        Require(Vector3.Distance(piece.transform.localPosition,view.CellToLocal(pair.Key))<.001f,"Frozen view moved.");
                    }
                }))yield break;
                yield return null;
            }while(board.IsBusy && Time.realtimeSinceStartup<until);
            Check(()=>Require(!board.IsBusy,"Frozen resolution timed out."));
        }
        private IEnumerator ReplayFrozen(int stage)
        {
            string route=null;
            if(!Check(()=>{foreach(string line in System.IO.File.ReadAllLines(Arg("-puzzleWinningRoutes")))if(line.StartsWith(stage+"|"))route=line.Substring(line.IndexOf('|')+1);Require(route!=null,"Missing frozen route.");}))yield break;
            foreach(string move in route.Split(';'))
            {
                var v=System.Array.ConvertAll(move.Split(','),int.Parse);var pinned=FrozenIds();
                if(!Check(()=>Require(board.TrySwap(new GridPosition(v[0],v[1]),new GridPosition(v[2],v[3])),"Frozen route swap rejected.")))yield break;
                yield return SettleFrozen(pinned);if(failed)yield break;
            }
            Check(()=>Require(board.Session.Progress.Outcome==LevelOutcome.Won,"Frozen route did not win."));
        }
    }
}
#endif
