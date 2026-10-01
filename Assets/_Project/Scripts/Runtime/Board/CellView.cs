using PuzzleGame.Core.Board;
using UnityEngine;

namespace PuzzleGame.Runtime.Board
{
    public sealed class CellView : MonoBehaviour
    {
        public GridPosition Position { get; private set; }

        public void Initialize(GridPosition position)
        {
            Position = position;
            GetComponent<SpriteRenderer>().color = (position.X + position.Y) % 2 == 0
                ? new Color32(165, 140, 196, 255) : new Color32(184, 159, 211, 255);
            name = $"Cell_{position.X}_{position.Y}";
        }
    }
}
