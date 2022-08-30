using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Wave {
    [System.Serializable]
    public class WaveGrid {
        static readonly int[] OPPOSITE = { 2, 3, 0, 1, 5, 4 };
        static readonly int MAX_ITERATIONS = 10000;
        static readonly int MAX_ATTEMPTS = 10;

        public string name = "WaveGrid";
        public List<Module> modules;
        public int width = 4;
        public int length = 4;
        public int height = 1;
        public float moduleSize = 4f;
        public Vector3 offset;

        int delay => Generator.instance.delay > 0 ? Mathf.CeilToInt(1 / Generator.instance.delay) : -1;

        Transform root;
        Stack<(int, int)> stack;
        Slot[] grid;
        Task task;
        int ticks;
        bool failed;

        public WaveGrid() {
            stack = new Stack<(int, int)>();
        }

        public void Generate() {
            if(task != null && !task.IsCompleted) return;
            if(delay >= 0)
                task = GenerateAsync();
            else
                GenerateSync();
        }

        void GenerateSync() {
            for(int i = 0; i < MAX_ATTEMPTS; i++) {
                failed = false;
                Run();
                if(failed)
                    Logging.Log($"Failed attempt {i}");
                else
                    break;
            }
        }

        async Task GenerateAsync() {
            for(int i = 0; i < MAX_ATTEMPTS; i++) {
                failed = false;
                await RunAsync();
                if(failed)
                    Logging.Log($"Failed attempt {i}");
                else
                    break;
            }
        }

        void Run() {
            Clear();
            InitializeGrid();

            for(int i = 0; i < MAX_ITERATIONS; i++) {
                int index = NextSlot();
                if(index >= 0) {
                    Observe(index);
                } else {
                    return;
                }
            }
        }

        async Task RunAsync() {
            Clear();
            InitializeGrid();

            for(int i = 0; i < MAX_ITERATIONS; i++) {
                int index = NextSlot();
                if(index >= 0) {
                    await CollapseAsync(index);
                } else {
                    return;
                }
            }
        }

        void Propagate(int index, int possibility) {
            if(failed) return;
            if(grid[index].entropy <= 1) {
                failed = true;
                return;
            }

            // Remove from possibilities
            grid[index].RemovePossibility(possibility);

            // Propagate to neighbors
            for(int i = 0; i < 6; i++) {
                if(GetAdjacent(index, i, out int adjacent) && !grid[adjacent].collapsed) {
                    for(int j = 0; j < modules.Count; j++) {
                        if(!grid[adjacent].possibilities[j]) continue;

                        int opposite = OPPOSITE[i];
                        grid[adjacent].affordances[opposite, j].Remove(possibility);
                        if(grid[adjacent].affordances[opposite, j].Count == 0)
                            Propagate(adjacent, j);
                    }
                }
            }

            if(grid[index].entropy == 1)
                grid[index].Collapse();
        }

        async Task PropagateAsync(int index, int possibility) {
            if(failed) return;
            if(grid[index].entropy <= 1) {
                failed = true;
                return;
            }

            // Remove from possibilities
            grid[index].RemovePossibility(possibility);

            if(delay >= 0 && ++ticks >= delay) {
                ticks = 0;
                await Task.Delay(1);
            }

            // Propagate to neighbors
            for(int i = 0; i < 6; i++) {
                if(GetAdjacent(index, i, out int adjacent) && !grid[adjacent].collapsed) {
                    for(int j = 0; j < modules.Count; j++) {
                        if(!grid[adjacent].possibilities[j]) continue;

                        int opposite = OPPOSITE[i];
                        grid[adjacent].affordances[opposite, j].Remove(possibility);
                        if(grid[adjacent].affordances[opposite, j].Count == 0)
                            await PropagateAsync(adjacent, j);
                    }
                }
            }

            if(delay >= 0 && ++ticks >= delay) {
                ticks = 0;
                await Task.Delay(1);
            }

            if(grid[index].entropy == 1)
                grid[index].Collapse();
        }

        int NextSlot() {
            int argmin = -1;
            if(failed) return argmin;
            float min = float.MaxValue;
            for(int i = 0; i < grid.Length; i++) {
                if(!grid[i].collapsed && grid[i].entropy <= min) {
                    float noise = Random.Range(0f, 1f);
                    if(grid[i].entropy + noise < min) {
                        min = grid[i].entropy + noise;
                        argmin = i;
                    }
                }
            }
            return argmin;
        }

        void Observe(int index) {
            int choice = grid[index].weights.Sample();
            for(int i = 0; i < modules.Count; i++) {
                if(grid[index].possibilities[i] != (i == choice))
                    Propagate(index, i);
            }
        }

        async Task CollapseAsync(int index) {
            int choice = grid[index].weights.Sample();
            for(int i = 0; i < modules.Count; i++) {
                if(grid[index].possibilities[i] != (i == choice))
                    await PropagateAsync(index, i);
            }
        }

        void InitializeGrid() {
            root = new GameObject(name).transform;
            root.SetParent(Generator.instance.transform);
            root.localPosition = offset;
            grid = new Slot[width * length * height];
            Vector3 slotOffset = new Vector3(width * moduleSize / 2f, height * moduleSize / 2f, length * moduleSize / 2f);
            for(int y = 0; y < height; y++) {
                for(int z = 0; z < length; z++) {
                    for(int x = 0; x < width; x++) {
                        int index = (x, y, z).Flatten(this);
                        Slot slot = new GameObject($"slot_{x}_{y}_{z}").AddComponent<Slot>();
                        slot.transform.SetParent(root);
                        slot.transform.localPosition = new Vector3(
                            x * moduleSize - slotOffset.x + moduleSize / 2f,
                            y * moduleSize - slotOffset.y + moduleSize / 2f,
                            z * moduleSize - slotOffset.z + moduleSize / 2f
                        );
                        grid[index] = slot;
                        slot.Initialize(this, modules.Count);
                    }
                }
            }
            for(int y = 0; y < height; y++) {
                for(int z = 0; z < length; z++) {
                    for(int x = 0; x < width; x++) {
                        int index = (x, y, z).Flatten(this);
                        for(int i = 0; i < 6; i++) {
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
                for(int z = 0; z < length; z++) {
                    for(int x = 0; x < width; x++) {
                        int index = (x, y, z).Flatten(this);
                        Slot slot = grid[index];
                        if(slot.collapsed) continue;
                        Gizmos.DrawCube(slot.transform.position, Vector3.one * 2f * slot.entropy / modules.Count);
                    }
                }
            }
        }

        bool GetAdjacent(int index, int direction, out int adjacent) {
            adjacent = -1;
            (int x, int y, int z) = index.Reshape(this);
            switch(direction) {
                // Down
                case 0:
                    if(z > 0) {
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
                    if(z < length - 1) {
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
                // Above
                case 4:
                    if(y < height - 1) {
                        adjacent = index + length * width;
                        return true;
                    }
                    return false;
                // Below
                case 5:
                    if(y > 0) {
                        adjacent = index - length * width;
                        return true;
                    }
                    return false;
                default:
                    throw new System.ArgumentException("Invalid direction");
            }
        }
    }
}