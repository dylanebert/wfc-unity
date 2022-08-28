using UnityEngine;

namespace Wave {
    [CreateAssetMenu(menuName = "Wave/Module")]
    public class Module : ScriptableObject {
        public GameObject prefab;
        public Connection down;
        public Connection left;
        public Connection up;
        public Connection right;
    }
}
