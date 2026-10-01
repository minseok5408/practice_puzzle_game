using System;
using System.IO;
using System.Linq;
using PuzzleGame.Core.Board;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Config;
using PuzzleGame.Runtime.Levels;
using PuzzleGame.Runtime.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore.LowLevel;

namespace PuzzleGame.Editor
{
    public static class CandyThemeSetup
    {
        private const string Root = "Assets/_Project";
        private const string Theme = Root + "/UI/Theme";
        private static readonly Color Ink = new Color32(103, 57, 107, 255);
        private static readonly Color Muted = new Color32(148, 105, 149, 255);
        private static readonly Color Cream = new Color32(255, 249, 238, 255);
        private static readonly Color Pink = new Color32(226, 77, 151, 255);
        private static TMP_FontAsset font;
        private static Sprite rounded, circle, star;

        [MenuItem("Puzzle Game/Apply Candy Garden Theme")]
        public static void Apply()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(BuildCommands.GameScene);
            Directory.CreateDirectory(Theme);
            AssetDatabase.Refresh();
            rounded = Shape("Rounded", false, false);
            circle = Shape("Circle", true, false);
            star = Shape("Star", false, true);
            Sprite[] candies = ImportCandies();
            UpdatePieces(candies);
            UpdateCells();
            font = CreateCandyFont("Jua/Jua-Regular.ttf", "CandyBody");
            TMP_FontAsset titleFont = CreateCandyFont("BagelFatOne/BagelFatOne-Regular.ttf", "CandyTitle");
            var board = UnityEngine.Object.FindFirstObjectByType<BoardController>();
            var session = board.GetComponent<LevelSession>();
            var view = board.GetComponent<BoardView>();
            foreach (string name in new[] { "LevelCanvas", "CandyScenery" })
            {
                var old = GameObject.Find(name);
                if (old) UnityEngine.Object.DestroyImmediate(old);
            }

            var scenery = new GameObject("CandyScenery");
            Sprite backgroundSprite = ImportSingle(Root + "/Art/Backgrounds/SugarGarden.png", 100, Vector4.zero);
            SpriteRenderer background = World("SugarGarden", scenery.transform, backgroundSprite, Color.white, -30, Vector2.one);
            World("BoardShadow", scenery.transform, rounded, new Color32(96, 38, 113, 80), -12, new Vector2(8.72f, 8.72f)).transform.localPosition = new Vector3(0, -.13f);
            SpriteRenderer boardFrame = World("CreamFrame", scenery.transform, rounded, Cream, -11, new Vector2(8.66f, 8.66f));
            World("PinkRim", scenery.transform, rounded, new Color32(233, 152, 196, 255), -10, new Vector2(8.52f, 8.52f));
            World("BoardTray", scenery.transform, rounded, new Color32(115, 85, 153, 255), -9, new Vector2(8.30f, 8.30f));

