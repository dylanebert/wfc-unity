using System.Collections.Generic;
using UnityEngine;

namespace Wave {
    public class Slot : MonoBehaviour {
        public bool[] possibilities { get; private set; }
        public float[] weights { get; private set; }
        public HashSet<int>[,] affordances { get; private set; }
        public int entropy { get; private set; }

        public bool solved => entropy == 1;

        WaveGrid grid;
        GameObject child;

        public void Initialize(WaveGrid grid, int size) {
            this.grid = grid;
            possibilities = new bool[size];
            weights = new float[size];
            affordances = new HashSet<int>[4, size];
            entropy = size;
            for(int i = 0; i < size; i++) {
                possibilities[i] = true;
                weights[i] = 1;
            }
        }

        public void RemovePossibility(int index) {
            Debug.Assert(possibilities[index] && entropy > 1);
            possibilities[index] = false;
            weights[index] = 0;
            entropy--;
        }

        public void Collapse() {
            for(int i = 0; i < possibilities.Length; i++) {
                if(possibilities[i]) {
                    GameObject prefab = grid.modules[i].prefab;
                    if(prefab != null)
                        child = Instantiate(prefab, transform);
                    break;
                }
            }
        }

        bool CheckConnection(Module a, Module b, int index) {
            switch(index) {
                case 0:
                    return a.down.name == b.up.name;
                case 1:
                    return a.right.name == b.left.name;
                case 2:
                    return a.up.name == b.down.name;
                case 3:
                    return a.left.name == b.right.name;
                default:
                    throw new System.ArgumentException("Invalid index");
            }
        }

        public void Clear() {
            if(child != null) {
#if UNITY_EDITOR
                DestroyImmediate(child);
#else
                Destroy(child);
#endif
            }
        }
    }
}