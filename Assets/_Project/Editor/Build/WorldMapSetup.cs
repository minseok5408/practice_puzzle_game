using System;
using PuzzleGame.Runtime.Config;
using PuzzleGame.Core.Board;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.UI;
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
    public static class WorldMapSetup
    {
        private const string Root = "Assets/_Project";
        private static TMP_FontAsset font, numberFont;
        private static Sprite rounded, circle, star;
        private static readonly Color Cream = new Color32(255, 248, 232, 248), Ink = new Color32(106, 61, 99, 255);

        [MenuItem("Puzzle Game/Rebuild Illustrated World Map")]
        public static void Apply()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            AssetDatabase.Refresh();
            Create(AssetDatabase.LoadAssetAtPath<LevelCatalog>(CampaignSetup.CatalogPath));
            AssetDatabase.SaveAssets();
            Debug.Log("ILLUSTRATED MAP READY: five dioramas, compact jewel buttons, current-stage marker and world navigation.");
        }

        public static void Create(LevelCatalog catalog)
        {
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root + "/UI/Fonts/CandyBody.asset");
            numberFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root + "/UI/Fonts/CandyTitle.asset");
            if (!numberFont.TryAddCharacters("0123456789", out string missingNumbers)) throw new InvalidOperationException("Missing number glyphs: " + missingNumbers);
            foreach (var texture in numberFont.atlasTextures) if (!AssetDatabase.Contains(texture)) AssetDatabase.AddObjectToAsset(texture, numberFont);
            EditorUtility.SetDirty(numberFont);
            rounded = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/UI/Theme/Rounded.png");
            circle = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/UI/Theme/Circle.png");
            star = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/UI/Theme/Star.png");
            if (!font.TryAddCharacters("WORLD SELECT 0123456789-/ 이 월드 전체 도전하기 현재 위치 사탕 정원 오렌지 과수원 얼음 소다 포도 밤정원 무지개 궁전 완료 다시 하기", out string missing))
                throw new InvalidOperationException("Missing map glyphs: " + missing);
            foreach (var texture in font.atlasTextures) if (!AssetDatabase.Contains(texture)) AssetDatabase.AddObjectToAsset(texture, font);
            EditorUtility.SetDirty(font);
            string[] files = { "SugarGardenMap", "OrangeOrchardMap", "IceSodaMap", "GrapeNightGardenMap", "RainbowPalaceMap" };
            var art = new Sprite[5];
            for (int i = 0; i < 5; i++) art[i] = Import(files[i], 2048);
            var available = Import("LevelMedallion", 256); var locked = Import("LevelMedallionLocked", 256); var complete = Import("LevelMedallionComplete", 256);
            catalog.ConfigureMapArt(art); EditorUtility.SetDirty(catalog);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("MapCamera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera"; camera.orthographic = true;
            camera.gameObject.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = false;
            var obj = new GameObject("WorldMapCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            obj.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 800); scaler.matchWidthOrHeight = .5f;
            var root = (RectTransform)obj.transform;
            var outer = Panel("SurroundingScenery", root, 0, 0, 1, 1, Color.white, art[0]);
            var fit = outer.gameObject.AddComponent<AspectRatioFitter>(); fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent; fit.aspectRatio = 1.6f;
            Panel("SurroundingShade", root, 0, 0, 1, 1, new Color(.25f, .13f, .3f, .32f), null);
            var map = Rect("Map", root, .5f, .5f, .5f, .5f); map.sizeDelta = new Vector2(1280, 800);
            var bg = Panel("IllustratedKingdom", map, 0, 0, 1, 1, Color.white, art[0]);

            var particles = new RectTransform[14];
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i] = Panel("SugarSparkle" + i, map, 0, 0, 0, 0, new Color(1, 1, .86f, .8f), star);
                particles[i].sizeDelta = new Vector2(9 + i % 3 * 3, 9 + i % 3 * 3);
                particles[i].anchoredPosition = new Vector2(70 + i * 89 % 1160, 160 + i * 137 % 470);
            }
            var halo = Panel("CurrentHalo", map, 0, 0, 0, 0, new Color(1, .82f, .45f, .3f), circle); halo.sizeDelta = new Vector2(62, 62);
            var positions = StagePositions();
            var stages = new Button[LevelCatalog.StagesPerWorld]; var numbers = new TMP_Text[LevelCatalog.StagesPerWorld]; var states = new TMP_Text[LevelCatalog.StagesPerWorld];
            for (int i = 0; i < stages.Length; i++)
            {
                var node = Panel("Stage" + (i + 1), map, 0, 0, 0, 0, Color.white, available);
                node.anchoredPosition = positions[i]; node.sizeDelta = new Vector2(64, 64);
                var img = node.GetComponent<Image>(); img.preserveAspect = true; img.raycastTarget = true;
                // Keep the original 106 x 76 click area around the smaller artwork.
                img.raycastPadding = new Vector4(-21, -6, -21, -6);
                Shadow(node, 2);
                stages[i] = node.gameObject.AddComponent<Button>(); stages[i].targetGraphic = img;
                var colors = stages[i].colors; colors.disabledColor = Color.white; colors.highlightedColor = new Color(1, .94f, .82f); stages[i].colors = colors;
                // Center the visible digits on the upper jewel face, excluding the lower rim.
                numbers[i] = Text("Number", node, .24f, .30f, .76f, .80f, (i + 1).ToString(), 22);
                numbers[i].font = numberFont; numbers[i].color = new Color32(255, 252, 234, 255);
                numbers[i].alignment = TextAlignmentOptions.MidlineGeoAligned;
                var letterShadow = numbers[i].gameObject.AddComponent<Shadow>(); letterShadow.effectColor = new Color(.38f,.13f,.29f,.45f); letterShadow.effectDistance = new Vector2(0,-1);
                states[i] = Text("Completed", node, .1f, .12f, .9f, .31f, "", 11); states[i].color = Color.white;
            }
            var marker = Panel("CurrentPosition", map, 0, 0, 0, 0, Cream, rounded); marker.sizeDelta = new Vector2(68, 23);
            var pointer = Panel("Pointer", marker, .44f, -.12f, .56f, .2f, Cream, rounded); pointer.localEulerAngles = new Vector3(0, 0, 45);
            Text("Label", marker, .04f, 0, .96f, 1, "현재 위치", 12);
            var heading = Panel("WorldPlaque", map, .025f, .815f, .315f, .975f, Cream, rounded); Shadow(heading, 5);
            var tag = Text("WorldTag", heading, .08f, .72f, .93f, .95f, "WORLD 01", 16); tag.alignment = TextAlignmentOptions.Left; tag.color = new Color32(193, 122, 71, 255);
            var title = Text("Title", heading, .07f, .27f, .96f, .78f, "사탕 정원", 38); title.alignment = TextAlignmentOptions.Left;
            var progress = Text("Progress", heading, .08f, .06f, .94f, .30f, "이 월드 0 / 10    전체 0 / 50", 16); progress.alignment = TextAlignmentOptions.Left;
            var toolbar = Rect("MapToolbar", map, .885f, .91f, .975f, .974f);
            Panel("WorldDock", map, .023f, .018f, .688f, .114f, Cream, rounded);
            string[] shortNames = { "사탕 정원", "과수원", "얼음 소다", "밤정원", "무지개 궁전" };
            var tabs = new Button[5];
            var pieces = AssetDatabase.LoadAssetAtPath<PieceCatalog>(Root + "/Data/Pieces/PieceCatalog.asset");
            int[] candy = { 1, 2, 5, 6, 3 };
            for (int i = 0; i < 5; i++)
            {
                tabs[i] = Button("World" + (i + 1), map, .034f + i * .129f, .032f, .155f + i * .129f, .099f, (i + 1) + " " + shortNames[i], new Color32(131, 100, 146, 245), 17);
                Place((RectTransform)tabs[i].transform.Find("Label"), .24f, 0, .99f, 1);
                var icon = Panel("Candy", tabs[i].transform, .025f, .20f, .23f, .8f, Color.white, pieces.Get((PieceColor)candy[i]).sprite);
                icon.GetComponent<Image>().preserveAspect = true;
            }
            var resume = Button("Continue", map, .725f, .027f, .975f, .109f, "1-1 도전하기", new Color32(218, 61, 135, 255), 27); Shadow((RectTransform)resume.transform, 5);
            obj.AddComponent<WorldMapView>().Configure(catalog, bg.GetComponent<Image>(), outer.GetComponent<Image>(), map,
                title, progress, tag, tabs, stages, numbers, states, resume, available, locked, complete, marker, halo, particles);
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule)).GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            SettingsSetup.Install(obj, toolbar);
            EditorSceneManager.SaveScene(scene, CampaignSetup.MapScene);
        }

        // Pixel coordinates follow the shared illustrated road; sample equal distances along it.
        private static Vector2[] StagePositions()
        {
            var road = new[] { new Vector2(.105f,.758f), new Vector2(.160f,.783f), new Vector2(.218f,.784f), new Vector2(.263f,.755f),
                new Vector2(.280f,.709f), new Vector2(.274f,.656f), new Vector2(.294f,.614f), new Vector2(.343f,.589f),
                new Vector2(.413f,.582f), new Vector2(.485f,.575f), new Vector2(.552f,.553f), new Vector2(.604f,.528f),
                new Vector2(.620f,.493f), new Vector2(.605f,.460f), new Vector2(.560f,.431f), new Vector2(.495f,.409f),
                new Vector2(.444f,.388f), new Vector2(.422f,.358f), new Vector2(.425f,.330f), new Vector2(.452f,.306f),
                new Vector2(.495f,.288f), new Vector2(.551f,.282f), new Vector2(.605f,.273f), new Vector2(.638f,.254f),
                new Vector2(.642f,.229f), new Vector2(.630f,.205f) };
            for (int i = 0; i < road.Length; i++) road[i] = new Vector2(road[i].x * 1280, (1 - road[i].y) * 800);
            float length = 0; for (int i = 1; i < road.Length; i++) length += Vector2.Distance(road[i - 1], road[i]);
            var result = new Vector2[LevelCatalog.StagesPerWorld];
            for (int n = 0; n < result.Length; n++)
            {
                float remaining = length * n / (result.Length - 1);
                for (int i = 1; i < road.Length; i++)
                {
                    float segment = Vector2.Distance(road[i - 1], road[i]);
                    if (remaining <= segment || i == road.Length - 1) { result[n] = Vector2.Lerp(road[i - 1], road[i], remaining / segment); break; }
                    remaining -= segment;
                }
            }
            return result;
        }
        private static Sprite Import(string name, int maxSize)
        {
            string path = Root + "/Art/WorldMaps/" + name + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (!importer) throw new InvalidOperationException("Missing map artwork: " + path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false; importer.alphaIsTransparency = true; importer.maxTextureSize = maxSize;
            importer.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        private static void Place(RectTransform rect, float x0, float y0, float x1, float y1) { rect.anchorMin = new Vector2(x0,y0); rect.anchorMax = new Vector2(x1,y1); rect.offsetMin = rect.offsetMax = Vector2.zero; }
        private static RectTransform Rect(string name, Transform parent, float x0, float y0, float x1, float y1) { var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent,false); Place(r,x0,y0,x1,y1); return r; }
        private static RectTransform Panel(string name, Transform parent, float x0, float y0, float x1, float y1, Color tint, Sprite sprite)
        { var r = Rect(name,parent,x0,y0,x1,y1); var img=r.gameObject.AddComponent<Image>(); img.sprite=sprite; img.color=tint; img.type=sprite==rounded?Image.Type.Sliced:Image.Type.Simple; img.raycastTarget=false; return r; }
        private static TMP_Text Text(string name, Transform parent, float x0, float y0, float x1, float y1, string value, float size)
        { var t=Rect(name,parent,x0,y0,x1,y1).gameObject.AddComponent<TextMeshProUGUI>(); t.font=font; t.text=value; t.fontSize=size; t.color=Ink; t.alignment=TextAlignmentOptions.Center; t.enableAutoSizing=true; t.fontSizeMin=Mathf.Min(10,size); t.fontSizeMax=size; t.raycastTarget=false; return t; }
        private static Button Button(string name, Transform parent, float x0, float y0, float x1, float y1, string caption, Color tint, float size)
        { var r=Panel(name,parent,x0,y0,x1,y1,tint,rounded); var img=r.GetComponent<Image>(); img.raycastTarget=true; var b=r.gameObject.AddComponent<Button>(); b.targetGraphic=img; Text("Label",r,.03f,0,.97f,1,caption,size).color=Color.white; return b; }
        private static void Shadow(RectTransform rect, float offset) { var s=rect.gameObject.AddComponent<Shadow>(); s.effectColor=new Color(.28f,.09f,.25f,.22f); s.effectDistance=new Vector2(0,-offset); }
    }
}
