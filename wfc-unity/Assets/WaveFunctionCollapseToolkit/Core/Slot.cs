using System.Collections.Generic;
using UnityEngine;

namespace Wave {
    public class Slot : MonoBehaviour {
        public List<Module> options { get; set; }

        public bool collapsed => options.Count <= 1;

        GameObject child;

        public bool Collapse() {
            if(options.Count == 0) {
                Logging.Log($"No options found for {name}");
                return false;
            }
            Module result = options[Random.Range(0, options.Count)];
            if(result.prefab != null)
                child = Instantiate(result.prefab, transform);
            options = new List<Module> { result };
            return true;
        }

        public void Clear() {
            options.Clear();
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