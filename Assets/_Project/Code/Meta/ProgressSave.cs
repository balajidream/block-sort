using UnityEngine;

namespace BlockSort.Meta
{
    public static class ProgressSave
    {
        const string CurrentKey = "block-sort.current";
        const string ClearedKey = "block-sort.cleared";
        const string CoinsKey = "block-sort.coins";

        public static int CurrentLevel
        {
            get => Mathf.Max(1, PlayerPrefs.GetInt(CurrentKey, 1));
            set => PlayerPrefs.SetInt(CurrentKey, Mathf.Max(1, value));
        }

        public static int Coins
        {
            get => Mathf.Max(0, PlayerPrefs.GetInt(CoinsKey, 120));
            set => PlayerPrefs.SetInt(CoinsKey, Mathf.Max(0, value));
        }

        public static int[] ClearedLevels
        {
            get
            {
                var raw = PlayerPrefs.GetString(ClearedKey, "");
                if (string.IsNullOrEmpty(raw))
                {
                    return System.Array.Empty<int>();
                }

                var parts = raw.Split(',');
                var values = new int[parts.Length];
                for (var i = 0; i < parts.Length; i++)
                {
                    int.TryParse(parts[i], out values[i]);
                }

                return values;
            }
            set
            {
                PlayerPrefs.SetString(ClearedKey, value == null || value.Length == 0 ? "" : string.Join(",", value));
            }
        }

        public static void Flush() => PlayerPrefs.Save();
    }
}
