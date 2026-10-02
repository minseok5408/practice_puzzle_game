using System.Collections;
using NUnit.Framework;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Services;
using PuzzleGame.Runtime.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PuzzleGame.Tests
{
    public sealed class CommonUIStyleTests
    {
        [TearDown]
        public void Cleanup()
        {
            Object.FindFirstObjectByType<PlayerDialogs>()?.Close();
            Object.FindFirstObjectByType<SettingsPopup>()?.Close();
            Time.timeScale=1;GamePreferences.Load();
        }

        [UnityTest]
        public IEnumerator HeaderCloseRestoresNestedPauseAndSettingsCloseResumesPlay()
        {
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var settings=Object.FindFirstObjectByType<SettingsPopup>();
            var dialogs=settings.GetComponent<PlayerDialogs>();
            settings.Open();dialogs.Preferences();yield return null;
            dialogs.transform.Find("PlayerDialog/Card/CloseDialog").GetComponent<Button>().onClick.Invoke();
            Assert.That(dialogs.IsVisible,Is.False);
            Assert.That(settings.IsVisible,Is.False);
            Assert.That(Time.timeScale,Is.EqualTo(1));
            settings.Open();settings.CloseButton.onClick.Invoke();
            Assert.That(settings.IsVisible,Is.False);
            Assert.That(Object.FindFirstObjectByType<BoardController>().IsPaused,Is.False);
            Assert.That(Time.timeScale,Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator KeyboardFocusRemainsVisibleWithoutMotionAndDisabledButtonsReset()
        {
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var settings=Object.FindFirstObjectByType<SettingsPopup>();settings.Open();
            var button=settings.CloseButton;
            EventSystem.current.SetSelectedGameObject(button.gameObject);
            var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left};
            ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerDownHandler);
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(button.transform.localScale.x,Is.LessThan(1));
            GamePreferences.Current.reducedEffects=true;
            yield return null;
            Assert.That(button.transform.localScale,Is.EqualTo(Vector3.one));
            Assert.That(button.GetComponent<Outline>().enabled,Is.True);
            button.interactable=false;yield return null;
            Assert.That(button.GetComponent<Outline>().enabled,Is.False);
            settings.Close();settings.Open();yield return null;
            button=settings.CloseButton;
            Assert.That(button.transform.localScale,Is.EqualTo(Vector3.one));
        }
    }
}
