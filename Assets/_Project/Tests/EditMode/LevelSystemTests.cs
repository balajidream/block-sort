using BlockSort.Gameplay;
using BlockSort.Levels;
using NUnit.Framework;
using UnityEngine;

namespace BlockSort.Tests.EditMode
{
    public sealed class LevelSystemTests
    {
        [Test]
        public void JsonWorld_ContainsTwelveHandAuthoredLevels()
        {
            var json = Resources.Load<TextAsset>("Levels/world-01");
            Assert.That(json, Is.Not.Null);
            var world = LevelJson.Parse(json);
            Assert.That(world.levels.Length, Is.EqualTo(12));
            Assert.That(world.levels[0].number, Is.EqualTo(1));
            var board = LevelJson.ToDefinition(world.levels[0], world.worldId).CreateBoard();
            Assert.That(board.Slots.Count, Is.EqualTo(3));
            Assert.That(board.Slots[2].IsEmpty, Is.True);
        }

        [Test]
        public void ReverseGenerator_KeepsSolvableShape()
        {
            var definition = LevelGenerator.Build(14, 99);
            var board = definition.CreateBoard();
            Assert.That(board.Slots.Count, Is.GreaterThanOrEqualTo(3));
            Assert.That(board.HasAnyValidMove() || board.IsSolved(), Is.True);
        }
    }
}
