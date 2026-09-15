using UnityEngine;

namespace BlockSort.Levels
{
    [CreateAssetMenu(menuName = "Block Sort/World Database", fileName = "WorldDatabase")]
    public sealed class WorldDatabase : ScriptableObject
    {
        public LevelCatalog[] worlds = new LevelCatalog[0];

        public LevelCatalog PrimaryWorld => worlds != null && worlds.Length > 0 ? worlds[0] : null;

        public LevelDefinition GetLevel(int levelNumber)
        {
            return PrimaryWorld != null ? PrimaryWorld.Get(levelNumber) : null;
        }
    }
}
