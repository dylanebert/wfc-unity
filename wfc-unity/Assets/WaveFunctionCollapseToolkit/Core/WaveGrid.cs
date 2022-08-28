using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Wave {
    [Serializable]
    public class WaveGrid {
        static readonly int MAX_ATTEMPTS = 100;

        public string name = "WaveGrid";
        public List<Module> modules;
        public int height = 4;
        public int width = 4;
        public float moduleSize = 4f;
        public Vector3 offset;

        Transform root;
        List<Slot> slots;
        Slot[,] map;
        int attempts;

        public void Generate() {
            attempts = 0;
            while(attempts < MAX_ATTEMPTS) {
                attempts++;
                if(TryGenerate())
                    break;
                else
                    Logging.Log($"Failed attempt {attempts}, trying again");
            }
            if(attempts >= MAX_ATTEMPTS)
                Logging.Log($"Failed to generate after {attempts} attempts", LoggingLevel.Warning);
        }

        bool TryGenerate() {
            Clear();
            InitializeGrid();
            while(slots.Any(x => !x.collapsed)) {
                int minEntropy = slots.Where(x => !x.collapsed).Min(x => x.options.Count);
                Slot next = slots.Where(x => x.options.Count == minEntropy).OrderBy(x => UnityEngine.Random.Range(0f, 1f)).FirstOrDefault();
                next.Collapse();
                bool success = Evaluate();
                if(!success)
                    return false;
            }
            return true;
        }

        bool Evaluate() {
            for(int y = 0; y < height; y++) {
                for(int x = 0; x < width; x++) {
                    Slot slot = map[x, y];
                    // Down
                    if(y > 0) {
                        Slot down = map[x, y - 1];
                        slot.options = slot.options.Where(x => down.options.Any(y => x.down.name == y.up.name)).ToList();
                    }

                    // Left
                    if(x > 0) {
                        Slot left = map[x - 1, y];
                        slot.options = slot.options.Where(x => left.options.Any(y => x.left.name == y.right.name)).ToList();
                    }

                    // Up
                    if(y < height - 1) {
                        Slot up = map[x, y + 1];
                        slot.options = slot.options.Where(x => up.options.Any(y => x.up.name == y.down.name)).ToList();
                    }

                    // Right
                    if(x < width - 1) {
                        Slot right = map[x + 1, y];
                        slot.options = slot.options.Where(x => right.options.Any(y => x.right.name == y.left.name)).ToList();
                    }

                    if(slot.options.Count <= 1) {
                        bool success = slot.Collapse();
                        if(!success)
                            return false;
                    }
                }
            }
            return true;
        }

        void InitializeGrid() {
            root = new GameObject(name).transform;
            root.SetParent(Generator.instance.transform);
            root.localPosition = offset;
            slots = new List<Slot>();
            map = new Slot[width, height];
            Vector3 slotOffset = new Vector3(width * moduleSize / 2f, 0, height * moduleSize / 2f);
            for(int y = 0; y < height; y++) {
                for(int x = 0; x < width; x++) {
                    Slot slot = new GameObject($"slot_{x}_{y}").AddComponent<Slot>();
                    slot.transform.SetParent(root);
                    slot.transform.localPosition = new Vector3(
                        x * moduleSize - slotOffset.x + moduleSize / 2f,
                        0,
                        y * moduleSize - slotOffset.z + moduleSize / 2f
                    );
                    slot.options = modules;
                    slots.Add(slot);
                    map[x, y] = slot;
                }
            }
        }

        public void Clear(bool async = false) {
            if(root != null) {
#if UNITY_EDITOR
                if(async)
                    EditorApplication.delayCall += () => GameObject.DestroyImmediate(root.gameObject);
                else
                    GameObject.DestroyImmediate(root.gameObject);
#else
                GameObject.Destroy(root.gameObject);
#endif
            }
            map = null;
        }

        public void DrawGizmos() {
            if(map == null) return;
            Gizmos.color = Color.red;
            for(int y = 0; y < height; y++) {
                for(int x = 0; x < width; x++) {
                    Slot slot = map[x, y];
                    for(int i = 0; i < slot.options.Count; i++) {
                        Gizmos.DrawCube(slot.transform.position + Vector3.up * i, Vector3.one);
                    }
                }
            }
        }
    }
}