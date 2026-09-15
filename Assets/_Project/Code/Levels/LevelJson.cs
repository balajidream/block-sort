using System;
using UnityEngine;

namespace BlockSort.Levels
{
    [Serializable]
    public sealed class LevelJsonRecord
    {
        public int number = 1;
        public int slotCount = 3;
        public int emptySlots = 1;
        public int colorCount = 2;
        public int reward = 30;
        public int parMoves = 8;
        public int[] layout = Array.Empty<int>();
    }

    [Serializable]
    public sealed class WorldJson
    {
        public string worldId = "world-01";
        public string displayName = "Walnut Workshop";
        public LevelJsonRecord[] levels = Array.Empty<LevelJsonRecord>();
    }

    public static class LevelJson
    {
        public static WorldJson Parse(TextAsset asset)
        {
            if (asset == null || string.IsNullOrWhiteSpace(asset.text))
            {
                return new WorldJson();
            }

            return JsonUtility.FromJson<WorldJson>(asset.text) ?? new WorldJson();
        }

        public static LevelDefinition ToDefinition(LevelJsonRecord record, string worldId)
        {
            var definition = ScriptableObject.CreateInstance<LevelDefinition>();
            definition.worldId = worldId;
            definition.levelNumber = record.number;
            definition.slotCount = record.slotCount;
            definition.emptySlots = record.emptySlots;
            definition.colorCount = record.colorCount;
            definition.cubesPerColor = 4;
            definition.serializedLayout = record.layout ?? Array.Empty<int>();
            definition.reward = record.reward;
            definition.parMoves = record.parMoves;
            definition.name = $"Level_{record.number:00}";
            return definition;
        }
    }
}
