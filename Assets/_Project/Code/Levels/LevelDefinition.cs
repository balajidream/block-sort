using System;
using System.Collections.Generic;
using BlockSort.Gameplay;
using UnityEngine;

namespace BlockSort.Levels
{
    public enum LevelModifier { LockedSlot, HiddenBlocks, FrozenBlocks }

    [CreateAssetMenu(menuName = "Block Sort/Level Definition", fileName = "LevelDefinition")]
    public sealed class LevelDefinition : ScriptableObject
    {
        [Min(1)] public int levelNumber;
        [Min(3)] public int slotCount = 3;
        [Range(1, 3)] public int emptySlots = 1;
        [Range(2, 6)] public int colorCount = 2;
        [Range(1, 4)] public int cubesPerColor = 4;
        [Tooltip("Bottom-to-top color values, separated by -1 per slot.")]
        public int[] serializedLayout;
        public int moveLimit;
        public LevelModifier[] modifiers;

        public Board CreateBoard()
        {
            var slots = new List<Slot>();
            var current = new List<CubeColor>();
            foreach (var value in serializedLayout ?? Array.Empty<int>())
            {
                if (value == -1)
                {
                    slots.Add(new Slot(cubesPerColor, current));
                    current.Clear();
                    continue;
                }
                current.Add((CubeColor)value);
            }
            if (slots.Count < slotCount) slots.Add(new Slot(cubesPerColor, current));
            while (slots.Count < slotCount) slots.Add(new Slot(cubesPerColor));
            return new Board(slots);
        }
    }
}
