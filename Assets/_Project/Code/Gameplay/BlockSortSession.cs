using System;
using System.Collections.Generic;
using System.Linq;

namespace BlockSort.Gameplay
{
    /// <summary>View-independent tap/undo coordinator. A scene adapter owns animation and input.</summary>
    public sealed class BlockSortSession
    {
        private readonly Stack<int[][]> _undoSnapshots = new();
        public Board Board { get; }
        public int SelectedSlot { get; private set; } = -1;
        public int UndoCharges { get; private set; } = 3;
        public int MoveCount { get; private set; }
        public event Action<int> SlotSelected;
        public event Action SelectionCleared;
        public event Action<int, int, MoveResult> MoveApplied;
        public event Action<int> SlotCleared;
        public event Action LevelWon;

        public BlockSortSession(Board board) => Board = board;

        public void TapSlot(int slotIndex)
        {
            if (SelectedSlot < 0)
            {
                if (Board.Slots[slotIndex].IsEmpty) return;
                SelectedSlot = slotIndex;
                SlotSelected?.Invoke(slotIndex);
                return;
            }

            if (SelectedSlot == slotIndex)
            {
                SelectedSlot = -1;
                SelectionCleared?.Invoke();
                return;
            }
            if (!Board.CanMove(SelectedSlot, slotIndex))
            {
                if (Board.Slots[slotIndex].IsEmpty)
                {
                    return;
                }

                SelectedSlot = slotIndex;
                SlotSelected?.Invoke(slotIndex);
                return;
            }

            _undoSnapshots.Push(Board.Snapshot());
            var source = SelectedSlot;
            var result = Board.ApplyMove(source, slotIndex);
            SelectedSlot = -1;
            MoveCount++;
            MoveApplied?.Invoke(source, slotIndex, result);
            if (result.DestinationCompleted)
            {
                Board.Slots[slotIndex].Cubes.Clear();
                SlotCleared?.Invoke(slotIndex);
            }
            if (Board.Slots.All(slot => slot.IsEmpty)) LevelWon?.Invoke();
        }

        public bool Undo()
        {
            if (UndoCharges <= 0 || _undoSnapshots.Count == 0) return false;
            Board.Restore(_undoSnapshots.Pop());
            UndoCharges--;
            SelectedSlot = -1;
            SelectionCleared?.Invoke();
            return true;
        }

        public bool TryAddExtraSlot()
        {
            Board.AddEmptySlot();
            return true;
        }
    }
}
