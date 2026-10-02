using System;
using System.Linq;
using PuzzleGame.Core.Levels;
using PuzzleGame.Runtime.Board;
using PuzzleGame.Runtime.Services;
using PuzzleGame.Runtime.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PuzzleGame.Editor
{
    public static class QualitySetup
    {
        [MenuItem("Puzzle Game/Prepare Quality Improvements")]
        public static void Apply()
        {
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/UI/Fonts/CandyBody.asset");
            if(!font.TryAddCharacters(string.Join("",Localization.Entries.Values.SelectMany(v=>v)),out string missing))
                throw new InvalidOperationException("Missing guide glyphs: "+missing);
            foreach(var texture in font.atlasTextures)if(!AssetDatabase.Contains(texture))AssetDatabase.AddObjectToAsset(texture,font);
            EditorUtility.SetDirty(font);
            var icons=Enumerable.Range(0,4).Select(i=>AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/Items/"+(ItemType)i+".png")).ToArray();
            var badge=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/UI/Theme/Circle.png");
            if(!badge || icons.Any(i=>!i))throw new InvalidOperationException("Quality artwork missing.");
            foreach(string path in new[]{BuildCommands.GameScene,"Assets/_Project/Scenes/WorldMap.unity"})
            {
                var scene=EditorSceneManager.OpenScene(path);
                var dialogs=UnityEngine.Object.FindFirstObjectByType<PlayerDialogs>();dialogs.ConfigureGuide(icons);EditorUtility.SetDirty(dialogs);
                var board=UnityEngine.Object.FindFirstObjectByType<BoardView>();
                if(board){board.ConfigureColorLabels(font,badge);EditorUtility.SetDirty(board);}
                EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();Debug.Log("QUALITY SETUP PASSED: guide artwork, numeric labels and localized glyphs.");
        }
    }
}
