using UnityEngine;

namespace BlockSort.Gameplay
{
    [CreateAssetMenu(menuName = "Block Sort/Juice Config", fileName = "JuiceConfig")]
    public sealed class JuiceConfig : ScriptableObject
    {
        public float liftSeconds = 0.08f;
        public float travelSeconds = 0.17f;
        public float dropSeconds = 0.08f;
        public float runStagger = 0.02f;
        public bool lockInputDuringMove;
    }
}
