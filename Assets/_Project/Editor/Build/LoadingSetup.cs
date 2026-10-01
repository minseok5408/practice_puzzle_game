using System;
using System.IO;
using PuzzleGame.Runtime.Startup;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace PuzzleGame.Editor
{
    public static class LoadingSetup
    {
        public const string BootScene = "Assets/_Project/Scenes/Boot.unity";
        private const string Root = "Assets/_Project";
        private static readonly Color Cream = new Color32(255, 244, 224, 255);
        private static readonly Color Ink = new Color32(130, 66, 124, 255);
        private static Sprite rounded;
        private static TMP_FontAsset font;

        [MenuItem("Puzzle Game/Prepare Loading Screen")]
        public static void Apply()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            AssetDatabase.Refresh();
            foreach (string name in new[] { "a", "b", "c", "d", "a_background" })
                Import(Root + "/Art/Loading/loading_screen_" + name + ".png");
            rounded = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/UI/Theme/Rounded.png");
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root + "/UI/Fonts/CandyBody.asset");
            if (!font.TryAddCharacters("로딩 중...불러오지 못했어요. 다시 시도해 주세요.다시 시도게임 종료", out string missing))
                throw new InvalidOperationException("Missing loading glyphs: " + missing);
            foreach (var texture in font.atlasTextures)
                if (!AssetDatabase.Contains(texture)) AssetDatabase.AddObjectToAsset(texture, font);
            EditorUtility.SetDirty(font);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("LoadingCamera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera"; camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color32(247, 181, 204, 255);
            camera.gameObject.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = false;
            var canvasObject = new GameObject("LoadingCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = .5f;
            var canvasRoot = (RectTransform)canvasObject.transform;
            Panel("Backdrop", canvasRoot, 0, 0, 1, 1, new Color32(247, 181, 204, 255), null);
            RectTransform artwork = Rect("Composition", canvasRoot, .5f, .5f, .5f, .5f);
            artwork.sizeDelta = new Vector2(1280, 720);
            var picture = Panel("Background", artwork, 0, 0, 1, 1, Color.white,
                AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Art/Loading/loading_screen_a_background.png"));
            picture.GetComponent<Image>().type = Image.Type.Simple;

            var frame = Panel("ProgressFrame", artwork, .265f, .21f, .735f, .303f, Cream, rounded);
            var shadow = frame.gameObject.AddComponent<Shadow>(); shadow.effectColor = new Color(0.45f, .16f, .37f, .2f);
            shadow.effectDistance = new Vector2(0, -5);
            Panel("Rim", frame, .014f, .12f, .986f, .88f, new Color32(181, 129, 173, 255), rounded);
            var track = Panel("Track", frame, .02f, .17f, .98f, .83f, new Color32(207, 164, 200, 255), rounded);
            var fill = Panel("Fill", track, 0, 0, 0, 1, new Color32(239, 66, 153, 255), rounded);
            Panel("Glaze", fill, .012f, .45f, .988f, .92f, new Color32(255, 166, 216, 165), rounded);
            Panel("Shine", fill, .05f, .77f, .95f, .87f, new Color32(255, 236, 249, 160), rounded);
            var sparkle = Panel("Sparkle", track, .6f, .5f, .6f, .5f, Cream,
                AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/UI/Theme/Star.png"));
            sparkle.sizeDelta = new Vector2(26, 26);
            sparkle.GetComponent<Image>().type = Image.Type.Simple;
            TMP_Text status = Text("Status", artwork, .2f, .105f, .8f, .185f, "로딩 중...", 38);
            var recovery = Rect("Recovery", artwork, .30f, .025f, .70f, .10f);
            Button retry = Button("Retry", recovery, 0, .02f, .48f, .98f, "다시 시도", new Color32(231, 87, 160, 255));
            Button quit = Button("Quit", recovery, .52f, .02f, 1, .98f, "게임 종료", new Color32(144, 91, 163, 255));
            recovery.gameObject.SetActive(false);
            var view = canvasObject.AddComponent<LoadingScreenView>();
            view.Configure(artwork, fill, sparkle, status, recovery.gameObject, retry, quit);
            var loader = canvasObject.AddComponent<PrototypeStartupLoader>();
            canvasObject.AddComponent<StartupLoadingScreen>().Configure(loader, view);
            canvasObject.GetComponent<StartupLoadingScreen>().ConfigureDestination("WorldMap");
            view.ResetLoading(); view.ApplyLayout();
            var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            EditorSceneManager.SaveScene(scene, BootScene);
            EditorBuildSettings.scenes = new[] {
                new EditorBuildSettingsScene(BootScene, true), new EditorBuildSettingsScene(CampaignSetup.MapScene, true), new EditorBuildSettingsScene(BuildCommands.GameScene, true)
            };
            AssetDatabase.SaveAssets();
            Debug.Log("LOADING READY: approved design a, simulated 5-second provider, Boot -> WorldMap -> Game.");
        }

        private static void Import(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("Missing loading artwork", path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048; importer.SaveAndReimport();
        }

        private static RectTransform Rect(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            var result = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            result.SetParent(parent, false); result.anchorMin = new Vector2(x0, y0); result.anchorMax = new Vector2(x1, y1);
            result.offsetMin = result.offsetMax = Vector2.zero;
            return result;
        }

        private static RectTransform Panel(string name, Transform parent, float x0, float y0, float x1, float y1, Color color, Sprite sprite)
        {
            var result = Rect(name, parent, x0, y0, x1, y1);
            var image = result.gameObject.AddComponent<Image>(); image.sprite = sprite; image.color = color;
            image.type = Image.Type.Sliced; image.raycastTarget = false;
            return result;
        }

        private static TMP_Text Text(string name, Transform parent, float x0, float y0, float x1, float y1, string text, float size)
        {
            var result = Rect(name, parent, x0, y0, x1, y1).gameObject.AddComponent<TextMeshProUGUI>();
            result.font = font; result.text = text; result.fontSize = size; result.color = Ink;
            result.alignment = TextAlignmentOptions.Center; result.enableAutoSizing = true;
            result.fontSizeMin = 16; result.fontSizeMax = size; result.raycastTarget = false;
            return result;
        }

        private static Button Button(string name, Transform parent, float x0, float y0, float x1, float y1, string caption, Color color)
        {
            var panel = Panel(name, parent, x0, y0, x1, y1, color, rounded);
            var image = panel.GetComponent<Image>(); image.raycastTarget = true;
            var button = panel.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            Text("Label", panel, .04f, 0, .96f, 1, caption, 23).color = Color.white;
            return button;
        }
    }
}
