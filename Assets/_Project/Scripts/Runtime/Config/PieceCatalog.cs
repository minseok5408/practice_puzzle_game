using System;
using System.Collections.Generic;
using PuzzleGame.Core.Board;
using UnityEngine;

namespace PuzzleGame.Runtime.Config
{
    [CreateAssetMenu(menuName = "Puzzle Game/Piece Catalog")]
    public sealed class PieceCatalog : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public PieceColor color;
            public SpecialPieceType specialType;
            public Sprite sprite;
            public Material material;
            public Color tint;
        }

        [SerializeField] private Entry[] entries = Array.Empty<Entry>();

        public Entry Get(PieceState piece)
        {
            if (piece == null) throw new ArgumentNullException(nameof(piece));
            return Get(piece.Color, piece.SpecialType);
        }

        public Entry Get(PieceColor color) => Get(color, SpecialPieceType.None);

        private Entry Get(PieceColor color, SpecialPieceType specialType)
        {
            foreach (Entry entry in entries)
                if (entry.color == color && entry.specialType == specialType && entry.sprite != null && entry.material != null) return entry;
            throw new InvalidOperationException($"Missing piece appearance: {color}/{specialType}");
        }

        public void ConfigureSpecials(Entry[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            var all = new List<Entry>();
            foreach (var entry in entries)
                if (entry.specialType == SpecialPieceType.None) all.Add(entry);
            foreach (var entry in values)
            {
                if (entry.specialType == SpecialPieceType.None) throw new ArgumentException("Expected a special candy entry.", nameof(values));
                all.Add(entry);
            }
            entries = all.ToArray();
        }

        public void Configure(Entry[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            entries = (Entry[])values.Clone();
        }
    }
}
