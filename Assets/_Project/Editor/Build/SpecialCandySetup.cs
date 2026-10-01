using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleGame.Core.Board;
using PuzzleGame.Runtime.Config;
using UnityEditor;
using UnityEngine;

namespace PuzzleGame.Editor
{
    public static class SpecialCandySetup
    {
        private const string Root = "Assets/_Project";
        private const string Art = Root + "/Art/Sprites/Candies/";
        private static readonly string[] Names = { "RedHeart", "OrangeGem", "YellowDrop", "GreenPillow", "BlueOrb", "PurpleFlower" };

        [MenuItem("Puzzle Game/Apply Special Candy Art")]
        public static void Apply()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<PieceCatalog>(Root + "/Data/Pieces/PieceCatalog.asset");
            if (!catalog) throw new InvalidOperationException("The candy catalog must exist first.");
            var entries = new List<PieceCatalog.Entry>();
            AddAtlas(entries, "CandyRowAtlas.png", SpecialPieceType.Row);
            AddAtlas(entries, "CandyColumnAtlas.png", SpecialPieceType.Column);
            AddAtlas(entries, "CandyWrappedAtlas.png", SpecialPieceType.Bomb);

            string rainbowPath = Art + "CandyRainbow.png";
            var rainbowImporter = ConfigureImporter(rainbowPath, false);
            rainbowImporter.GetSourceTextureWidthAndHeight(out int width, out _);
            rainbowImporter.spritePixelsPerUnit = width * 1.05f;
            CandySpriteAlignment.Center(rainbowImporter);
            rainbowImporter.SaveAndReimport();
            Sprite rainbow = AssetDatabase.LoadAssetAtPath<Sprite>(rainbowPath);
            entries.Add(new PieceCatalog.Entry {
                color = PieceColor.None, specialType = SpecialPieceType.ColorClear,
                sprite = rainbow, material = MaterialFor("ColorClear", rainbow.texture), tint = Color.white
            });
            catalog.ConfigureSpecials(entries.ToArray());
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("SPECIAL CANDY ART READY: 6 horizontal stripes, 6 vertical stripes, 6 wrapped candies, 1 rainbow candy.");
        }

        private static void AddAtlas(List<PieceCatalog.Entry> entries, string filename, SpecialPieceType type)
        {
            string path = Art + filename;
            var importer = ConfigureImporter(path, true);
            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            if (width != 1536 || height != 1024) throw new InvalidOperationException("Expected a 1536x1024 candy atlas: " + path);
            importer.spritePixelsPerUnit = 544;
            var metadata = new SpriteMetaData[6];
            for (int i = 0; i < metadata.Length; i++)
                metadata[i] = new SpriteMetaData {
                    name = type + Names[i], alignment = 9, pivot = Vector2.one * .5f,
                    rect = new Rect(i % 3 * 512, (1 - i / 3) * 512, 512, 512)
                };
#pragma warning disable 618
            importer.spritesheet = metadata;
#pragma warning restore 618
            CandySpriteAlignment.Center(importer);
            importer.SaveAndReimport();
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            Material material = MaterialFor(type.ToString(), sprites[0].texture);
            for (int i = 0; i < Names.Length; i++)
                entries.Add(new PieceCatalog.Entry {
                    color = (PieceColor)(i + 1), specialType = type,
                    sprite = sprites.Single(sprite => sprite.name == type + Names[i]),
                    material = material, tint = Color.white
                });
        }

        private static TextureImporter ConfigureImporter(string path, bool multiple)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = multiple ? SpriteImportMode.Multiple : SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.filterMode = FilterMode.Bilinear;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteGenerateFallbackPhysicsShape = false;
            settings.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.SetTextureSettings(settings);
            return importer;
        }

        private static Material MaterialFor(string name, Texture texture)
        {
            string path = Root + "/Art/Materials/Candy" + name + "Unlit.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!material)
            {
                var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (!shader) throw new InvalidOperationException("Candy shader was not found.");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.mainTexture = texture;
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
