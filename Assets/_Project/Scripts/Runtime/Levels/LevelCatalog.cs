using System;
using UnityEngine;

namespace PuzzleGame.Runtime.Levels
{
    [CreateAssetMenu(menuName = "Puzzle Game/Level Catalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        public const int WorldCount = 5, StagesPerWorld = 10, LevelCount = WorldCount * StagesPerWorld;
        [SerializeField] private LevelDefinition[] levels;
        [SerializeField] private Sprite[] backgrounds;
        [SerializeField] private Sprite[] worldMaps;
        [SerializeField] private Material[] materials;
        [SerializeField] private string[] worldNames;
        public LevelDefinition Get(int number)
        {
            if (number < 1 || number > LevelCount) throw new ArgumentOutOfRangeException(nameof(number));
            return levels[number - 1];
        }
        public string WorldName(int world) => worldNames[world - 1];
        public Sprite Background(int world) => backgrounds[world - 1];
        public Sprite MapArtwork(int world) => worldMaps[world - 1];
        public void ConfigureMapArt(Sprite[] maps)
        {
            if (maps == null || maps.Length != WorldCount) throw new ArgumentException("Five map illustrations are required.");
            worldMaps = maps;
        }
        public Material BackgroundMaterial(int world) => materials[world - 1];
        public void Configure(LevelDefinition[] stages, Sprite[] art, Material[] looks, string[] names)
        {
            if (stages.Length != LevelCount || art.Length != WorldCount || looks.Length != WorldCount || names.Length != WorldCount)
                throw new ArgumentException("Catalog requires 50 levels and 5 worlds.");
            for (int i = 0; i < stages.Length; i++)
                if (!stages[i] || stages[i].Number != i + 1) throw new ArgumentException("Level order is invalid.");
            levels = stages; backgrounds = art; materials = looks; worldNames = names;
        }
    }
}
