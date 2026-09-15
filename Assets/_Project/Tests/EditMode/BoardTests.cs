using BlockSort.Gameplay;
using NUnit.Framework;

namespace BlockSort.Tests.EditMode
{
    public sealed class BoardTests
    {
        [Test]
        public void ApplyMove_PartiallyMovesRunWhenDestinationHasOneSpace()
        {
            var board = new Board(new[] {
                new Slot(4, new[] { CubeColor.Red, CubeColor.Red, CubeColor.Red }),
                new Slot(4, new[] { CubeColor.Red, CubeColor.Red, CubeColor.Red }),
            });
            var result = board.ApplyMove(0, 1);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(board.Slots[0].Cubes.Count, Is.EqualTo(2));
            Assert.That(board.Slots[1].IsComplete, Is.True);
        }

        [Test]
        public void ApplyMove_MovesRunOntoEmptySlot()
        {
            var board = new Board(new[] { new Slot(4, new[] { CubeColor.Amber, CubeColor.Red, CubeColor.Red }), new Slot(4) });
            board.ApplyMove(0, 1);
            Assert.That(board.Slots[1].Cubes, Is.EqualTo(new[] { CubeColor.Red, CubeColor.Red }));
        }

        [Test]
        public void CanMove_RejectsMismatchedOrFullDestination()
        {
            var board = new Board(new[] {
                new Slot(4, new[] { CubeColor.Red }),
                new Slot(4, new[] { CubeColor.Amber }),
                new Slot(4, new[] { CubeColor.Red, CubeColor.Red, CubeColor.Red, CubeColor.Red }),
            });
            Assert.That(board.CanMove(0, 1), Is.False);
            Assert.That(board.CanMove(0, 2), Is.False);
        }

        [Test]
        public void SnapshotRestore_RestoresExactBoard()
        {
            var board = new Board(new[] { new Slot(4, new[] { CubeColor.Red, CubeColor.Amber }), new Slot(4) });
            var snapshot = board.Snapshot();
            board.ApplyMove(0, 1);
            board.Restore(snapshot);
            Assert.That(board.Snapshot()[0], Is.EqualTo(new[] { 0, 1 }));
            Assert.That(board.Snapshot()[1], Is.Empty);
        }

        [Test]
        public void IsSolved_AcceptsEmptyOrCompleteSlots()
        {
            var board = new Board(new[] { new Slot(4, new[] { CubeColor.Violet, CubeColor.Violet, CubeColor.Violet, CubeColor.Violet }), new Slot(4) });
            Assert.That(board.IsSolved(), Is.True);
        }
    }
}
