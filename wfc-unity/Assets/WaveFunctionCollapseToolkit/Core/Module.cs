using UnityEngine;

namespace Wave {
    [CreateAssetMenu(menuName = "Wave/Module")]
    public class Module : ScriptableObject {
        public GameObject prefab;
        public Connection down;
        public Connection left;
        public Connection up;
        public Connection right;

        public bool ConnectsTo(Module other, int direction) {
            switch(direction) {
                case 0:
                    return down.ConnectsTo(other.up);
                case 1:
                    return left.ConnectsTo(other.right);
                case 2:
                    return up.ConnectsTo(other.down);
                case 3:
                    return right.ConnectsTo(other.left);
                default:
                    throw new System.ArgumentException("Invalid direction");
            }
        }
    }
}
