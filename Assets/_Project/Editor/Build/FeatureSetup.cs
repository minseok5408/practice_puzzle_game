using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleGame.Core.Board;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.Services;
using PuzzleGame.Runtime.Startup;
using PuzzleGame.Runtime.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace PuzzleGame.Editor
{
    public static class FeatureSetup
    {
        private const string Root="Assets/_Project";
        private static TMP_FontAsset font;
        private static Sprite rounded;
        public static void ApplyAndValidate() { Apply(); CampaignValidation.Run(); }
        [MenuItem("Puzzle Game/Prepare Gameplay Expansion")]
        public static void Apply()
        {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root+"/UI/Fonts/CandyBody.asset");
            rounded=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/UI/Theme/Rounded.png");
            string characters=string.Join("",Localization.Entries.Values.SelectMany(v=>v))+"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789★%[]";
            if(!font.TryAddCharacters(characters,out string missing))throw new InvalidOperationException("Missing glyphs: "+missing);
            foreach(var texture in font.atlasTextures)if(!AssetDatabase.Contains(texture))AssetDatabase.AddObjectToAsset(texture,font);
            EditorUtility.SetDirty(font);
            var catalog=AssetDatabase.LoadAssetAtPath<LevelCatalog>(CampaignSetup.CatalogPath);
            Balance(catalog);
            foreach(string path in new[]{CampaignSetup.MapScene,BuildCommands.GameScene})
            {
                var scene=EditorSceneManager.OpenScene(path);
                var canvas=Object.FindFirstObjectByType<SettingsPopup>().gameObject;
                var dialogs=canvas.GetComponent<PlayerDialogs>()??canvas.AddComponent<PlayerDialogs>();dialogs.Configure(font,rounded);
                dialogs.ConfigureArtwork(AssetDatabase.LoadAssetAtPath<PuzzleGame.Runtime.Config.PieceCatalog>(Root+"/Data/Pieces/PieceCatalog.asset"));
                dialogs.ConfigureFrostIcon(AssetDatabase.LoadAllAssetsAtPath(Root+"/Art/Sprites/Frost/IceIntact.png").OfType<Sprite>().FirstOrDefault());
                if(path==BuildCommands.GameScene)InstallGame(canvas);
                LocalizeScene();
                EditorSceneManager.SaveScene(scene);
            }
            var boot=EditorSceneManager.OpenScene(LoadingSetup.BootScene);
            var screen=Object.FindFirstObjectByType<StartupLoadingScreen>();
            var loader=screen.GetComponent<LocalStartupLoader>()??screen.gameObject.AddComponent<LocalStartupLoader>();loader.Configure(catalog);
            screen.Configure(loader,screen.View);
            var prototype=screen.GetComponent<PrototypeStartupLoader>();if(prototype)Object.DestroyImmediate(prototype);
            LocalizeScene();EditorSceneManager.SaveScene(boot);
            PlayerSettings.bundleVersion="0.10.0";
            AssetDatabase.SaveAssets();
            Debug.Log("GAMEPLAY EXPANSION READY: Frost, guidance, balance, audio, dialogs, local loading, localization and reduced effects.");
        }
        private static void Balance(LevelCatalog catalog)
        {
            for(int number=1;number<=50;number++)
            {
                var level=catalog.Get(number);int world=level.World,stage=level.Stage;
                // First 1,600 automated runs left 9-12 moves in later worlds.
                // Leave less slack as collection and Frost rules become familiar.
                int moves=number<=4?20:new[]{26,24,28,27,29}[world-1];
                if(number==46 || number==49)moves+=2; // Paired item/no-item audit, 2026-10-02.
                int score=number<=4?new[]{500,600,750,850}[number-1]:850+(number-5)*17;
                var targets=new int[6];int colors=number<=1?0:world<=2?1:world<=4?2:3;
                for(int c=0;c<colors;c++)targets[(stage+world+c*2)%6]=number<=5?5:8+world*2+stage/3;
                level.ConfigureCampaign(number,level.Seed,moves,score,targets);
                var frost=new List<LevelDefinition.FrostCell>();
                var initial=BoardGenerator.Generate(8,8,6,level.Seed);
                int count=number<5?0:number==5?3:4+world*2+stage/3;
                if(number==5)
                {
                    MoveFinder.TryFindMove(initial,out var a,out var b);
                    initial.SwapPieces(a,b);var first=new BoardResolver(initial,6,new System.Random(0)).ResolveMatches(initial,a,b);
                    // Place the first ice lesson under the first three actual removed cells.
                    var original=BoardGenerator.Generate(8,8,6,level.Seed);original.SwapPieces(a,b);
                    for(int y=0;y<8 && frost.Count<count;y++)for(int x=0;x<8 && frost.Count<count;x++)
                    {var p=new GridPosition(x,y);if(first.RemovedIds.Contains(original.GetPiece(p).Id))frost.Add(new LevelDefinition.FrostCell(x,y,1));}
                }
                else for(int i=0;i<count;i++)
                {
                    int slot=(number*7+i*13)%64;
                    frost.Add(new LevelDefinition.FrostCell(slot%8,slot/8,world>=3 && i%3==0?2:1));
                }
                PieceColor[] fixedColors=null;
                if(number<=5)
                {
                    var board=BoardGenerator.Generate(8,8,6,level.Seed);fixedColors=new PieceColor[64];
                    for(int y=0;y<8;y++)for(int x=0;x<8;x++)fixedColors[y*8+x]=board.GetPiece(new GridPosition(x,y)).Color;
                }
                level.ConfigureLayout(frost.ToArray(),number==5?count:Mathf.CeilToInt(count*.7f),fixedColors);
                level.CreateBoard();EditorUtility.SetDirty(level);
            }
        }
        private static void InstallGame(GameObject canvas)
        {
            Transform sidebar=canvas.transform.Find("ScoreCard");
            canvas.GetComponent<CampaignHUD>()?.ConfigureGoalCheck(AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/UI/Theme/SettingsCheck.png"));
            canvas.GetComponent<CampaignHUD>()?.ConfigureGoalIce(AssetDatabase.LoadAllAssetsAtPath(Root+"/Art/Sprites/Frost/IceIntact.png").OfType<Sprite>().FirstOrDefault());
            var old=sidebar.Find("FrostProgress");if(old)Object.DestroyImmediate(old.gameObject);
            var frost=Text("FrostProgress",sidebar,.08f,.105f,.92f,.17f,"얼음 0 / 0",21);
            // The unfinished item placeholders give their space to a playable goal.
            sidebar.Find("ItemSlots")?.gameObject.SetActive(false);
            old=canvas.transform.Find("TutorialBanner");if(old)Object.DestroyImmediate(old.gameObject);
            var banner=Rect("TutorialBanner",canvas.transform,.34f,.015f,.97f,.14f);
            var bg=banner.gameObject.AddComponent<Image>();bg.sprite=rounded;bg.type=Image.Type.Sliced;bg.color=new Color32(255,248,231,248);bg.raycastTarget=false;
            var tutorial=Text("Tutorial",banner,.025f,.08f,.90f,.92f,"",21);
            var close=Rect("Dismiss",banner,.91f,.25f,.98f,.75f);var closeImage=close.gameObject.AddComponent<Image>();closeImage.color=new Color32(137,78,155,255);
            var button=close.gameObject.AddComponent<Button>();button.targetGraphic=closeImage;Text("Label",close,0,0,1,1,"X",22).color=Color.white;
            var board=Object.FindFirstObjectByType<BoardController>();
            var guidance=board.GetComponent<BoardGuidance>()??board.gameObject.AddComponent<BoardGuidance>();guidance.Configure(tutorial,frost,button);
            var result=canvas.transform.Find("ResultOverlay/ResultCard");
            old=canvas.transform.Find("ComboMessage");if(old)Object.DestroyImmediate(old.gameObject);
            var combo=Text("ComboMessage",canvas.transform,.35f,.88f,.97f,.95f,"",28);combo.color=new Color32(99,36,111,255);combo.gameObject.SetActive(false);
            old=result.Find("CompletionStars");if(old)Object.DestroyImmediate(old.gameObject);
            var stars=Text("CompletionStars",result,.10f,.89f,.90f,.96f,"★★★★★",30);stars.color=new Color32(189,113,15,255);stars.gameObject.SetActive(false);
            var celebration=canvas.GetComponent<GameplayCelebration>()??canvas.AddComponent<GameplayCelebration>();
            celebration.Configure(board,board.Session,combo,stars);
            old=result.Find("Feedback");if(old)Object.DestroyImmediate(old.gameObject);
            // Failure feedback shares the action row with the success-only NextStage button.
            // Keep it below ResultMessage so it cannot cover the score or failure explanation.
            var feedback=Rect("Feedback",result,.06f,.18f,.94f,.27f);var fi=feedback.gameObject.AddComponent<Image>();fi.color=new Color32(137,78,155,255);
            var feedbackButton=feedback.gameObject.AddComponent<Button>();feedbackButton.targetGraphic=fi;
            Text("Label",feedback,0,0,1,1,Localization.Get("feedback"),18).color=Color.white;
            UnityEditor.Events.UnityEventTools.AddPersistentListener(feedbackButton.onClick,canvas.GetComponent<PlayerDialogs>().Feedback);
            // Use the full body font for the new Korean/English completion messages.
            foreach(var text in result.GetComponentsInChildren<TMP_Text>(true))text.font=font;
            VisualPolishSetup.ConfigureResults(canvas.GetComponent<ResultPopup>());
        }
        private static void LocalizeScene()
        {
            foreach(var text in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                foreach(var entry in Localization.Entries)
                {
                    if(text.text!=entry.Value[0] || entry.Value[0].Contains("{"))continue;
                    // Dynamic result/status values are refreshed by their owning component.
                    if(text.name=="Status" || text.name=="ResultTitle" || text.name=="ResultMessage")break;
                    var localized=text.GetComponent<LocalizedText>()??text.gameObject.AddComponent<LocalizedText>();localized.Bind(entry.Key);break;
                }
            }
        }
        private static RectTransform Rect(string name,Transform parent,float x0,float y0,float x1,float y1)
        {var r=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;r.SetParent(parent,false);r.anchorMin=new Vector2(x0,y0);r.anchorMax=new Vector2(x1,y1);r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        private static TMP_Text Text(string name,Transform parent,float x0,float y0,float x1,float y1,string value,float size)
        {
            var text=Rect(name,parent,x0,y0,x1,y1).gameObject.AddComponent<TextMeshProUGUI>();text.font=font;text.text=value;text.fontSize=size;
            text.enableAutoSizing=true;text.fontSizeMin=14;text.fontSizeMax=size;text.color=new Color32(92,48,104,255);text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;return text;
        }
    }
}
