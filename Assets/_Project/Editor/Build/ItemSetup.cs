using System;
using System.Linq;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Services;
using PuzzleGame.Runtime.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PuzzleGame.Editor
{
    public static class ItemSetup
    {
        [MenuItem("Puzzle Game/Prepare Item Inventory")]
        public static void Apply()
        {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            var sprites=new Sprite[ItemRules.Count];
            for(int i=0;i<sprites.Length;i++)
            {
                string path="Assets/_Project/Art/UI/Items/"+(ItemType)i+".png";
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
                importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.maxTextureSize=512;importer.filterMode=FilterMode.Bilinear;
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;
                settings.spriteGenerateFallbackPhysicsShape=false;settings.alphaSource=TextureImporterAlphaSource.FromInput;importer.SetTextureSettings(settings);
                importer.SaveAndReimport();sprites[i]=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/UI/Fonts/CandyBody.asset");
            if(!font.TryAddCharacters(string.Join("",Localization.Entries.Values.SelectMany(v=>v)),out string missing))
                throw new InvalidOperationException("Missing item glyphs: "+missing);
            foreach(var texture in font.atlasTextures)if(!AssetDatabase.Contains(texture))AssetDatabase.AddObjectToAsset(texture,font);
            EditorUtility.SetDirty(font);
            var scene=EditorSceneManager.OpenScene(BuildCommands.GameScene);
            var hud=UnityEngine.Object.FindFirstObjectByType<CampaignHUD>();hud.ConfigureItems(sprites);EditorUtility.SetDirty(hud);
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("ITEM INVENTORY READY: four item sprites and localized glyphs.");
        }
    }
}
