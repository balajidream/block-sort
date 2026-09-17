using BlockSort.Gameplay;
using NUnit.Framework;

namespace BlockSort.Tests.EditMode
{
    public sealed class SessionTests
    {
        [Test]
        public void TapSlot_SelectsFilledTube()
        {
            var session = new BlockSortSession(new Board(new[]
            {
                new Slot(4, new[] { CubeColor.Red }),
                new Slot(4)
            }));
            session.TapSlot(0);
            Assert.That(session.SelectedSlot, Is.EqualTo(0));
        }

        [Test]
        public void TapSlot_IgnoresEmptyTubeUntilASourceIsSelected()
        {
            var session = new BlockSortSession(new Board(new[]
            {
                new Slot(4, new[] { CubeColor.Red }),
                new Slot(4)
            }));
            session.TapSlot(1);
            Assert.That(session.SelectedSlot, Is.EqualTo(-1));
        }

        [Test]
        public void TapSlot_RetargetsWhenDestinationCannotAccept()
        {
            var session = new BlockSortSession(new Board(new[]
            {
                new Slot(4, new[] { CubeColor.Red }),
                new Slot(4, new[] { CubeColor.Amber })
            }));
            session.TapSlot(0);
            session.TapSlot(1);
            Assert.That(session.SelectedSlot, Is.EqualTo(1));
            Assert.That(session.MoveCount, Is.EqualTo(0));
        }

        [Test]
        public void TapSlot_PoursMatchingRunOntoEmptyTube()
        {
            var session = new BlockSortSession(new Board(new[]
            {
                new Slot(4, new[] { CubeColor.Red, CubeColor.Red }),
                new Slot(4)
            }));
            session.TapSlot(0);
            session.TapSlot(1);
            Assert.That(session.SelectedSlot, Is.EqualTo(-1));
            Assert.That(session.MoveCount, Is.EqualTo(1));
            Assert.That(session.Board.Slots[1].Cubes.Count, Is.EqualTo(2));
        }
    }
}
