using System.Collections;
using NUnit.Framework;
using PuzzleGame.Core.Board;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.Services;
using PuzzleGame.Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace PuzzleGame.Tests
{
    public sealed class PlayerDialogTests
    {
        private const string Body="PlayerDialog/Card/Viewport/Content/";
        private const string Footer="PlayerDialog/Card/Footer/";
        [SetUp] public void Setup() { Time.timeScale=1;GamePreferences.Load(); }
        [TearDown] public void Cleanup()
        {
            Object.FindFirstObjectByType<PlayerDialogs>()?.Close();Object.FindFirstObjectByType<SettingsPopup>()?.Close();
            Time.timeScale=1;GamePreferences.Load();
        }

        [UnityTest]
        public IEnumerator AudioControlsUpdateInPlaceAndMutePreservesVolume()
        {
            yield return SceneManager.LoadSceneAsync("WorldMap");yield return null;
            var dialogs=Object.FindFirstObjectByType<PlayerDialogs>();dialogs.Preferences();yield return null;
            var slider=dialogs.transform.Find(Body+"music/Volume").GetComponent<Slider>();
            var mute=dialogs.transform.Find(Body+"music/Mute").GetComponent<Toggle>();
            float effects=GamePreferences.Current.effectsVolume;
            slider.value=37;
            // Click the actual rail, then continue a drag without rebuilding the control.
            var rail=(RectTransform)slider.handleRect.parent;
            var point=rail.TransformPoint(new Vector3(Mathf.Lerp(rail.rect.xMin,rail.rect.xMax,.82f),0));
            var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,point)};
            ExecuteEvents.Execute(slider.gameObject,pointer,ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(slider.gameObject,pointer,ExecuteEvents.dragHandler);
            ExecuteEvents.Execute(slider.gameObject,pointer,ExecuteEvents.pointerUpHandler);
            yield return null;
            Assert.That(GamePreferences.Current.musicVolume,Is.EqualTo(.82f));
            Assert.That(dialogs.transform.Find(Body+"music/Volume").GetComponent<Slider>(),Is.SameAs(slider));
            Assert.That(dialogs.transform.Find(Body+"music/Value").GetComponent<TMP_Text>().text,Is.EqualTo("82%"));
            mute.isOn=true;yield return null;
            Assert.That(GamePreferences.Current.musicMuted,Is.True);Assert.That(slider.interactable,Is.False);
            Assert.That(GamePreferences.Current.musicVolume,Is.EqualTo(.82f));
            mute.isOn=false;yield return null;
            Assert.That(slider.interactable,Is.True);Assert.That(slider.value,Is.EqualTo(82));
            Assert.That(GamePreferences.Current.effectsVolume,Is.EqualTo(effects));
            dialogs.transform.Find(Footer+"close").GetComponent<Button>().onClick.Invoke();
            Assert.That(Time.timeScale,Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator LanguageAndEffectControlsKeepNestedSettingsPaused()
        {
            yield return SceneManager.LoadSceneAsync("Game");yield return null;
            var dialogs=Object.FindFirstObjectByType<PlayerDialogs>();var settings=dialogs.GetComponent<SettingsPopup>();
            settings.Open();dialogs.Preferences();
            dialogs.transform.Find(Body+"Tabs/experienceTab").GetComponent<Button>().onClick.Invoke();yield return null;
            dialogs.transform.Find(Body+"Language/Choices/en").GetComponent<Button>().onClick.Invoke();yield return null;
            Assert.That(GamePreferences.Current.language,Is.EqualTo("en"));
            Assert.That(dialogs.transform.Find(Body+"Tabs/audioTab/Label").GetComponent<TMP_Text>().text,Is.EqualTo("Audio"));
            Assert.That(dialogs.transform.Find(Body+"ReducedEffects/Hint").GetComponent<TMP_Text>().text,Does.StartWith("Reduce sparkles"));
            var reduced=dialogs.transform.Find(Body+"ReducedEffects/Switch").GetComponent<Toggle>();reduced.isOn=true;yield return null;
            Assert.That(GamePreferences.Current.reducedEffects,Is.True);Assert.That(Time.timeScale,Is.Zero);
            dialogs.Close();Assert.That(settings.IsVisible,Is.False);Assert.That(Time.timeScale,Is.EqualTo(1));
            settings.Close();Assert.That(Time.timeScale,Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator PreviewUsesActualGoalsAndFixedActionsKeepTheirBehavior()
        {
            yield return SceneManager.LoadSceneAsync("WorldMap");yield return null;
            var dialogs=Object.FindFirstObjectByType<PlayerDialogs>();var campaign=CampaignState.Instance;
            var level=campaign.Catalog.Get(50);var rules=level.CreateRules();var progress=new CampaignProgress();
            for(int n=1;n<50;n++)progress.RecordWin(n,1000);
            progress.bestScores[49]=12345;bool started=false;dialogs.Preview(level,progress,()=>started=true);
            yield return null;Canvas.ForceUpdateCanvases();yield return null;
            Assert.That(dialogs.transform.Find(Body+"Overview/Moves/Value").GetComponent<TMP_Text>().text,Is.EqualTo(rules.StartingMoves.ToString()));
            Assert.That(dialogs.transform.Find(Body+"Overview/Best/Value").GetComponent<TMP_Text>().text,Is.EqualTo("12,345"));
            Assert.That(dialogs.transform.Find(Body+"Goal_Score/Amount").GetComponent<TMP_Text>().text,Is.EqualTo(Localization.Get("scoreAmount",rules.TargetScore)));
            Assert.That(dialogs.transform.Find(Body+"Goal_Frost/Amount").GetComponent<TMP_Text>().text,Is.EqualTo(Localization.Get("tileAmount",rules.FrostTarget)));
            string[] names={"red","orange","yellow","green","blue","purple"};
            for(int i=0;i<names.Length;i++)
            {
                int target=rules.CollectionTarget((PieceColor)(i+1));if(target==0)continue;
                var row=dialogs.transform.Find(Body+"Goal_"+names[i]);
                Assert.That(row.Find("Amount").GetComponent<TMP_Text>().text,Is.EqualTo(Localization.Get("candyAmount",target)));
                Assert.That(row.Find("Icon").GetComponent<Image>().sprite,Is.Not.Null);
            }
            var scroll=dialogs.transform.Find("PlayerDialog/Card").GetComponent<ScrollRect>();
            var start=dialogs.transform.Find(Footer+"start").GetComponent<Button>();var before=start.transform.position;
            scroll.verticalNormalizedPosition=0;Canvas.ForceUpdateCanvases();yield return null;
            Assert.That(start.transform.position,Is.EqualTo(before));Assert.That(start.IsInteractable(),Is.True);
            dialogs.transform.Find(Footer+"cancel").GetComponent<Button>().onClick.Invoke();
            Assert.That(started,Is.False);Assert.That(dialogs.IsVisible,Is.False);
            dialogs.Preview(level,progress,()=>started=true);yield return null;
            dialogs.transform.Find(Footer+"start").GetComponent<Button>().onClick.Invoke();
            Assert.That(started,Is.True);Assert.That(dialogs.IsVisible,Is.False);Assert.That(Time.timeScale,Is.EqualTo(1));
        }
    }
}
