using System.Linq;
using PuzzleGame.Core.Board;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Config;
using PuzzleGame.Runtime.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PuzzleGame.Editor
{
    public static class VisualPolishSetup
    {
        [MenuItem("Puzzle Game/Prepare Result Presentation")]
        public static void ApplyResults()
        {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            var scene=EditorSceneManager.OpenScene(BuildCommands.GameScene);
            ConfigureResults(Object.FindFirstObjectByType<ResultPopup>());
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("RESULT PRESENTATION READY: victory, retry and completion artwork");
        }

        public static void ConfigureResults(ResultPopup popup)
        {
            var candies=AssetDatabase.LoadAssetAtPath<PieceCatalog>("Assets/_Project/Data/Pieces/PieceCatalog.asset");
            popup.ConfigureArtwork(ImportBadge("VictoryBadge"),ImportBadge("RetryBadge"),ImportBadge("CompletionCrown"),
                Enumerable.Range(1,6).Select(i=>candies.Get((PieceColor)i).sprite).ToArray());
            EditorUtility.SetDirty(popup);
        }

        private static Sprite ImportBadge(string name)
        {
            string path="Assets/_Project/Art/UI/Results/"+name+".png";
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.maxTextureSize=512;importer.filterMode=FilterMode.Bilinear;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;
            settings.spriteGenerateFallbackPhysicsShape=false;settings.alphaSource=TextureImporterAlphaSource.FromInput;importer.SetTextureSettings(settings);
            importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        [MenuItem("Puzzle Game/Prepare Goal and Board Polish")]
        public static void ApplyGoals()
        {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            var stages=new[]{ImportIce("IceCracked"),ImportIce("IceChipped"),ImportIce("IceIntact")};
            string prefabPath="Assets/_Project/Prefabs/Board/Cell.prefab";
            var prefab=PrefabUtility.LoadPrefabContents(prefabPath);
            try{prefab.GetComponent<CellView>().ConfigureFrost(stages);PrefabUtility.SaveAsPrefabAsset(prefab,prefabPath);}
            finally{PrefabUtility.UnloadPrefabContents(prefab);}
            DialogSetup.Apply();
            foreach(string path in new[]{CampaignSetup.MapScene,BuildCommands.GameScene})
            {
                var scene=EditorSceneManager.OpenScene(path);
                var dialogs=Object.FindFirstObjectByType<PlayerDialogs>();dialogs.ConfigureFrostIcon(stages[2]);EditorUtility.SetDirty(dialogs);
                var hud=Object.FindFirstObjectByType<CampaignHUD>();
                if(hud){hud.ConfigureGoalCheck(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/UI/Theme/SettingsCheck.png"));hud.ConfigureGoalIce(stages[2]);EditorUtility.SetDirty(hud);}
                EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();Debug.Log("GOAL AND BOARD POLISH READY: three dimensional ice stages, goals and hints");
        }

        private static Sprite ImportIce(string name)
        {
            string path="Assets/_Project/Art/Sprites/Frost/"+name+".png";
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.maxTextureSize=1024;importer.filterMode=FilterMode.Bilinear;importer.spritePixelsPerUnit=1000;
            importer.GetSourceTextureWidthAndHeight(out int width,out int height);
            // All variants keep the same composition. Ignore the outside transparent gutter and stray glints.
            var rect=new Rect(width*.073f,height*.11f,width*.835f,height*.793f);
#pragma warning disable 618
            importer.spritesheet=new[]{new SpriteMetaData{name=name,rect=rect,alignment=9,pivot=Vector2.one*.5f}};
#pragma warning restore 618
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteGenerateFallbackPhysicsShape=false;settings.alphaSource=TextureImporterAlphaSource.FromInput;importer.SetTextureSettings(settings);
            importer.SaveAndReimport();return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Single();
        }
    }
}
