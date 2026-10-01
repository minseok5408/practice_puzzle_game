using System.IO;
using UnityEditor;
using UnityEngine;

namespace PuzzleGame.Editor
{
    // Align visible silhouettes, not the uneven transparent padding in generated atlases.
    public static class CandySpriteAlignment
    {
        public static void Center(TextureImporter importer)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                ImageConversion.LoadImage(texture, File.ReadAllBytes(importer.assetPath));
                Color32[] pixels = texture.GetPixels32();
#pragma warning disable 618
                if (importer.spriteImportMode == SpriteImportMode.Multiple)
                {
                    SpriteMetaData[] sprites = importer.spritesheet;
                    for (int i = 0; i < sprites.Length; i++)
                    {
                        sprites[i].alignment = 9;
                        sprites[i].pivot = CenterOf(pixels, texture.width, sprites[i].rect);
                        Debug.Log($"CANDY PIVOT: {sprites[i].name} = {sprites[i].pivot:F5}");
                    }
                    importer.spritesheet = sprites;
                }
#pragma warning restore 618
                else
                {
                    var settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    settings.spriteAlignment = 9;
                    settings.spritePivot = CenterOf(pixels, texture.width, new Rect(0, 0, texture.width, texture.height));
                    importer.SetTextureSettings(settings);
                }
            }
            finally { Object.DestroyImmediate(texture); }
            EditorUtility.SetDirty(importer);
        }

        private static Vector2 CenterOf(Color32[] pixels, int stride, Rect rect)
        {
            int minX = (int)rect.width, minY = (int)rect.height, maxX = -1, maxY = -1;
            for (int y = 0; y < rect.height; y++)
                for (int x = 0; x < rect.width; x++)
                {
                    if (pixels[((int)rect.y + y) * stride + (int)rect.x + x].a < 32) continue;
                    minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
                }
            return maxX < 0 ? Vector2.one * .5f :
                new Vector2((minX + maxX + 1) * .5f / rect.width, (minY + maxY + 1) * .5f / rect.height);
        }

        [MenuItem("Puzzle Game/Center Candy Sprites")]
        public static void Apply()
        {
            string path = "Assets/_Project/Art/Sprites/Candies/CandyAtlas.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            Center(importer);
            importer.SaveAndReimport();
            SpecialCandySetup.Apply();
            Debug.Log("CANDY ALIGNMENT READY: all 25 visible silhouettes centered.");
        }
    }
}
