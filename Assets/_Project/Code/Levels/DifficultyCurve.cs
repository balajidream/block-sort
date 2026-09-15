using UnityEngine;

namespace BlockSort.Levels
{
    public readonly struct DifficultySettings
    {
        public readonly int SlotCount;
        public readonly int EmptySlots;
        public readonly int ColorCount;
        public readonly int ScrambleMoves;

        public DifficultySettings(int slotCount, int emptySlots, int colorCount, int scrambleMoves)
        {
            SlotCount = slotCount;
            EmptySlots = emptySlots;
            ColorCount = colorCount;
            ScrambleMoves = scrambleMoves;
        }
    }

    public static class DifficultyCurve
    {
        public static DifficultySettings ForLevel(int levelNumber)
        {
            var number = Mathf.Max(1, levelNumber);
            var colorCount = number <= 10 ? 2 : Mathf.Clamp(2 + (number - 1) / 20, 2, 6);
            var slotCount = colorCount + 1;
            if (number >= 6 && slotCount < 4)
            {
                slotCount = 4;
            }

            var emptySlots = 2;
            if (number >= 30 && number % 5 == 0)
            {
                emptySlots = 1;
            }

            var scrambleMoves = 6 + number * 2;
            return new DifficultySettings(slotCount, emptySlots, colorCount, scrambleMoves);
        }
    }
}
