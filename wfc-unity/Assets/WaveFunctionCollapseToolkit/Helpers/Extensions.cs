using System.Collections;
using UnityEngine;

namespace Wave {
    public static class Extensions {
        public static int Sample(this float[] weights) {
            float sum = 0;
            for(int i = 0; i < weights.Length; i++)
                sum += weights[i];
            float threshold = Random.Range(0f, 1f) * sum;

            float partialSum = 0;
            for(int i = 0; i < weights.Length; i++) {
                partialSum += weights[i];
                if(partialSum >= threshold)
                    return i;
            }
            return 0;
        }

        public static int Flatten(this (int, int, int) index, WaveGrid grid) {
            (int x, int y, int z) = index;
            return grid.length * grid.width * y + grid.width * z + x;
        }

        public static (int, int, int) Reshape(this int index, WaveGrid grid) {
            int y = index / (grid.length * grid.width);
            index -= y * grid.length * grid.width;
            int z = index / grid.width;
            int x = index % grid.width;
            return (x, y, z);
        }

        public class CoroutineRunner : MonoBehaviour { }
        static CoroutineRunner _coroutineRunner;
        static CoroutineRunner coroutineRunner {
            get {
                if(_coroutineRunner == null)
                    _coroutineRunner = new GameObject("CoroutineRunner").AddComponent<CoroutineRunner>();
                return _coroutineRunner;
            }
        }

        public static Coroutine RunCoroutine(this IEnumerator item) {
            return coroutineRunner.StartCoroutine(item);
        }

        public static void StopCoroutine(this Coroutine coroutine) {
            coroutineRunner.StopCoroutine(coroutine);
        }
    }
}