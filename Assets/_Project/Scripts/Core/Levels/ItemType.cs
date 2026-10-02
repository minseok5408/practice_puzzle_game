namespace PuzzleGame.Core.Levels
{
    public enum ItemType { Hammer, Bomb, Shuffle, ExtraMoves }

    public static class ItemRules
    {
        public const int Count=4, StartingStock=3, MaximumStock=99, BonusMoves=5;
        public static bool IsValid(ItemType type)=>(int)type>=0 && (int)type<Count;
        public static bool NeedsTarget(ItemType type)=>type==ItemType.Hammer || type==ItemType.Bomb;
    }
}