            var canvasObject = new GameObject("LevelCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 800);
            scaler.matchWidthOrHeight = 1;
            var root = (RectTransform)canvasObject.transform;

            RectTransform sidebar = Panel("ScoreCard", root, .055f, .053f, .31f, .947f, Cream, true);
            RectTransform heading = Rect("Header", sidebar, .065f, .865f, .935f, .96f);
            TMP_Text brand = Text("Brand", heading, 0, 0, 1, 1, "SUGAR GARDEN", 34, Ink);
            brand.fontStyle = FontStyles.Normal;

            RectTransform moves = Panel("MoveBadge", sidebar, .20f, .49f, .80f, .69f, Pink);
            moves.GetComponent<Image>().sprite = rounded;
            TMP_Text movesValue = Text("MovesValue", moves, .04f, .28f, .96f, .97f, "20", 60, Color.white);
            movesValue.fontStyle = FontStyles.Normal;
            Text("MovesLabel", moves, .04f, .07f, .96f, .29f, "남은 이동", 18, Color.white);

            RectTransform score = Rect("Score", sidebar, .09f, .305f, .91f, .455f);
            Text("ScoreLabel", score, 0, .60f, 1, 1, "현재 점수", 18, Muted);
            TMP_Text scoreValue = Text("ScoreValue", score, 0, 0, 1, .64f, "0", 43, Ink);
            scoreValue.fontStyle = FontStyles.Normal;
            RectTransform goal = Rect("Goal", sidebar, .09f, .19f, .91f, .29f);
            Text("GoalLabel", goal, 0, .52f, 1, 1, "목표 점수", 16, Muted);
            TMP_Text targetValue = Text("TargetValue", goal, 0, 0, 1, .60f, "1,000", 27, Pink);
            RectTransform track = Panel("ProgressTrack", sidebar, .12f, .15f, .88f, .175f, new Color32(239, 214, 235, 255));
            RectTransform fill = Panel("ProgressFill", track, 0, 0, 0, 1, Pink);
            Button restart = Button("Restart", sidebar, .09f, .035f, .91f, .125f, "다시 하기", Pink);
            RectTransform stage = Panel("StageRibbon", sidebar, .06f, .72f, .94f, .795f, new Color32(140, 95, 170, 255), true);
            TMP_Text title = Text("LevelName", stage, .04f, .20f, .96f, .89f, "연습 스테이지", 22, Color.white);
            RectTransform itemSlots = Rect("ItemSlots", sidebar, .075f, .115f, .925f, .19f);
            for (int i = 0; i < 4; i++)
            {
                // Empty visual placeholders. Item data and interaction will be added later.
                RectTransform cell = Rect("ItemSlot" + (i + 1), itemSlots, i * .25f + .015f, 0, (i + 1) * .25f - .015f, 1);
                RectTransform slot = Panel("Frame", cell, 0, 0, 1, 1, new Color32(211, 187, 210, 255));
                var square = slot.gameObject.AddComponent<AspectRatioFitter>();
                square.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                square.aspectRatio = 1;
                RectTransform inset = Panel("Empty", slot, 0, 0, 1, 1, new Color32(243, 231, 239, 255));
                inset.offsetMin = new Vector2(3, 3);
                inset.offsetMax = new Vector2(-3, -3);
            }
            canvasObject.AddComponent<HUDView>().Configure(session, title, scoreValue, targetValue, movesValue, null, fill, restart);
            canvasObject.AddComponent<CandyLayout>().Configure(view, background, heading, sidebar, moves, score, goal, track,
                (RectTransform)restart.transform, stage, boardFrame, itemSlots);

            RectTransform overlay = Rect("ResultOverlay", root, 0, 0, 1, 1);
            overlay.gameObject.AddComponent<Image>().color = new Color(.24f, .10f, .32f, .76f);
            RectTransform card = Panel("ResultCard", overlay, .5f, .5f, .5f, .5f, Cream, true);
            card.sizeDelta = new Vector2(500, 440);
            Panel("ResultAccent", card, .06f, .94f, .94f, .955f, new Color32(245, 192, 218, 255));
            Image trophy = Panel("ResultStar", card, .41f, .69f, .59f, .90f, new Color32(255, 208, 88, 255), true).GetComponent<Image>();
            trophy.sprite = star; trophy.type = Image.Type.Simple; trophy.preserveAspect = true;
            TMP_Text resultTitle = Text("ResultTitle", card, .07f, .56f, .93f, .70f, "스테이지 성공!", 35, Ink);
            resultTitle.font = titleFont;
            resultTitle.fontStyle = FontStyles.Normal;
            TMP_Text detail = Text("ResultScore", card, .06f, .42f, .94f, .53f, "획득 1,000점  /  목표 1,000점", 23, Ink);
            TMP_Text message = Text("ResultMessage", card, .08f, .25f, .92f, .41f, "목표 점수를 달성했어요!", 20, Muted);
            message.textWrappingMode = TextWrappingModes.Normal;
            Button again = Button("PlayAgain", card, .18f, .075f, .82f, .22f, "다시 하기", Pink);
            canvasObject.AddComponent<ResultPopup>().Configure(session, overlay.gameObject, resultTitle, detail, message, again);
            overlay.gameObject.SetActive(false);
            SettingsSetup.Install(canvasObject);

            view.SetPlayableArea(new Rect(.33f, .015f, .665f, .97f));
            canvasObject.GetComponent<CandyLayout>().ApplyLayout();
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("CANDY THEME READY: six alpha sprites, garden background, responsive UI, rounded board, result popup.");
        }

