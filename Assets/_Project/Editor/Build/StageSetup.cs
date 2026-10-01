using System;
using System.IO;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace PuzzleGame.Editor
{
    public static class StageSetup
    {
        private const string Root = "Assets/_Project";
        private static readonly Color Ink = new Color32(227, 238, 251, 255);
        private static readonly Color Muted = new Color32(150, 174, 195, 255);
        private static readonly Color Mint = new Color32(119, 237, 190, 255);
        private static TMP_FontAsset font;

        [MenuItem("Puzzle Game/Prepare Practice Level")]
        public static void PreparePracticeLevel()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(BuildCommands.GameScene);
            if (UnityEngine.Object.FindFirstObjectByType<LevelSession>())
            { Debug.Log("Practice level already exists."); return; }
            EnsureFolder(Root + "/Data/Levels/Definitions");
            EnsureFolder(Root + "/UI/Fonts");
            font = CreateFont();
            var definition = ScriptableObject.CreateInstance<LevelDefinition>();
            AssetDatabase.CreateAsset(definition, Root + "/Data/Levels/Definitions/Level_001.asset");
            var board = UnityEngine.Object.FindFirstObjectByType<BoardController>();
            var session = board.gameObject.AddComponent<LevelSession>();
            session.Configure(board, definition);
            board.ConfigureSession(session);

            var canvasObject = new GameObject("LevelCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 800);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1;
            RectTransform root = (RectTransform)canvasObject.transform;

            RectTransform header = Rect("Header", root, .08f, .80f, .92f, .98f);
            TMP_Text title = Text("LevelName", header, 0, .76f, .7f, 1, "연습 스테이지", 26, Ink, TextAlignmentOptions.Left);
            Text("LevelId", header, .75f, .76f, 1, 1, "STAGE 01", 16, Muted, TextAlignmentOptions.Right);
            TMP_Text score = Stat(header, "Score", 0, .31f, "현재 점수", "0", Mint);
            TMP_Text target = Stat(header, "Target", .345f, .655f, "목표 점수", "1000", Ink);
            TMP_Text moves = Stat(header, "Moves", .69f, 1, "남은 이동", "20", Ink);
            RectTransform progress = Panel("ProgressTrack", header, 0, .05f, 1, .085f, new Color32(36, 54, 73, 255));
            RectTransform fill = Panel("ProgressFill", progress, 0, 0, 0, 1, Mint);
            RectTransform footer = Rect("Footer", root, .08f, .01f, .92f, .10f);
            TMP_Text status = Text("Instructions", footer, 0, 0, .75f, 1, "블록을 누른 채 옆으로 끌어 놓으세요", 18, Muted, TextAlignmentOptions.Left);
            status.textWrappingMode = TextWrappingModes.Normal;
            Button restart = Button("Restart", footer, .79f, .12f, 1, .88f, "다시 하기", new Color32(36, 54, 73, 255), Ink);
            canvasObject.AddComponent<HUDView>().Configure(session, title, score, target, moves, status, fill, restart);

            RectTransform overlay = Panel("ResultOverlay", root, 0, 0, 1, 1, new Color(0.015f, .025f, .04f, .88f));
            RectTransform card = Panel("ResultCard", overlay, .5f, .5f, .5f, .5f, new Color32(27, 43, 61, 255));
            card.sizeDelta = new Vector2(520, 340);
            TMP_Text resultTitle = Text("ResultTitle", card, .08f, .72f, .92f, .9f, "스테이지 성공!", 36, Mint);
            TMP_Text detail = Text("ResultScore", card, .08f, .52f, .92f, .66f, "획득 1000점  /  목표 1000점", 23, Ink);
            TMP_Text message = Text("ResultMessage", card, .08f, .3f, .92f, .49f, "목표 점수를 달성했어요!\n남은 이동 4회", 20, Muted);
            message.textWrappingMode = TextWrappingModes.Normal;
            Button again = Button("PlayAgain", card, .18f, .08f, .82f, .23f, "다시 하기", Mint, new Color32(17, 39, 36, 255));
            canvasObject.AddComponent<ResultPopup>().Configure(session, overlay.gameObject, resultTitle, detail, message, again);
            overlay.gameObject.SetActive(false);

            var events = new GameObject("EventSystem", typeof(EventSystem));
            var module = events.AddComponent<InputSystemUIInputModule>();
            const string actionPath = "Assets/Settings/InputSystem_Actions.inputactions";
            module.actionsAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(actionPath);
            module.point = UIAction(actionPath, "Point");
            module.leftClick = UIAction(actionPath, "Click");
            module.rightClick = UIAction(actionPath, "RightClick");
            module.middleClick = UIAction(actionPath, "MiddleClick");
            module.scrollWheel = UIAction(actionPath, "ScrollWheel");
            module.move = UIAction(actionPath, "Navigate");
            module.submit = UIAction(actionPath, "Submit");
            module.cancel = UIAction(actionPath, "Cancel");
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Practice level prepared: 20 moves, target 1000, 10 points per piece.");
        }

        private static InputActionReference UIAction(string path, string name)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                if (asset is InputActionReference reference && reference.action.actionMap.name == "UI" && reference.action.name == name)
                    return reference;
            throw new InvalidOperationException("Missing UI action: " + name);
        }

        private static TMP_FontAsset CreateFont()
        {
            string path = Root + "/UI/Fonts/PuzzleUI.asset";
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (existing) return existing;
            Font source = AssetDatabase.LoadAssetAtPath<Font>(Root + "/UI/Fonts/NanumGothic/NanumGothic-Regular.ttf");
            if (!source) throw new InvalidOperationException("Import the Korean font first.");
            var created = TMP_FontAsset.CreateFontAsset(source, 64, 7, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
            created.name = "PuzzleUI";
            created.material.name = "PuzzleUI Material";
            AssetDatabase.CreateAsset(created, path);
            AssetDatabase.AddObjectToAsset(created.material, created);
            foreach (var texture in created.atlasTextures) AssetDatabase.AddObjectToAsset(texture, created);
            string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz ,./:!?-"
                + "연습 스테이지 현재 점수 목표 남은 이동 다시 하기 블록을 누른 채 옆으로 끌어 놓으세요"
                + "정리하는 중 아래 결과를 확인하세요 성공 실패 획득 달성했어요 횟수 모두 사용 도전해 보세요 회";
            if (!created.TryAddCharacters(characters, out string missing))
                throw new InvalidOperationException("Font is missing characters: " + missing);
            EditorUtility.SetDirty(created);
            AssetDatabase.SaveAssets();
            return created;
        }

        private static RectTransform Rect(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(x0, y0); rect.anchorMax = new Vector2(x1, y1);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static RectTransform Panel(string name, Transform parent, float x0, float y0, float x1, float y1, Color color)
        {
            RectTransform rect = Rect(name, parent, x0, y0, x1, y1);
            rect.gameObject.AddComponent<Image>().color = color;
            return rect;
        }

        private static TMP_Text Text(string name, Transform parent, float x0, float y0, float x1, float y1,
            string content, float size, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            RectTransform rect = Rect(name, parent, x0, y0, x1, y1);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.text = content;
            text.fontSize = size;
            text.enableAutoSizing = true; text.fontSizeMin = 14; text.fontSizeMax = size;
            text.alignment = alignment; text.color = color;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            return text;
        }

        private static TMP_Text Stat(Transform parent, string name, float x0, float x1, string label, string value, Color color)
        {
            RectTransform card = Panel(name, parent, x0, .22f, x1, .70f, new Color32(27, 43, 61, 255));
            Text(name + "Label", card, .06f, .58f, .94f, .95f, label, 16, Muted, TextAlignmentOptions.Left);
            return Text(name + "Value", card, .06f, .05f, .94f, .59f, value, 29, color, TextAlignmentOptions.Left);
        }

        private static Button Button(string name, Transform parent, float x0, float y0, float x1, float y1,
            string label, Color background, Color foreground)
        {
            RectTransform rect = Panel(name, parent, x0, y0, x1, y1, background);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            Text(name + "Label", rect, .05f, 0, .95f, 1, label, 21, foreground);
            return button;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            EnsureFolder(path.Substring(0, slash));
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }
    }
}
