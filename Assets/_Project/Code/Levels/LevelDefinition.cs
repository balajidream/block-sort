using System;
using System.Collections.Generic;
using BlockSort.Gameplay;
using UnityEngine;

namespace BlockSort.Levels
{
    public enum LevelModifier
    {
        None = 0,
        LockedSlot = 1,
        HiddenBlocks = 2,
        FrozenBlocks = 3
    }

    [CreateAssetMenu(menuName = "Block Sort/Level Definition", fileName = "Level")]
    public sealed class LevelDefinition : ScriptableObject
    {
        public string worldId = "world-01";
        [Min(1)] public int levelNumber = 1;
        [Min(3)] public int slotCount = 3;
        [Range(0, 3)] public int emptySlots = 1;
        [Range(2, 6)] public int colorCount = 2;
        [Range(1, 4)] public int cubesPerColor = 4;
        [Tooltip("Bottom-to-top cube colors. Use -1 to start the next slot.")]
        public int[] serializedLayout = Array.Empty<int>();
        public int moveLimit;
        public int parMoves = 8;
        public int reward = 30;
        public LevelModifier[] modifiers = Array.Empty<LevelModifier>();

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

            if (current.Count > 0 || slots.Count < slotCount)
            {
                slots.Add(new Slot(cubesPerColor, current));
            }

            while (slots.Count < slotCount)
            {
                slots.Add(new Slot(cubesPerColor));
            }

            return new Board(slots);
        }
    }
}
