using UnityEngine;

namespace BlockSort.Levels
{
    [CreateAssetMenu(menuName = "Block Sort/Level Catalog", fileName = "LevelCatalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        public string worldId = "world-01";
        public string displayName = "Walnut Workshop";
        public LevelDefinition[] levels = new LevelDefinition[0];

        public int Count => levels == null ? 0 : levels.Length;

        public LevelDefinition Get(int levelNumber)
        {
            if (levels == null || levels.Length == 0)
            {
                return null;
            }

            foreach (var level in levels)
            {
                if (level != null && level.levelNumber == levelNumber)
                {
                    return level;
                }
            }

            var index = Mathf.Clamp(levelNumber - 1, 0, levels.Length - 1);
            return levels[index];
        }
    }
}
