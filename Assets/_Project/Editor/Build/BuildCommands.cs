using System;
using System.IO;
using PuzzleGame.Core.Board;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Config;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PuzzleGame.Editor
{
    public static class BuildCommands
    {
        public const string GameScene = "Assets/_Project/Scenes/Game.unity";
        private const string Root = "Assets/_Project";
        private static readonly string[] ShapeNames = { "Circle", "Diamond", "Square", "Triangle", "Hexagon", "Star" };
        private static readonly Color[] Tints = {
            new Color32(245, 105, 134, 255), new Color32(255, 164, 91, 255),
            new Color32(255, 218, 104, 255), new Color32(100, 220, 176, 255),
            new Color32(107, 190, 250, 255), new Color32(182, 145, 251, 255)
        };

        [MenuItem("Puzzle Game/Prepare First Board")]
        public static void PrepareFirstBoard()
        {
            // Never replace an existing game scene or silently discard editor changes.
            if (File.Exists(GameScene))
            {
                Debug.Log("Game scene already exists. Open Assets/_Project/Scenes/Game.unity.");
                return;
            }
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EnsureFolder(Root + "/Scenes/Sandbox");
            EnsureFolder(Root + "/Scripts/Runtime/Debug");
            EnsureFolder(Root + "/Prefabs/Board");
            EnsureFolder(Root + "/Data/Pieces");
            EnsureFolder(Root + "/Art/Sprites/Pieces");
            EnsureFolder(Root + "/Art/Sprites/Board");
            EnsureFolder(Root + "/Art/Materials");

            MoveIfPresent("Assets/Scenes/SampleScene.unity", Root + "/Scenes/Sandbox/SetupCheck.unity");
            MoveIfPresent("Assets/Scripts/SetupCheck.cs", Root + "/Scripts/Runtime/Debug/SetupCheck.cs");

            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) throw new InvalidOperationException("URP 2D unlit shader was not found.");
            var material = new Material(shader) { name = "BoardUnlit" };
            AssetDatabase.CreateAsset(material, Root + "/Art/Materials/BoardUnlit.mat");

            var entries = new PieceCatalog.Entry[6];
            for (int i = 0; i < entries.Length; i++)
                entries[i] = new PieceCatalog.Entry {
                    color = (PieceColor)(i + 1),
                    sprite = CreateSprite(Root + "/Art/Sprites/Pieces/" + ShapeNames[i] + ".png", i),
                    tint = Tints[i]
                };
            for (int i = 0; i < entries.Length; i++)
            {
                var pieceMaterial = new Material(shader) { name = ShapeNames[i] + "Unlit", mainTexture = entries[i].sprite.texture };
                AssetDatabase.CreateAsset(pieceMaterial, Root + "/Art/Materials/" + ShapeNames[i] + "Unlit.mat");
                entries[i].material = pieceMaterial;
            }
            Sprite cellSprite = CreateSprite(Root + "/Art/Sprites/Board/Cell.png", 6);
            material.mainTexture = cellSprite.texture;
            var catalog = ScriptableObject.CreateInstance<PieceCatalog>();
            catalog.Configure(entries);
            AssetDatabase.CreateAsset(catalog, Root + "/Data/Pieces/PieceCatalog.asset");

            var pieceObject = new GameObject("Piece");
            var face = pieceObject.AddComponent<SpriteRenderer>();
            face.sharedMaterial = entries[0].material;
            face.sortingOrder = 2;
            face.sprite = entries[0].sprite;
            face.color = entries[0].tint;
            pieceObject.transform.localScale = Vector3.one * 0.80f;
            var shadowObject = new GameObject("Shadow");
            shadowObject.transform.SetParent(pieceObject.transform, false);
            shadowObject.transform.localPosition = new Vector3(0.025f, -0.065f, 0);
            var shadow = shadowObject.AddComponent<SpriteRenderer>();
            shadow.sharedMaterial = entries[0].material;
            shadow.sprite = entries[0].sprite;
            shadow.sortingOrder = 1;
            shadow.color = new Color(0.015f, 0.025f, 0.055f, 0.45f);
            var pieceView = pieceObject.AddComponent<PieceView>();
            pieceView.Configure(face, shadow);
            GameObject piecePrefab = PrefabUtility.SaveAsPrefabAsset(pieceObject, Root + "/Prefabs/Board/Piece.prefab");
            UnityEngine.Object.DestroyImmediate(pieceObject);

            var cellObject = new GameObject("Cell");
            var cellRenderer = cellObject.AddComponent<SpriteRenderer>();
            cellRenderer.sprite = cellSprite;
            cellRenderer.sharedMaterial = material;
            cellRenderer.color = new Color32(33, 47, 67, 255);
            cellRenderer.sortingOrder = -1;
            cellObject.AddComponent<CellView>();
            GameObject cellPrefab = PrefabUtility.SaveAsPrefabAsset(cellObject, Root + "/Prefabs/Board/Cell.prefab");
            UnityEngine.Object.DestroyImmediate(cellObject);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 4.7f;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(15, 23, 37, 255);
            cameraObject.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = false;
            var light = new GameObject("Global Light 2D").AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1;
            var board = new GameObject("BoardRoot");
            var view = board.AddComponent<BoardView>();
            view.Configure(piecePrefab.GetComponent<PieceView>(), cellPrefab.GetComponent<CellView>(), catalog, camera);
            board.AddComponent<BoardController>();
            board.AddComponent<BoardInput>().Configure(AssetDatabase.LoadAssetAtPath<InputActionAsset>(Root + "/Input/PuzzleInput.inputactions"));
            EditorSceneManager.SaveScene(scene, GameScene);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(GameScene, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("First board assets and Game scene prepared.");
        }

        [MenuItem("Puzzle Game/Build Windows Prototype")]
        public static void BuildWindows()
        {
            if (!File.Exists(GameScene)) throw new InvalidOperationException("Prepare the first board before building.");
            Directory.CreateDirectory("Builds/Windows/0.7.0");
            var options = new BuildPlayerOptions {
                scenes = new[] { GameScene },
                locationPathName = "Builds/Windows/0.7.0/practice_puzzle_game.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException($"Windows build failed: {report.summary.result}.");
            File.Copy(Root + "/UI/Fonts/NanumGothic/OFL.txt", "Builds/Windows/0.7.0/Font-LICENSE.txt", true);
            Directory.CreateDirectory("Builds/Windows/0.7.0/ThirdPartyNotices");
            File.Copy(Root + "/UI/Fonts/Jua/OFL.txt", "Builds/Windows/0.7.0/ThirdPartyNotices/Jua-OFL.txt", true);
            File.Copy(Root + "/UI/Fonts/BagelFatOne/OFL.txt", "Builds/Windows/0.7.0/ThirdPartyNotices/BagelFatOne-OFL.txt", true);
            File.Copy("Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt", "Builds/Windows/0.7.0/ThirdPartyNotices/LiberationSans-OFL.txt", true);
            File.Copy("Assets/TextMesh Pro/Sprites/EmojiOne Attribution.txt", "Builds/Windows/0.7.0/ThirdPartyNotices/EmojiOne-Attribution.txt", true);
            Debug.Log($"Windows build succeeded: {report.summary.totalSize} bytes.");
        }

        [MenuItem("Puzzle Game/Export Board Preview")]
        public static void ExportPreview()
        {
            if (Application.isPlaying || UnityEngine.SceneManagement.SceneManager.GetSceneByPath(GameScene).isLoaded)
                throw new InvalidOperationException("Stop Play Mode and close the saved Game scene before exporting a preview.");
            var scene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Additive);
            RenderTexture target = null;
            Texture2D image = null;
            RenderTexture previous = RenderTexture.active;
            try
            {
                BoardController controller = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                    if (root.TryGetComponent(out BoardController found)) controller = found;
                if (!controller) throw new InvalidOperationException("No board in Game scene.");
                controller.GenerateBoard();
                var view = controller.GetComponent<BoardView>();
                var camera = view.BoardCamera;
                camera.aspect = 16f / 10f;
                view.FitCamera();
                target = new RenderTexture(1280, 800, 24);
                target.Create();
                var request = new UniversalRenderPipeline.SingleCameraRequest { destination = target };
                RenderPipeline.SubmitRenderRequest(camera, request);
                RenderTexture.active = target;
                image = new Texture2D(1280, 800, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 1280, 800), 0, 0);
                image.Apply();
                Directory.CreateDirectory("Builds/Preview");
                File.WriteAllBytes("Builds/Preview/Board.png", image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                if (image) UnityEngine.Object.DestroyImmediate(image);
                if (target) { target.Release(); UnityEngine.Object.DestroyImmediate(target); }
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void MoveIfPresent(string oldPath, string newPath)
        {
            if (AssetDatabase.LoadMainAssetAtPath(oldPath) == null) return;
            if (AssetDatabase.LoadMainAssetAtPath(newPath) != null)
                throw new InvalidOperationException($"Destination already exists: {newPath}");
            string error = AssetDatabase.MoveAsset(oldPath, newPath);
            if (!string.IsNullOrEmpty(error)) throw new IOException(error);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
        }

        private static Sprite CreateSprite(string path, int shape)
        {
            const int size = 128;
            const int samples = 4;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    int covered = 0;
                    for (int sy = 0; sy < samples; sy++)
                        for (int sx = 0; sx < samples; sx++)
                        {
                            float px = ((x + (sx + 0.5f) / samples) / size - 0.5f) * 2;
                            float py = ((y + (sy + 0.5f) / samples) / size - 0.5f) * 2;
                            if (Inside(shape, px, py)) covered++;
                        }
                    float shade = Mathf.Lerp(0.80f, 1f, (float)y / (size - 1));
                    texture.SetPixel(x, y, new Color(shade, shade, shade, (float)covered / (samples * samples)));
                }
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = size;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            var textureSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(textureSettings);
            textureSettings.spriteMeshType = SpriteMeshType.FullRect;
            textureSettings.spriteGenerateFallbackPhysicsShape = false;
            textureSettings.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.SetTextureSettings(textureSettings);
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static bool Inside(int shape, float x, float y)
        {
            float ax = Mathf.Abs(x), ay = Mathf.Abs(y);
            switch (shape)
            {
                case 0: return x * x + y * y <= 0.82f * 0.82f;
                case 1: return ax + ay <= 0.94f;
                case 2: return RoundedSquare(ax, ay, 0.77f, 0.16f);
                case 3: return y >= -0.70f && y <= 0.88f && ax <= (0.88f - y) * 0.55f;
                case 4: return ay <= 0.76f && ax <= 0.88f - ay * 0.5f;
                case 5:
                    float angle = Mathf.Atan2(y, x) - Mathf.PI * 0.5f;
                    float radius = Mathf.Sqrt(x * x + y * y);
                    float sector = Mathf.Repeat(angle, Mathf.PI * 0.4f);
                    float local = Mathf.Min(sector, Mathf.PI * 0.4f - sector);
                    // Five pointed stars with a distinct silhouette at small sizes.
                    float edge = 0.86f * 0.39f * Mathf.Sin(Mathf.PI * 0.2f)
                        / (0.39f * Mathf.Sin(Mathf.PI * 0.2f - local) + 0.86f * Mathf.Sin(local));
                    return radius <= edge;
                default: return RoundedSquare(ax, ay, 0.93f, 0.15f);
            }
        }

        private static bool RoundedSquare(float x, float y, float extent, float radius)
        {
            float dx = Mathf.Max(0, x - (extent - radius));
            float dy = Mathf.Max(0, y - (extent - radius));
            return dx * dx + dy * dy <= radius * radius;
        }
    }
}
