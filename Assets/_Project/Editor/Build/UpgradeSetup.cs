using System;
using System.Linq;
using PuzzleGame.Runtime.Services;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace PuzzleGame.Editor
{
    public static class UpgradeSetup
    {
        [MenuItem("Puzzle Game/Prepare UI and Feature Upgrades")]
        public static void Apply()
        {
            var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/UI/Fonts/CandyBody.asset");
            if(!font.TryAddCharacters(string.Join("",Localization.Entries.Values.SelectMany(v=>v)),out string missing))
                throw new InvalidOperationException("Missing upgrade glyphs: "+missing);
            foreach(var texture in font.atlasTextures)if(!AssetDatabase.Contains(texture))AssetDatabase.AddObjectToAsset(texture,font);
            EditorUtility.SetDirty(font);PlayerSettings.bundleVersion="0.11.0";AssetDatabase.SaveAssets();
            Debug.Log("UI UPGRADE SETUP PASSED: localized glyphs; level assets preserved.");
        }
    }
}
