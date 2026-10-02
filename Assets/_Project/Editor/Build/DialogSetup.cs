using System;
using System.Linq;
using PuzzleGame.Runtime.Config;
using PuzzleGame.Runtime.Services;
using PuzzleGame.Runtime.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PuzzleGame.Editor
{
    // Update dialog artwork and glyphs without regenerating levels or scene layouts.
    public static class DialogSetup
    {
        [MenuItem("Puzzle Game/Prepare Settings and Preview")]
        public static void Apply()
        {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/UI/Fonts/CandyBody.asset");
            string characters=string.Join("",Localization.Entries.Values.SelectMany(v=>v))+"★%0123456789";
            if(!font.TryAddCharacters(characters,out string missing))throw new InvalidOperationException("Missing dialog glyphs: "+missing);
            foreach(var texture in font.atlasTextures)if(!AssetDatabase.Contains(texture))AssetDatabase.AddObjectToAsset(texture,font);
            EditorUtility.SetDirty(font);
            var pieces=AssetDatabase.LoadAssetAtPath<PieceCatalog>("Assets/_Project/Data/Pieces/PieceCatalog.asset");
            foreach(string path in new[]{CampaignSetup.MapScene,BuildCommands.GameScene})
            {
                var scene=EditorSceneManager.OpenScene(path);
                var dialogs=UnityEngine.Object.FindFirstObjectByType<PlayerDialogs>();
                dialogs.ConfigureArtwork(pieces);EditorUtility.SetDirty(dialogs);
                EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();Debug.Log("DIALOGS READY: settings controls, localized goals and candy artwork.");
        }
    }
}
