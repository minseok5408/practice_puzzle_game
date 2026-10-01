using System;
using System.IO;
using PuzzleGame.Core.Board;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Config;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.Startup;
using PuzzleGame.Runtime.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PuzzleGame.Editor
{
    public static class CampaignSetup
    {
        public const string MapScene = "Assets/_Project/Scenes/WorldMap.unity";
        public const string CatalogPath = "Assets/_Project/Data/Levels/LevelCatalog.asset";
        private const string Root = "Assets/_Project";
        private static readonly Color Ink = new Color32(103, 57, 107, 255), Cream = new Color32(255, 247, 230, 255);
        private static readonly Color Pink = new Color32(228, 83, 157, 255), Purple = new Color32(140, 95, 170, 255);
        private static TMP_FontAsset font;
        private static Sprite rounded, circle;

        [MenuItem("Puzzle Game/Prepare 50 Stage Campaign")]
        public static void Apply()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            AssetDatabase.Refresh();
            var catalog = CreateCatalog();
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root + "/UI/Fonts/CandyBody.asset");
            rounded = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/UI/Theme/Rounded.png");
            circle = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/UI/Theme/Circle.png");
            const string text = "0123456789-/ 월드 사탕 정원 오렌지 과수원 얼음 소다 포도 밤정원 무지개 궁전 맵 보기"
                + "완료 도전 잠김 클리어하면 다음 스테이지가 열려요 이어하기 마지막 스테이지 다시 하기 모든 목표를 달성했어요"
                + "100개 스테이지를 모두 클리어했어요 진행 기록을 저장하지 못했어요 로드맵";
            if (!font.TryAddCharacters(text, out string missing)) throw new InvalidOperationException("Missing campaign glyphs: " + missing);
            foreach (var texture in font.atlasTextures) if (!AssetDatabase.Contains(texture)) AssetDatabase.AddObjectToAsset(texture, font);
            EditorUtility.SetDirty(font);
            InstallGame(catalog); CreateMap(catalog);
            var boot = EditorSceneManager.OpenScene(LoadingSetup.BootScene);
            var loading = Object.FindFirstObjectByType<StartupLoadingScreen>(); loading.ConfigureDestination("WorldMap");
            EditorUtility.SetDirty(loading); EditorSceneManager.SaveScene(boot);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(LoadingSetup.BootScene, true),
                new EditorBuildSettingsScene(MapScene, true), new EditorBuildSettingsScene(BuildCommands.GameScene, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("CAMPAIGN READY: 5 worlds, 50 distinct levels, Boot -> WorldMap -> Game.");
        }

        private static LevelCatalog CreateCatalog()
        {
            string[] names = { "사탕 정원", "오렌지 과수원", "얼음 소다", "포도 밤정원", "무지개 궁전" };
            string[] files = { "SugarGarden", "OrangeOrchard", "IceSoda", "GrapeNightGarden", "RainbowPalace" };
            var art = new Sprite[5]; var materials = new Material[5];
            for (int i = 0; i < 5; i++)
            {
                string path = Root + "/Art/Backgrounds/" + files[i] + ".png";
                if (!File.Exists(path)) throw new FileNotFoundException("Missing world background", path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false; importer.maxTextureSize = 2048; importer.SaveAndReimport();
                art[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                string materialPath = Root + "/Art/Materials/" + files[i] + ".mat";
                materials[i] = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (!materials[i])
                {
                    materials[i] = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
                    AssetDatabase.CreateAsset(materials[i], materialPath);
                }
                materials[i].mainTexture = art[i].texture; EditorUtility.SetDirty(materials[i]);
            }
            var stages = new LevelDefinition[LevelCatalog.LevelCount];
            for (int index = 1; index <= LevelCatalog.LevelCount; index++)
            {
                string path = Root + "/Data/Levels/Definitions/Level_" + index.ToString("000") + ".asset";
                stages[index - 1] = AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);
                if (!stages[index - 1]) { stages[index - 1] = ScriptableObject.CreateInstance<LevelDefinition>(); AssetDatabase.CreateAsset(stages[index - 1], path); }
                int world = (index - 1) / LevelCatalog.StagesPerWorld + 1, stage = (index - 1) % LevelCatalog.StagesPerWorld + 1;
                int moves = world <= 2 ? 20 : world <= 4 ? 19 : 18;
                int target = 1000 + (index - 1) * 10;
                var targets = new int[6];
                int colors = index <= 5 ? 0 : world <= 2 ? 1 : world <= 4 ? 2 : 3;
                int required = world == 1 ? 8 + stage / 2 : 14 + (index - 11) / 4;
                for (int j = 0; j < colors; j++) targets[(stage + world + j * 2) % 6] = required;
                stages[index - 1].ConfigureCampaign(index, 5408 + ((world - 1) * 20 + stage - 1) * 7919, moves, target, targets);
                EditorUtility.SetDirty(stages[index - 1]);
            }
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
            if (!catalog) { catalog = ScriptableObject.CreateInstance<LevelCatalog>(); AssetDatabase.CreateAsset(catalog, CatalogPath); }
            catalog.Configure(stages, art, materials, names); EditorUtility.SetDirty(catalog);
            return catalog;
        }

        private static void InstallGame(LevelCatalog catalog)
        {
            var scene = EditorSceneManager.OpenScene(BuildCommands.GameScene);
            var session = Object.FindFirstObjectByType<LevelSession>(); session.ConfigureCampaign(catalog);
            EditorUtility.SetDirty(session);
            var canvas = Object.FindFirstObjectByType<HUDView>().gameObject;
            var old = canvas.GetComponent<CampaignHUD>(); if (old) Object.DestroyImmediate(old);
            Transform sidebar = canvas.transform.Find("ScoreCard");
            foreach (string name in new[] { "WorldTitle", "CollectionGoals" }) Remove(sidebar.Find(name));
            var worldTitle = Text("WorldTitle", sidebar, .08f, .85f, .92f, .89f, "사탕 정원", 18);
            var goals = Rect("CollectionGoals", sidebar, .08f, .185f, .92f, .26f);
            var slots = new GameObject[3]; var icons = new Image[3]; var counts = new TMP_Text[3]; var candySprites = new Sprite[6];
            var pieces = AssetDatabase.LoadAssetAtPath<PieceCatalog>(Root + "/Data/Pieces/PieceCatalog.asset");
            for (int i = 0; i < 6; i++) candySprites[i] = pieces.Get((PieceColor)(i + 1)).sprite;
            for (int i = 0; i < 3; i++)
            {
                var slot = Rect("Goal" + i, goals, i / 3f, 0, (i + 1) / 3f, 1); slots[i] = slot.gameObject;
                var icon = Panel("Candy", slot, .10f, .30f, .90f, 1, Color.white, candySprites[i]);
                icons[i] = icon.GetComponent<Image>(); icons[i].type = Image.Type.Simple; icons[i].preserveAspect = true;
                counts[i] = Text("Count", slot, 0, 0, 1, .38f, "0/15", 17);
            }
            var stage = sidebar.Find("StageRibbon");
            Button map = stage.GetComponent<Button>() ?? stage.gameObject.AddComponent<Button>();
            var stageImage = stage.GetComponent<Image>(); stageImage.raycastTarget = true; map.targetGraphic = stageImage;
            canvas.AddComponent<CampaignHUD>().Configure(session, GameObject.Find("SugarGarden").GetComponent<SpriteRenderer>(), worldTitle, map, slots, icons, counts, candySprites);
            canvas.GetComponent<CandyLayout>().ConfigureCampaign((RectTransform)worldTitle.transform, goals);
            canvas.GetComponent<CandyLayout>().ApplyLayout();
            var card = canvas.transform.Find("ResultOverlay/ResultCard");
            Remove(card.Find("NextStage")); Remove(card.Find("WorldMap"));
            var again = (RectTransform)card.Find("PlayAgain"); Place(again, .06f, .045f, .47f, .16f);
            var next = Button("NextStage", card, .12f, .18f, .88f, .29f, "다음 스테이지", Pink);
            var toMap = Button("WorldMap", card, .53f, .045f, .94f, .16f, "로드맵", Purple);
            Place((RectTransform)card.Find("ResultMessage"), .06f, .30f, .94f, .43f);
            canvas.GetComponent<ResultPopup>().ConfigureNavigation(next, toMap);
            EditorSceneManager.SaveScene(scene);
        }

        private static void CreateMap(LevelCatalog catalog) => WorldMapSetup.Create(catalog);

        private static void Remove(Transform item) { if (item) Object.DestroyImmediate(item.gameObject); }
        private static void Place(RectTransform rect, float x0, float y0, float x1, float y1)
        { rect.anchorMin = new Vector2(x0, y0); rect.anchorMax = new Vector2(x1, y1); rect.offsetMin = rect.offsetMax = Vector2.zero; }
        private static RectTransform Rect(string name, Transform parent, float x0, float y0, float x1, float y1)
        { var result = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); result.SetParent(parent, false); Place(result, x0, y0, x1, y1); return result; }
        private static RectTransform Panel(string name, Transform parent, float x0, float y0, float x1, float y1, Color color, Sprite sprite)
        {
            var rect = Rect(name, parent, x0, y0, x1, y1); var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite; image.type = Image.Type.Sliced; image.color = color; image.raycastTarget = false; return rect;
        }
        private static TMP_Text Text(string name, Transform parent, float x0, float y0, float x1, float y1, string value, float size)
        {
            var text = Rect(name, parent, x0, y0, x1, y1).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font; text.text = value; text.fontSize = size; text.color = Ink; text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true; text.fontSizeMin = 12; text.fontSizeMax = size; text.raycastTarget = false; return text;
        }
        private static Button Button(string name, Transform parent, float x0, float y0, float x1, float y1, string caption, Color color, float size = 23)
        {
            var rect = Panel(name, parent, x0, y0, x1, y1, color, rounded); var image = rect.GetComponent<Image>(); image.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            Text("Label", rect, .03f, 0, .97f, 1, caption, size).color = Color.white; return button;
        }
    }
}
