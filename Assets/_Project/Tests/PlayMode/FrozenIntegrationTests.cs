using System.Collections;
using System.Linq;
using NUnit.Framework;
using PuzzleGame.Core.Board;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.Services;
using PuzzleGame.Runtime.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace PuzzleGame.Tests
{
    public sealed class FrozenIntegrationTests
    {
        [SetUp] public void Setup(){Time.timeScale=1;GamePreferences.Load();if(CampaignState.Instance)Object.DestroyImmediate(CampaignState.Instance.gameObject);}
        [TearDown] public void Cleanup(){Object.FindFirstObjectByType<PlayerDialogs>()?.Close();Time.timeScale=1;GamePreferences.Load();}
        [UnityTest] public IEnumerator FrozenPieceAndViewStayStillDuringGravityAndItemShuffle()
        {
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var board=Object.FindFirstObjectByType<BoardController>();var at=new GridPosition(3,3);
            board.Model.SetCell(at,new CellState(2));board.GetComponent<BoardView>().Rebuild(board.Model);yield return null;
            int id=board.Model.GetPiece(at).Id;var piece=board.GetComponentsInChildren<PieceView>().Single(p=>p.PieceId==id);var position=piece.transform.localPosition;
            Assert.That(board.TryUseItem(ItemType.Hammer,new GridPosition(3,1)),Is.True);
            yield return CheckPinned(board,piece,at,position);
            Assert.That(board.TryUseItem(ItemType.Shuffle),Is.True);yield return CheckPinned(board,piece,at,position);
            Assert.That(board.Session.ItemCount(ItemType.Shuffle),Is.EqualTo(2));Assert.That(board.Model.GetCell(at).FrostHealth,Is.EqualTo(2));
        }
        [UnityTest] public IEnumerator InterruptedIceHitRestoresCandyIceProgressAndInventory()
        {
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var board=Object.FindFirstObjectByType<BoardController>();var at=new GridPosition(3,3);int id=board.Model.GetPiece(at).Id;
            board.Model.SetCell(at,new CellState(1));board.GetComponent<BoardView>().Rebuild(board.Model);
            Assert.That(board.TryUseItem(ItemType.Hammer,at),Is.True);board.enabled=false;yield return null;
            Assert.That(board.Model.GetPiece(at).Id,Is.EqualTo(id));Assert.That(board.Model.GetCell(at).FrostHealth,Is.EqualTo(1));
            Assert.That(board.Session.ItemCount(ItemType.Hammer),Is.EqualTo(3));Assert.That(board.Session.Progress.FrostCleared,Is.Zero);Assert.That(board.Session.Progress.Score,Is.Zero);
            board.enabled=true;
        }
        [UnityTest] public IEnumerator ImpossibleShuffleKeepsStockAndExplainsWhy()
        {
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var board=Object.FindFirstObjectByType<BoardController>();
            for(int y=0;y<8;y++)for(int x=0;x<8;x++)board.Model.SetCell(new GridPosition(x,y),new CellState(2));
            board.GetComponent<BoardView>().Rebuild(board.Model);
            Assert.That(board.TryUseItem(ItemType.Shuffle),Is.False);yield return null;
            Assert.That(board.Session.ItemCount(ItemType.Shuffle),Is.EqualTo(3));Assert.That(board.ItemFailureKey,Is.EqualTo("shuffleUnavailable"));
            var dialogs=Object.FindFirstObjectByType<PlayerDialogs>();Assert.That(dialogs.IsVisible,Is.True);dialogs.Close();
        }
        private static IEnumerator CheckPinned(BoardController board,PieceView piece,GridPosition at,Vector3 position)
        {
            float until=Time.realtimeSinceStartup+15;
            do
            {
                Assert.That(board.Model.GetPiece(at).Id,Is.EqualTo(piece.PieceId));Assert.That(piece.transform.localPosition,Is.EqualTo(position));
                yield return null;
            }while(board.IsBusy && Time.realtimeSinceStartup<until);
            Assert.That(board.IsBusy,Is.False);
        }
    }
}
