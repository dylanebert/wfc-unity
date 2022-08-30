using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Wave {
    [System.Serializable]
    public class WaveGrid {
        static readonly int[] OPPOSITE = { 2, 3, 0, 1 };
        static readonly int MAX_ITERATIONS = 10000;

        public string name = "WaveGrid";
        public List<Module> modules;
        public int width = 4;
        public int height = 4;
        public float moduleSize = 4f;
        public Vector3 offset;

        Transform root;
        Stack<(int, int)> stack;
        Slot[] grid;
        Task task;

        public WaveGrid() {
            stack = new Stack<(int, int)>();
        }

        public void Generate() {
            if(task != null && !task.IsCompleted) return;
            if(Generator.instance.delay > 0)
                task = RunAsync();
            else
                Run();
        }

        void Run() {
            Clear();
            InitializeGrid();
            for(int i = 0; i < MAX_ITERATIONS; i++) {
                int index = NextSlot();
                if(index >= 0) {
                    Solve(index);
                    while(stack.Count > 0)
                        Propagate();
                } else return;
            }
        }

        async Task RunAsync() {
            Clear();
            InitializeGrid();
            int milliseconds = Mathf.CeilToInt(Generator.instance.delay * 1000);
            for(int i = 0; i < MAX_ITERATIONS; i++) {
                int index = NextSlot();
                if(index >= 0) {
                    Solve(index);
                    while(stack.Count > 0) {
                        Propagate();
                        await Task.Delay(milliseconds);
                    }
                } else {
                    return;
                }
            }
        }

        void Propagate() {
            (int index, int possibility) = stack.Pop();
            if(grid[index].solved)
                grid[index].Collapse();

            // Iterate possible modules in neighbors
            for(int i = 0; i < 4; i++) {
                if(GetAdjacent(index, i, out int adjacent) && !grid[adjacent].solved) {
                    for(int j = 0; j < modules.Count; j++) {
                        if(!grid[adjacent].possibilities[j]) continue;

                        // Check if the possibility should now be removed
                        int opposite = OPPOSITE[i];
                        grid[adjacent].affordances[opposite, j].Remove(possibility);
                        if(grid[adjacent].affordances[opposite, j].Count == 0) {
                            Constrain(adjacent, j);
                        }
                    }
                }
            }
        }

        int NextSlot() {
            float min = float.MaxValue;
            int argmin = -1;
            for(int i = 0; i < grid.Length; i++) {
                if(!grid[i].solved && grid[i].entropy <= min) {
                    float noise = 1e-6f * Random.Range(0f, 1f);
                    if(grid[i].entropy + noise < min) {
                        min = grid[i].entropy + noise;
                        argmin = i;
                    }
                }
            }
            return argmin;
        }

        void Solve(int index) {
            int choice = grid[index].weights.Sample();
            for(int i = 0; i < modules.Count; i++) {
                if(grid[index].possibilities[i] != (i == choice))
                    Constrain(index, i);
            }
        }

        void Constrain(int index, int possibility) {
            grid[index].RemovePossibility(possibility);
            stack.Push((index, possibility));
        }

        void InitializeGrid() {
            root = new GameObject(name).transform;
            root.SetParent(Generator.instance.transform);
            root.localPosition = offset;
            grid = new Slot[width * height];
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
                    grid[y * width + x] = slot;
                    slot.Initialize(this, modules.Count);
                }
            }
            for(int y = 0; y < height; y++) {
                for(int x = 0; x < width; x++) {
                    int index = y * width + x;
                    for(int i = 0; i < 4; i++) {
                        if(GetAdjacent(index, i, out _)) {
                            for(int j = 0; j < modules.Count; j++) {
                                if(grid[index].possibilities[j]) {
                                    grid[index].affordances[i, j] = new HashSet<int>();
                                    for(int k = 0; k < modules.Count; k++) {
                                        if(modules[j].ConnectsTo(modules[k], i)) {
                                            grid[index].affordances[i, j].Add(k);
                                        }
                                    }
                                }
                            }
                        }
                    }
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
            modules ??= new List<Module>();
            stack.Clear();
            grid = null;
        }

        public void DrawGizmos() {
            if(grid == null) return;
            Gizmos.color = Color.red;
            for(int y = 0; y < height; y++) {
                for(int x = 0; x < width; x++) {
                    Slot slot = grid[y * width + x];
                    if(slot.solved) continue;
                    for(int i = 0; i < slot.entropy; i++) {
                        Gizmos.DrawCube(slot.transform.position + Vector3.up * i, Vector3.one);
                    }
                }
            }
        }

        bool GetAdjacent(int index, int direction, out int adjacent) {
            adjacent = -1;
            int x = index % width;
            int y = index / width;
            switch(direction) {
                // Down
                case 0:
                    if(y > 0) {
                        adjacent = index - width;
                        return true;
                    }
                    return false;
                // Left
                case 1:
                    if(x > 0) {
                        adjacent = index - 1;
                        return true;
                    }
                    return false;
                // Up
                case 2:
                    if(y < height - 1) {
                        adjacent = index + width;
                        return true;
                    }
                    return false;
                // Right
                case 3:
                    if(x < width - 1) {
                        adjacent = index + 1;
                        return true;
                    }
                    return false;
                default:
                    throw new System.ArgumentException("Invalid direction");
            }
        }
    }
}