        private static TMP_FontAsset CreateCandyFont(string sourceName, string assetName)
        {
            string path = Root + "/UI/Fonts/" + assetName + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (existing) return existing;
            var source = AssetDatabase.LoadAssetAtPath<Font>(Root + "/UI/Fonts/" + sourceName);
            if (!source) throw new InvalidOperationException("Missing font source: " + sourceName);
            var created = TMP_FontAsset.CreateFontAsset(source, 64, 7, GlyphRenderMode.SDFAA,
                1024, 1024, AtlasPopulationMode.Dynamic, true);
            created.name = assetName;
            created.material.name = assetName + " Material";
            AssetDatabase.CreateAsset(created, path);
            AssetDatabase.AddObjectToAsset(created.material, created);
            foreach (var texture in created.atlasTextures) AssetDatabase.AddObjectToAsset(texture, created);
            const string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz ,./:!?-"
                + "슈가 가든 연습 스테이지 현재 점수 목표 남은 이동 다시 하기 성공 실패 획득"
                + "목표 점수를 달성했어요 횟수 모두 사용 도전해 보세요 회";
            if (!created.TryAddCharacters(characters, out string missing))
                throw new InvalidOperationException("Font missing required UI characters: " + missing);
            EditorUtility.SetDirty(created);
            return created;
        }

        private static Sprite[] ImportCandies()
        {
            const string path = Root + "/Art/Sprites/Candies/CandyAtlas.png";
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.filterMode = FilterMode.Bilinear;
            importer.spritePixelsPerUnit = 512;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);
            string[] names = { "RedHeart", "OrangeGem", "YellowDrop", "GreenPillow", "BlueOrb", "PurpleFlower" };
            var metadata = new SpriteMetaData[6];
            for (int i = 0; i < 6; i++)
                metadata[i] = new SpriteMetaData { name = names[i], alignment = 9, pivot = Vector2.one * .5f,
                    rect = new Rect((i % 3) * 512, (1 - i / 3) * 512, 512, 512) };
#pragma warning disable 618
            importer.spritesheet = metadata;
#pragma warning restore 618
            CandySpriteAlignment.Center(importer);
            importer.SaveAndReimport();
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            return names.Select(name => sprites.Single(sprite => sprite.name == name)).ToArray();
        }

