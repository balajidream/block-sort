using System;
using System.Collections.Generic;
using System.Linq;

namespace BlockSort.Gameplay
{
    public enum CubeColor { Red, Amber, Violet, Teal, Lime, Orange }

    public readonly struct MoveResult
    {
        public readonly CubeColor Color;
        public readonly int Count;
        public readonly bool DestinationCompleted;

        public MoveResult(CubeColor color, int count, bool destinationCompleted)
        {
            Color = color;
            Count = count;
            DestinationCompleted = destinationCompleted;
        }
    }

    public sealed class Slot
    {
        public int Capacity { get; }
        public List<CubeColor> Cubes { get; }
        public bool IsEmpty => Cubes.Count == 0;
        public bool IsFull => Cubes.Count == Capacity;
        public CubeColor TopColor => Cubes[^1];
        public int FreeSpace => Capacity - Cubes.Count;
        public bool IsComplete => IsFull && Cubes.All(color => color == Cubes[0]);

        public Slot(int capacity, IEnumerable<CubeColor> cubes = null)
        {
            Capacity = capacity;
            Cubes = cubes == null ? new List<CubeColor>() : new List<CubeColor>(cubes);
            if (Cubes.Count > Capacity) throw new ArgumentException("Slot cannot exceed capacity.");
        }
    }

    public sealed class Board
    {
        public IReadOnlyList<Slot> Slots { get; }

        public Board(IEnumerable<Slot> slots) => Slots = slots.ToList();

        public int CountTopRun(int slotIndex)
        {
            var slot = Slots[slotIndex];
            if (slot.IsEmpty) return 0;
            var color = slot.TopColor;
            var count = 0;
            for (var i = slot.Cubes.Count - 1; i >= 0 && slot.Cubes[i] == color; i--) count++;
            return count;
        }

        public bool CanMove(int from, int to)
        {
            if (from == to || from < 0 || to < 0 || from >= Slots.Count || to >= Slots.Count) return false;
            var source = Slots[from];
            var destination = Slots[to];
            return !source.IsEmpty && !destination.IsFull && (destination.IsEmpty || destination.TopColor == source.TopColor);
        }

        public MoveResult ApplyMove(int from, int to)
        {
            if (!CanMove(from, to)) throw new InvalidOperationException("Illegal block move.");
            var source = Slots[from];
            var destination = Slots[to];
            var color = source.TopColor;
            var count = Math.Min(CountTopRun(from), destination.FreeSpace);
            var start = source.Cubes.Count - count;
            destination.Cubes.AddRange(source.Cubes.GetRange(start, count));
            source.Cubes.RemoveRange(start, count);
            return new MoveResult(color, count, destination.IsComplete);
        }

        public bool IsSolved() => Slots.All(slot => slot.IsEmpty || slot.IsComplete);
        public bool HasAnyValidMove() => Slots.SelectMany((_, from) => Slots.Select((__, to) => CanMove(from, to))).Any(valid => valid);
        public int[][] Snapshot() => Slots.Select(slot => slot.Cubes.Select(color => (int)color).ToArray()).ToArray();

        public void Restore(int[][] snapshot)
        {
            if (snapshot == null || snapshot.Length != Slots.Count) throw new ArgumentException("Snapshot does not match board.");
            for (var i = 0; i < Slots.Count; i++)
            {
                if (snapshot[i].Length > Slots[i].Capacity) throw new ArgumentException("Snapshot overfills a slot.");
                Slots[i].Cubes.Clear();
                Slots[i].Cubes.AddRange(snapshot[i].Select(value => (CubeColor)value));
            }
        }

        public void ClearCompletedSlots()
        {
            foreach (var slot in Slots.Where(slot => slot.IsComplete)) slot.Cubes.Clear();
        }
    }
}
