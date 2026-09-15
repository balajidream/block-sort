using System.Collections.Generic;
using BlockSort.Gameplay;
using UnityEngine;

namespace BlockSort.Levels
{
    /// <summary>
    /// Reverse generation: start from a solved board, then apply legal reverse moves.
    /// That always yields a solvable puzzle.
    /// </summary>
    public static class LevelGenerator
    {
        public static LevelDefinition Build(int levelNumber, int seed)
        {
            var difficulty = DifficultyCurve.ForLevel(levelNumber);
            var random = new System.Random(seed);
            var board = SolvedBoard(difficulty);
            var reverseMoves = Mathf.Max(4, difficulty.ScrambleMoves);
            for (var i = 0; i < reverseMoves; i++)
            {
                if (!TryReverseMove(board, random))
                {
                    break;
                }
            }

            var asset = ScriptableObject.CreateInstance<LevelDefinition>();
            asset.worldId = "world-01";
            asset.levelNumber = levelNumber;
            asset.slotCount = difficulty.SlotCount;
            asset.emptySlots = difficulty.EmptySlots;
            asset.colorCount = difficulty.ColorCount;
            asset.cubesPerColor = 4;
            asset.serializedLayout = Serialize(board);
            asset.moveLimit = 0;
            asset.parMoves = Mathf.Max(6, reverseMoves / 2);
            asset.reward = 25 + levelNumber * 5;
            asset.name = $"Level_{levelNumber:00}";
            return asset;
        }

        public static Board SolvedBoard(DifficultySettings difficulty)
        {
            var slots = new List<Slot>();
            var filled = difficulty.SlotCount - difficulty.EmptySlots;
            for (var i = 0; i < filled; i++)
            {
                var color = (CubeColor)(i % difficulty.ColorCount);
                slots.Add(new Slot(4, new[] { color, color, color, color }));
            }

            while (slots.Count < difficulty.SlotCount)
            {
                slots.Add(new Slot(4));
            }

            return new Board(slots);
        }

        static bool TryReverseMove(Board board, System.Random random)
        {
            var options = new List<(int from, int to)>();
            for (var from = 0; from < board.Slots.Count; from++)
            {
                for (var to = 0; to < board.Slots.Count; to++)
                {
                    if (from == to || board.Slots[from].IsEmpty || board.Slots[to].IsFull)
                    {
                        continue;
                    }

                    var moving = board.Slots[from].TopColor;
                    if (board.Slots[to].IsEmpty || board.Slots[to].TopColor == moving)
                    {
                        options.Add((from, to));
                    }
                }
            }

            if (options.Count == 0)
            {
                return false;
            }

            var pick = options[random.Next(options.Count)];
            board.ApplyMove(pick.from, pick.to);
            return true;
        }

        public static int[] Serialize(Board board)
        {
            var values = new List<int>();
            for (var i = 0; i < board.Slots.Count; i++)
            {
                foreach (var color in board.Slots[i].Cubes)
                {
                    values.Add((int)color);
                }

                if (i < board.Slots.Count - 1)
                {
                    values.Add(-1);
                }
            }

            return values.ToArray();
        }
    }
}