        private static void UpdatePieces(Sprite[] candies)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Art/Materials/CandyUnlit.mat");
            if (!material)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
                AssetDatabase.CreateAsset(material, Root + "/Art/Materials/CandyUnlit.mat");
            }
            material.mainTexture = candies[0].texture;
            EditorUtility.SetDirty(material);
            var catalog = AssetDatabase.LoadAssetAtPath<PieceCatalog>(Root + "/Data/Pieces/PieceCatalog.asset");
            var entries = new PieceCatalog.Entry[6];
            for (int i = 0; i < 6; i++) entries[i] = new PieceCatalog.Entry {
                color = (PieceColor)(i + 1), sprite = candies[i], material = material, tint = Color.white };
            catalog.Configure(entries); EditorUtility.SetDirty(catalog);
            SpecialCandySetup.Apply();
            string path = Root + "/Prefabs/Board/Piece.prefab";
            GameObject prefab = PrefabUtility.LoadPrefabContents(path);
            try
            {
                prefab.transform.localScale = Vector3.one * 1.02f;
                SpriteRenderer face = prefab.GetComponent<SpriteRenderer>();
                face.sprite = candies[0]; face.color = Color.white; face.sharedMaterial = material;
                var shadow = prefab.transform.Find("Shadow").GetComponent<SpriteRenderer>();
                shadow.sprite = candies[0]; shadow.sharedMaterial = material; shadow.color = new Color(.20f, .05f, .25f, .20f);
                shadow.transform.localPosition = new Vector3(.02f, -.045f);
                // Old themes used a filled disk behind the selected candy. Keep the silhouette clear.
                var oldHalo = prefab.transform.Find("SelectionHalo");
                if (oldHalo) UnityEngine.Object.DestroyImmediate(oldHalo.gameObject);
                PrefabUtility.SaveAsPrefabAsset(prefab, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
        }

        private static void UpdateCells()
        {
            string path = Root + "/Prefabs/Board/Cell.prefab";
            GameObject prefab = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var renderer = prefab.GetComponent<SpriteRenderer>();
                renderer.sprite = rounded;
                renderer.sharedMaterial = MaterialFor(rounded, "CandyCell");
                renderer.drawMode = SpriteDrawMode.Sliced;
                renderer.size = Vector2.one;
                renderer.color = new Color32(174, 148, 204, 255);
                prefab.transform.localScale = Vector3.one * .98f;
                PrefabUtility.SaveAsPrefabAsset(prefab, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
        }

        private static Material MaterialFor(Sprite sprite, string name)
        {
            string path = Root + "/Art/Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!material) { material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")); AssetDatabase.CreateAsset(material, path); }
            material.mainTexture = sprite.texture;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static SpriteRenderer World(string name, Transform parent, Sprite sprite, Color color, int order, Vector2 size)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite; renderer.color = color; renderer.sortingOrder = order;
            renderer.sharedMaterial = MaterialFor(sprite, name);
            if (size != Vector2.one) { renderer.drawMode = SpriteDrawMode.Sliced; renderer.size = size; }
            return renderer;
        }

        // Small code-native UI shapes use nine-slicing; the candy art remains an imported atlas.
        private static Sprite Shape(string name, bool isCircle, bool isStar)
        {
            string path = Theme + "/" + name + ".png";
            if (!File.Exists(path))
            {
                const int size = 128;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        Vector2 point = new Vector2(x + .5f, y + .5f) - Vector2.one * 64;
                        float distance;
                        if (isCircle) distance = point.magnitude - 61;
                        else if (isStar)
                        {
                            float angle = Mathf.Atan2(point.x, point.y);
                            float wave = Mathf.Abs(Mathf.Repeat(angle / (Mathf.PI * 2) * 5 + .5f, 1) * 2 - 1);
                            distance = point.magnitude - Mathf.Lerp(28, 60, wave);
                        }
                        else
                        {
                            Vector2 q = new Vector2(Mathf.Abs(point.x), Mathf.Abs(point.y)) - Vector2.one * 34;
                            distance = new Vector2(Mathf.Max(q.x, 0), Mathf.Max(q.y, 0)).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0) - 28;
                        }
                        float alpha = Mathf.Clamp01(.5f - distance);
                        float shade = Mathf.Lerp(.88f, 1, (float)y / size);
                        texture.SetPixel(x, y, new Color(shade, shade, shade, alpha));
                    }
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
            }
            return ImportSingle(path, 128, isCircle || isStar ? Vector4.zero : new Vector4(32, 32, 32, 32));
        }

        private static Sprite ImportSingle(string path, float ppu, Vector4 border)
        {
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ppu; importer.spriteBorder = border;
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings); importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static RectTransform Rect(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false); rect.anchorMin = new Vector2(x0, y0); rect.anchorMax = new Vector2(x1, y1);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }
        private static RectTransform Panel(string name, Transform parent, float x0, float y0, float x1, float y1, Color color, bool shadow = false)
        {
            var rect = Rect(name, parent, x0, y0, x1, y1);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = rounded; image.type = Image.Type.Sliced; image.color = color; image.raycastTarget = false;
            if (shadow)
            {
                var effect = rect.gameObject.AddComponent<Shadow>();
                effect.effectDistance = new Vector2(0, -6); effect.effectColor = new Color(.40f, .14f, .42f, .22f);
            }
            return rect;
        }
        private static TMP_Text Text(string name, Transform parent, float x0, float y0, float x1, float y1,
            string value, float size, Color color)
        {
            var rect = Rect(name, parent, x0, y0, x1, y1);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font; text.text = value; text.color = color; text.fontSize = size;
            text.enableAutoSizing = true; text.fontSizeMin = 11; text.fontSizeMax = size;
            text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            return text;
        }
        private static Button Button(string name, Transform parent, float x0, float y0, float x1, float y1, string label, Color color)
        {
            var rect = Panel(name, parent, x0, y0, x1, y1, color, true);
            var image = rect.GetComponent<Image>(); image.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(1, .92f, .98f); colors.pressedColor = new Color(.85f, .70f, .87f);
            colors.disabledColor = new Color(.85f, .73f, .84f, .5f); button.colors = colors;
            var text = Text(name + "Label", rect, .05f, .08f, .95f, .93f, label, 23, Color.white);
            text.fontStyle = FontStyles.Normal;
            rect.gameObject.AddComponent<CandyButtonMotion>();
            return button;
        }
    }
}
